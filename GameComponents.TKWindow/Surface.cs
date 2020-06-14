/*Copyright (c) 2020  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameComponents.TKWindow{
	public delegate float PositionFromIndex(int idx);
	public class Surface{
		public GLWindow window;
		public VBO vbo;
		public Texture texture;
		public Shader shader;
		public List<CellLayout> layouts = new List<CellLayout>();
		public float raw_x_offset = 0.0f;
		public float raw_y_offset = 0.0f; //todo: should be properties
		private int x_offset_px;
		private int y_offset_px;
		public bool UseDepthBuffer = false;
		public bool Disabled = false;
		protected Surface(){}
		public static Surface Create(GLWindow window_,string texture_filename,params int[] vertex_attrib_counts){
			return Create(window_,texture_filename,false,ShaderCollection.DefaultFS(),false,vertex_attrib_counts);
		}
		public static Surface Create(GLWindow window_,string texture_filename,bool loadTextureFromEmbeddedResource,string frag_shader,bool has_depth,params int[] vertex_attrib_counts){
			Surface s = new Surface();
			s.window = window_;
			int dims = has_depth? 3 : 2;
			s.UseDepthBuffer = has_depth;
			VertexAttributes attribs = VertexAttributes.Create(vertex_attrib_counts);
			s.vbo = VBO.Create(dims,attribs);
			s.texture = Texture.Create(texture_filename,null,loadTextureFromEmbeddedResource);
			s.shader = Shader.Create(frag_shader);
			if(window_ != null){
				window_.Surfaces.Add(s);
			}
			return s;
		}
		public void RemoveFromWindow(){
			if(window != null){
				window.Surfaces.Remove(this);
			}
		}
		public void SetOffsetInPixels(int x_offset_px,int y_offset_px){
			this.x_offset_px = x_offset_px;
			this.y_offset_px = y_offset_px;
			raw_x_offset = (float)(x_offset_px * 2) / (float)window.Viewport.Width;
			raw_y_offset = (float)(y_offset_px * 2) / (float)window.Viewport.Height;
		}
		public void ChangeOffsetInPixels(int dx_offset_px,int dy_offset_px){
			x_offset_px += dx_offset_px;
			y_offset_px += dy_offset_px;
			raw_x_offset += (float)(dx_offset_px * 2) / (float)window.Viewport.Width;
			raw_y_offset += (float)(dy_offset_px * 2) / (float)window.Viewport.Height; //todo check all this offset stuff to make sure it's alright
		}
		//public void XOffsetPx(){ return x_offset_px; }
		//public void YOffsetPx(){ return y_offset_px; }
		public int TotalXOffsetPx(CellLayout layout){
			return x_offset_px + layout.HorizontalOffsetPx;
		}
		public int TotalXOffsetPx(){
			return x_offset_px + layouts[0].HorizontalOffsetPx;
		}
		public int TotalYOffsetPx(CellLayout layout){
			return y_offset_px + layout.VerticalOffsetPx;
		}
		public int TotalYOffsetPx(){
			return y_offset_px + layouts[0].VerticalOffsetPx;
		}
		public void InitializePositions(params int[] countByLayout){
			if(countByLayout.Length == 1){
				window.UpdatePositionVertexArray(this, Enumerable.Range(0, countByLayout[0]).ToArray());
			}
			else{
				List<int> indexList = new List<int>();
				List<int> layoutList = new List<int>();
				for(int i=0;i<countByLayout.Length;++i){
					indexList.AddRange(Enumerable.Range(0, countByLayout[i]));
					layoutList.AddRange(Enumerable.Repeat(i, countByLayout[i]));
				}
				window.UpdatePositionVertexArray(this, indexList, layout_list: layoutList);
			}
		}
		public void InitializeOtherDataForSingleLayout(int count, int spriteType, int spriteIndex, params IList<float>[] otherData){
			float[][] resultData = new float[otherData.Length][];
			for(int n=0;n<otherData.Length;++n){
				IList<float> dataArray = otherData[n];
				int arrayCount = dataArray.Count;
				resultData[n] = new float[count * arrayCount];
				for(int i=0;i<count;++i){
					int offset = i * arrayCount;
					for(int indexInData=0;indexInData<arrayCount;++indexInData){
						resultData[n][offset + indexInData] = dataArray[indexInData];
					}
				}
			}
			window.UpdateOtherVertexArray(this, Enumerable.Repeat(spriteIndex, count).ToArray(), resultData, single_sprite_type: spriteType);
		}
	}
}
