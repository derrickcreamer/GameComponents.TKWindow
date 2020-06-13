/*Copyright (c) 2014-2020  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace GameComponents.TKWindow{
	public class GLWindow : GameWindow{ public static List<string> log = new List<string>();
		public List<Surface> Surfaces = new List<Surface>();

		// todo xml
		public IResizeRules WindowSizeRules;
		public IResizeRules ViewportSizeRules;
		// todo xml, controls behavior when window size is less than the minimum viewport size
		public bool NoShrinkToFit;
		public bool NoClose;

		protected bool Resizing;

		protected FrameEventArgs render_args = new FrameEventArgs();
		protected Dictionary<Key,bool> key_down = new Dictionary<Key,bool>(); // todo - decide if these few input things should be here or not. Are they useful, or is it going to be necessary to inherit from this class anyway?
		protected bool DepthTestEnabled;
		protected int LastShaderID = -1;

		public Action FinalResize; //todo name?
		protected Rectangle internalViewport;
		public Rectangle Viewport{ get{ return internalViewport; }
			set{
				internalViewport = value;
				GL.Viewport(value);
			}
		}
		public void SetViewport(int x,int y,int width,int height){
			internalViewport = new Rectangle(x,y,width,height);
			GL.Viewport(x,y,width,height);
		}

		public GLWindow(int w,int h,string title) : base(w,h,GraphicsMode.Default,title){log.Add($"starting size {ClientRectangle.Width} by {ClientRectangle.Height}");
			VSync = VSyncMode.On;
			GL.ClearColor(0.0f,0.0f,0.0f,0.0f);
			GL.EnableVertexAttribArray(0); //these 2 attrib arrays are always on, for position and texcoords.
			GL.EnableVertexAttribArray(1);
			KeyDown += KeyDownHandler;
			KeyUp += KeyUpHandler;
			//Keyboard.KeyRepeat = true;
			internalViewport = new Rectangle(0,0,w,h);
			FinalResize = DefaultHandleResize;
		}
		public bool FullScreen => this.WindowState == WindowState.Fullscreen;
		protected virtual void KeyDownHandler(object sender,KeyboardKeyEventArgs args){ // todo, keeping or not? see input note above.
			key_down[args.Key] = true;
		}
		protected virtual void KeyUpHandler(object sender,KeyboardKeyEventArgs args){
			key_down[args.Key] = false;
		}
		public bool KeyIsDown(Key key){
			bool value;
			key_down.TryGetValue(key,out value);
			return value;
		}
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e){
			e.Cancel = NoClose;
			base.OnClosing(e);
		}
		protected override void OnFocusedChanged(EventArgs e){
			base.OnFocusedChanged(e);
			if(Focused){
				key_down[Key.AltLeft] = false; //i could simply reset the whole dictionary, too...
				key_down[Key.AltRight] = false;
				key_down[Key.ShiftLeft] = false;
				key_down[Key.ShiftRight] = false;
				key_down[Key.ControlLeft] = false;
				key_down[Key.ControlRight] = false; // todo, any more (mod4?), or should I really clear the dictionary?
			}
		}
		protected override void OnResize(EventArgs e){log.Add($"OnResize called. Resizing is {Resizing}. Size {ClientRectangle.Width} by {ClientRectangle.Height}");
			Resizing = true;
		}
		protected override void OnWindowStateChanged(EventArgs e){
			Resizing = true;
		}
		public void DefaultHandleResize(){
			Size windowSize = ClientSize;
			if(!FullScreen && WindowSizeRules != null){
				Size newSize = WindowSizeRules.CalculateResize(windowSize);
				if(newSize != windowSize) ClientSize = newSize;
				windowSize = ClientSize;
			}
			Size viewportSize = ViewportSizeRules?.CalculateResize(windowSize) ?? windowSize;
			if(!NoShrinkToFit){
				if(viewportSize.Width > windowSize.Width) viewportSize.Width = windowSize.Width;
				if(viewportSize.Height > windowSize.Height) viewportSize.Height = windowSize.Height;
			}
			int x = (windowSize.Width - viewportSize.Width) / 2;
			int y = (windowSize.Height - viewportSize.Height) / 2;
			SetViewport(x, y, viewportSize.Width, viewportSize.Height);
		}
		public void ToggleFullScreen(){ ToggleFullScreen(!FullScreen); }
		public void ToggleFullScreen(bool fullScreen){
			if(fullScreen){
				WindowState = WindowState.Fullscreen;
			}
			else{
				WindowState = WindowState.Normal;
			}
			Resizing = true;
		}
		public bool WindowUpdate(){
			ProcessEvents();
			if(IsExiting){
				return false;
			}
			if(Resizing){log.Add($"WindowUpdate while Resizing. Size {ClientRectangle.Width} by {ClientRectangle.Height}");
				FinalResize?.Invoke();
				Resizing = false;log.Add($"WindowUpdate Resize finished. Size {ClientRectangle.Width} by {ClientRectangle.Height}");
			}
			DrawSurfaces();
			return true;
		}
		public void DrawSurfaces(){
			base.OnRenderFrame(render_args);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			foreach(Surface s in Surfaces){
				if(!s.Disabled){
					if(DepthTestEnabled != s.UseDepthBuffer){
						if(s.UseDepthBuffer){
							GL.Enable(EnableCap.DepthTest);
						}
						else{
							GL.Disable(EnableCap.DepthTest);
						}
						DepthTestEnabled = s.UseDepthBuffer;
					}
					if(LastShaderID != s.shader.ShaderProgramID){
						GL.UseProgram(s.shader.ShaderProgramID);
						LastShaderID = s.shader.ShaderProgramID;
					}
					GL.Uniform2(s.shader.OffsetUniformLocation,s.raw_x_offset,s.raw_y_offset);
					GL.Uniform1(s.shader.TextureUniformLocation,s.texture.TextureIndex);
					GL.BindBuffer(BufferTarget.ElementArrayBuffer,s.vbo.ElementArrayBufferID);
					GL.BindBuffer(BufferTarget.ArrayBuffer,s.vbo.PositionArrayBufferID);
					GL.VertexAttribPointer(0,s.vbo.PositionDimensions,VertexAttribPointerType.Float,false,sizeof(float)*s.vbo.PositionDimensions,new IntPtr(0)); //position
					GL.BindBuffer(BufferTarget.ArrayBuffer,s.vbo.OtherArrayBufferID);
					int stride = sizeof(float) * s.vbo.VertexAttribs.TotalSize;
					GL.VertexAttribPointer(1,s.vbo.VertexAttribs.Size[0],VertexAttribPointerType.Float,false,stride,new IntPtr(0)); //texcoords
					int total_of_previous_attribs = s.vbo.VertexAttribs.Size[0];
					for(int i=1;i<s.vbo.VertexAttribs.Size.Length;++i){
						GL.EnableVertexAttribArray(i+1); //i+1 because 0 and 1 are always on (for position & texcoords)
						GL.VertexAttribPointer(i+1,s.vbo.VertexAttribs.Size[i],VertexAttribPointerType.Float,false,stride,new IntPtr(sizeof(float)*total_of_previous_attribs));
						total_of_previous_attribs += s.vbo.VertexAttribs.Size[i];
					}
					GL.DrawElements(PrimitiveType.Triangles,s.vbo.NumElements,DrawElementsType.UnsignedInt,IntPtr.Zero);
					for(int i=1;i<s.vbo.VertexAttribs.Size.Length;++i){
						GL.DisableVertexAttribArray(i+1);
					}
				}
			}
			SwapBuffers();
		}
		/*public static void ReplaceTexture(int texture_unit,string filename){ //binds a texture to the given texture unit, replacing the texture that's already there
			if(String.IsNullOrEmpty(filename)){
				throw new ArgumentException(filename);
			}
			GL.ActiveTexture(TextureUnit.Texture0 + texture_unit);
			int id = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D,id);
			Bitmap bmp = new Bitmap(filename);
			BitmapData bmp_data = bmp.LockBits(new Rectangle(0,0,bmp.Width,bmp.Height),ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,bmp_data.Width,bmp_data.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,bmp_data.Scan0);
			bmp.UnlockBits(bmp_data);
			GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
		}*/
		public void UpdatePositionVertexArray(Surface s,IList<int> index_list,IList<int> layout_list = null){
			UpdatePositionVertexArray(s,-1,index_list,layout_list);
		}
		public void UpdatePositionVertexArray(Surface s,int start_index,IList<int> index_list,IList<int> layout_list = null){
			int count = index_list.Count;
			if(layout_list == null){
				layout_list = new int[count]; //if not supplied, assume layout 0.
			}
			float[] values = new float[count * 4 * s.vbo.PositionDimensions]; //2 or 3 dimensions for 4 vertices for each tile
			int[] indices = null;
			if(start_index < 0 && s.vbo.NumElements != count * 6){
				indices = new int[count * 6];
				s.vbo.NumElements = count * 6;
			}
			else{
				if(start_index >= 0){
					if((start_index + count) * 6 > s.vbo.NumElements && s.vbo.NumElements > 0){
						throw new ArgumentException("Error: start_index + count is bigger than VBO size. To always replace the previous data, set start_index to -1.");
					} //todo: I could also just ignore the start_index if there's too much data.
					if((start_index + count) * 4 * s.vbo.PositionDimensions > s.vbo.PositionDataSize && s.vbo.PositionDataSize > 0){
						throw new ArgumentException("Error: (start_index + count) * total_attrib_size is bigger than VBO size. To always replace the previous data, set start_index to -1.");
					}
				}
			}
			float width_ratio = 2.0f / (float)Viewport.Width;
			float height_ratio = 2.0f / (float)Viewport.Height;
			int current_total = 0;
			foreach(int i in index_list){
				float x_offset = (float)s.layouts[layout_list[current_total]].HorizontalOffsetPx;
				float y_offset = (float)s.layouts[layout_list[current_total]].VerticalOffsetPx;
				float x_w = (float)s.layouts[layout_list[current_total]].CellWidthPx;
				float y_h = (float)s.layouts[layout_list[current_total]].CellHeightPx;
				float cellx = s.layouts[layout_list[current_total]].X(i) + x_offset;
				float celly = s.layouts[layout_list[current_total]].Y(i) + y_offset;
				float x = cellx * width_ratio - 1.0f;
				float y = celly * height_ratio - 1.0f;
				float x_plus1 = (cellx + x_w) * width_ratio - 1.0f;
				float y_plus1 = (celly + y_h) * height_ratio - 1.0f;

				int N = s.vbo.PositionDimensions;
				int idxN = current_total * 4 * N;

				values[idxN] = x; //the 4 corners, flipped so it works with the inverted Y axis
				values[idxN + 1] = y_plus1;
				values[idxN + N] = x;
				values[idxN + N + 1] = y;
				values[idxN + N*2] = x_plus1;
				values[idxN + N*2 + 1] = y;
				values[idxN + N*3] = x_plus1;
				values[idxN + N*3 + 1] = y_plus1;
				if(N == 3){
					float z = s.layouts[layout_list[current_total]].Z(i);
					values[idxN + 2] = z;
					values[idxN + N + 2] = z;
					values[idxN + N*2 + 2] = z;
					values[idxN + N*3 + 2] = z;
				}

				if(indices != null){
					int idx4 = current_total * 4;
					int idx6 = current_total * 6;
					indices[idx6] = idx4;
					indices[idx6 + 1] = idx4 + 1;
					indices[idx6 + 2] = idx4 + 2;
					indices[idx6 + 3] = idx4;
					indices[idx6 + 4] = idx4 + 2;
					indices[idx6 + 5] = idx4 + 3;
				}
				current_total++;
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer,s.vbo.PositionArrayBufferID);
			if((start_index < 0 && s.vbo.PositionDataSize != values.Length) || s.vbo.PositionDataSize == 0){
				GL.BufferData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float)*values.Length),values,BufferUsageHint.StreamDraw);
				s.vbo.PositionDataSize = values.Length;
			}
			else{
				int offset = start_index;
				if(offset < 0){
					offset = 0;
				}
				GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float) * 4 * s.vbo.PositionDimensions * offset),new IntPtr(sizeof(float) * values.Length),values);
				//GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(0),new IntPtr(sizeof(float) * values.Length),values);
			}
			if(indices != null){
				GL.BindBuffer(BufferTarget.ElementArrayBuffer,s.vbo.ElementArrayBufferID);
				GL.BufferData(BufferTarget.ElementArrayBuffer,new IntPtr(sizeof(int)*indices.Length),indices,BufferUsageHint.StaticDraw);
			}
		}
		public void UpdatePositionSingleVertex(Surface s,int index,int layout = 0){
			float[] values = new float[4 * s.vbo.PositionDimensions]; //2 or 3 dimensions for 4 vertices
			float width_ratio = 2.0f / (float)Viewport.Width;
			float height_ratio = 2.0f / (float)Viewport.Height;
			float x_offset = (float)s.layouts[layout].HorizontalOffsetPx;
			float y_offset = (float)s.layouts[layout].VerticalOffsetPx;
			float x_w = (float)s.layouts[layout].CellWidthPx;
			float y_h = (float)s.layouts[layout].CellHeightPx;
			float cellx = s.layouts[layout].X(index) + x_offset;
			float celly = s.layouts[layout].Y(index) + y_offset;
			float x = cellx * width_ratio - 1.0f;
			float y = celly * height_ratio - 1.0f;
			float x_plus1 = (cellx + x_w) * width_ratio - 1.0f;
			float y_plus1 = (celly + y_h) * height_ratio - 1.0f;

			int N = s.vbo.PositionDimensions;

			values[0] = x; //the 4 corners, flipped so it works with the inverted Y axis
			values[1] = y_plus1;
			values[N] = x;
			values[N + 1] = y;
			values[N*2] = x_plus1;
			values[N*2 + 1] = y;
			values[N*3] = x_plus1;
			values[N*3 + 1] = y_plus1;
			if(N == 3){
				float z = s.layouts[layout].Z(index);
				values[2] = z;
				values[N + 2] = z;
				values[N*2 + 2] = z;
				values[N*3 + 2] = z;
			}

			GL.BindBuffer(BufferTarget.ArrayBuffer,s.vbo.PositionArrayBufferID);
			GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float) * 4 * s.vbo.PositionDimensions * index),new IntPtr(sizeof(float) * values.Length),values);
		}
		public void UpdateOtherVertexArray(Surface s,IList<int> sprite_index,params IList<float>[] vertex_attributes){
			UpdateOtherVertexArray(s,-1,sprite_index,new int[sprite_index.Count],vertex_attributes); //default to sprite type 0.
		} //should I add more overloads here?
		public void UpdateOtherVertexArray(Surface s,int start_index,IList<int> sprite_index,IList<int> sprite_type,params IList<float>[] vertex_attributes){
			int count = sprite_index.Count;
			int a = s.vbo.VertexAttribs.TotalSize;
			int a4 = a * 4;
			if(start_index >= 0 && (start_index + count) * a4 > s.vbo.OtherDataSize && s.vbo.OtherDataSize > 0){
				throw new ArgumentException("Error: (start_index + count) * total_attrib_size is bigger than VBO size. To always replace the previous data, set start_index to -1.");
			}
			SpriteType sprite = s.texture.Sprite[0];
			float[] all_values = new float[count * a4];
			for(int i=0;i<count;++i){
				if(sprite_type != null) sprite = s.texture.Sprite[sprite_type[i]];
				float tex_start_x = sprite.X(sprite_index[i]); //todo... have a way to calculate X and Y for a specific index range and never call these delegates.
				float tex_start_y = sprite.Y(sprite_index[i]);
				float tex_end_x = tex_start_x + sprite.SpriteWidth;
				float tex_end_y = tex_start_y + sprite.SpriteHeight;
				int ia4 = i*a4;
				all_values[ia4] = tex_start_x; //the 4 corners, texcoords:
				all_values[ia4 + 1] = tex_end_y;
				all_values[ia4 + a] = tex_start_x;
				all_values[ia4 + a + 1] = tex_start_y;
				all_values[ia4 + a*2] = tex_end_x;
				all_values[ia4 + a*2 + 1] = tex_start_y;
				all_values[ia4 + a*3] = tex_end_x;
				all_values[ia4 + a*3 + 1] = tex_end_y;
				int prev_total = 2;
				for(int g=1;g<s.vbo.VertexAttribs.Size.Length;++g){ //starting at 1 because texcoords are already done
					int attrib_size = s.vbo.VertexAttribs.Size[g];
					for(int k=0;k<attrib_size;++k){
						float attrib = vertex_attributes[g-1][k + i*attrib_size]; // -1 because the vertex_attributes array doesn't contain texcoords here in the update method.
						all_values[ia4 + prev_total + k] = attrib;
						all_values[ia4 + prev_total + k + a] = attrib;
						all_values[ia4 + prev_total + k + a*2] = attrib;
						all_values[ia4 + prev_total + k + a*3] = attrib;
					}
					prev_total += attrib_size;
				}
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer,s.vbo.OtherArrayBufferID);
			if((start_index < 0 && s.vbo.OtherDataSize != a4 * count) || s.vbo.OtherDataSize == 0){
				GL.BufferData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float) * a4 * count),all_values,BufferUsageHint.StreamDraw);
				s.vbo.OtherDataSize = a4 * count;
			}
			else{
				int offset = start_index;
				if(offset < 0){
					offset = 0;
				}
				GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float) * a4 * offset),new IntPtr(sizeof(float) * a4 * count),all_values);
				//GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(0),new IntPtr(sizeof(float) * a4 * count),all_values);
			}
		}
		public void UpdateOtherSingleVertex(Surface s,int index,int sprite_index,int sprite_type,params IList<float>[] vertex_attributes){
			int a = s.vbo.VertexAttribs.TotalSize;
			int a4 = a * 4;
			float[] values = new float[a4];
			SpriteType sprite = s.texture.Sprite[sprite_type];
			float tex_start_x = sprite.X(sprite_index);
			float tex_start_y = sprite.Y(sprite_index);
			float tex_end_x = tex_start_x + sprite.SpriteWidth;
			float tex_end_y = tex_start_y + sprite.SpriteHeight;
			values[0] = tex_start_x; //the 4 corners, texcoords:
			values[1] = tex_end_y;
			values[a] = tex_start_x;
			values[a + 1] = tex_start_y;
			values[a*2] = tex_end_x;
			values[a*2 + 1] = tex_start_y;
			values[a*3] = tex_end_x;
			values[a*3 + 1] = tex_end_y;
			int prev_total = 2;
			for(int g=1;g<s.vbo.VertexAttribs.Size.Length;++g){ //starting at 1 because texcoords are already done
				int attrib_size = s.vbo.VertexAttribs.Size[g];
				for(int k=0;k<attrib_size;++k){
					float attrib = vertex_attributes[g-1][k]; // -1 because the vertex_attributes array doesn't contain texcoords here in the update method.
					values[prev_total + k] = attrib;
					values[prev_total + k + a] = attrib;
					values[prev_total + k + a*2] = attrib;
					values[prev_total + k + a*3] = attrib;
				}
				prev_total += attrib_size;
			}
			GL.BindBuffer(BufferTarget.ArrayBuffer,s.vbo.OtherArrayBufferID);
			GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(sizeof(float) * a4 * index),new IntPtr(sizeof(float) * a4),values);
		}
	}
}
