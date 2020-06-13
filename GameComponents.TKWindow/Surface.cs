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

namespace GameComponents.TKWindow{
	public delegate float PositionFromIndex(int idx);
	public delegate void SurfaceUpdateMethod(SurfaceDefaults defaults);
	public class SurfaceDefaults{
		public List<int> positions = null;
		public List<int> layouts = null;
		public List<int> sprites = null;
		public List<int> sprite_types = null;
		public List<float>[] other_data = null;

		public int single_position = -1;
		public int single_layout = -1;
		public int single_sprite = -1;
		public int single_sprite_type = -1;
		public List<float>[] single_other_data = null;

		public int fill_count = -1;
		public SurfaceDefaults(){}
		public SurfaceDefaults(SurfaceDefaults other){
			if(other.positions != null){
				positions = new List<int>(other.positions);
			}
			if(other.layouts != null){
				layouts = new List<int>(other.layouts);
			}
			if(other.sprites != null){
				sprites = new List<int>(other.sprites);
			}
			if(other.sprite_types != null){
				sprite_types = new List<int>(other.sprite_types);
			}
			if(other.other_data != null){
				other_data = new List<float>[other.other_data.GetLength(0)];
				int idx = 0;
				foreach(List<float> l in other.other_data){
					other_data[idx] = new List<float>(l);
					++idx;
				}
			}
			single_position = other.single_position;
			single_layout = other.single_layout;
			single_sprite = other.single_sprite;
			single_sprite_type = other.single_sprite_type;
			single_other_data = other.single_other_data;
			fill_count = other.fill_count;
		}
		public void FillValues(bool fill_positions,bool fill_other_data){
			if(fill_count <= 0){
				return;
			}
			if(fill_positions){
				if(single_position != -1){ //if this value is -1, nothing can be added anyway.
					if(positions == null){
						positions = new List<int>();
					}
					while(positions.Count < fill_count){
						positions.Add(single_position);
					}
				}
				if(single_layout != -1){
					if(layouts == null){
						layouts = new List<int>();
					}
					while(layouts.Count < fill_count){
						layouts.Add(single_layout);
					}
				}
			}
			if(fill_other_data){
				if(single_sprite != -1){
					if(sprites == null){
						sprites = new List<int>();
					}
					while(sprites.Count < fill_count){
						sprites.Add(single_sprite);
					}
				}
				if(single_sprite_type != -1){
					if(sprite_types == null){
						sprite_types = new List<int>();
					}
					while(sprite_types.Count < fill_count){
						sprite_types.Add(single_sprite_type);
					}
				}
				if(single_other_data != null){
					if(other_data == null){
						other_data = new List<float>[single_other_data.GetLength(0)];
						for(int i=0;i<other_data.GetLength(0);++i){
							other_data[i] = new List<float>();
						}
					}
					int idx = 0;
					foreach(List<float> l in other_data){
						while(l.Count < fill_count * single_other_data[idx].Count){
							foreach(float f in single_other_data[idx]){
								l.Add(f);
							}
						}
						++idx;
					}
				}
			}
		}
	}
	public class Surface{
		public GLWindow window;
		public VBO vbo;
		public Texture texture;
		public Shader shader;
		public List<CellLayout> layouts = new List<CellLayout>();
		protected SurfaceDefaults defaults = new SurfaceDefaults();
		public float raw_x_offset = 0.0f;
		public float raw_y_offset = 0.0f; //todo: should be properties
		private int x_offset_px;
		private int y_offset_px;
		public bool UseDepthBuffer = false;
		public bool Disabled = false;
		public SurfaceUpdateMethod UpdateMethod = null;
		public SurfaceUpdateMethod UpdatePositionsOnlyMethod = null;
		public SurfaceUpdateMethod UpdateOtherDataOnlyMethod = null;
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
			raw_y_offset += (float)(dy_offset_px * 2) / (float)window.Viewport.Height;
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
		public void SetEasyLayoutCounts(params int[] counts_per_layout){
			if(counts_per_layout.GetLength(0) != layouts.Count){
				throw new ArgumentException("SetEasyLayoutCounts: Number of arguments (" + counts_per_layout.GetLength(0) + ") must match number of layouts (" + layouts.Count + ").");
			}
			defaults.positions = new List<int>(); //this method creates the default lists used by Update()
			defaults.layouts = new List<int>();
			int idx = 0;
			foreach(int count in counts_per_layout){
				for(int i=0;i<count;++i){
					defaults.positions.Add(i);
					defaults.layouts.Add(idx);
				}
				++idx;
			}
			defaults.fill_count = defaults.positions.Count;
		}
		public void SetDefaultPosition(int position_index){
			defaults.single_position = position_index;
		}
		public void SetDefaultLayout(int layout_index){
			defaults.single_layout = layout_index;
		}
		public void SetDefaultSprite(int sprite_index){
			defaults.single_sprite = sprite_index;
		}
		public void SetDefaultSpriteType(int sprite_type_index){
			defaults.single_sprite_type = sprite_type_index;
		}
		public void SetDefaultOtherData(params List<float>[] other_data){
			defaults.single_other_data = other_data;
		}
		/*public void SetDefaults(IList<int> positions,IList<int> layouts,IList<int> sprites,IList<int> sprite_types,params IList<float>[] other_data){
			defaults = new SurfaceDefaults();
			if(positions != null){
				defaults.positions = new List<int>(positions);
			}
			if(layouts != null){
				defaults.layouts = new List<int>(layouts);
			}
			if(sprites != null){
				defaults.sprites = new List<int>(sprites);
			}
			if(sprite_types != null){
				defaults.sprite_types = new List<int>(sprite_types);
			}
			if(other_data != null){
				defaults.other_data = new List<float>[other_data.GetLength(0)];
				int idx = 0;
				foreach(List<float> l in other_data){
					other_data[idx] = new List<float>(l);
					++idx;
				}
			}
		}*/
		public void SetDefaults(SurfaceDefaults new_defaults){
			defaults = new_defaults;
		}
		public void DefaultUpdate(){
			SurfaceDefaults d = new SurfaceDefaults(defaults);//todo check these for sanity
			d.FillValues(true,true);
			window.UpdatePositionVertexArray(this,d.positions,d.layouts);
			window.UpdateOtherVertexArray(this,-1,d.sprites,0,d.sprite_types,d.other_data);
		}
		public void DefaultUpdatePositions(){
			SurfaceDefaults d = new SurfaceDefaults(defaults);
			d.FillValues(true,false);
			window.UpdatePositionVertexArray(this,d.positions,d.layouts);
		}
		public void DefaultUpdateOtherData(){
			SurfaceDefaults d = new SurfaceDefaults(defaults);
			d.FillValues(false,true);
			window.UpdateOtherVertexArray(this,-1,d.sprites,0,d.sprite_types,d.other_data);
		}
		public void Update(){
			if(UpdateMethod != null){
				SurfaceDefaults d = new SurfaceDefaults(defaults);
				UpdateMethod(d);
				d.FillValues(true,true);
				window.UpdatePositionVertexArray(this,d.positions,d.layouts);
				window.UpdateOtherVertexArray(this,-1,d.sprites,0,d.sprite_types,d.other_data);
			}
		}
		public void UpdatePositionsOnly(){
			if(UpdatePositionsOnlyMethod != null){
				SurfaceDefaults d = new SurfaceDefaults(defaults);
				UpdatePositionsOnlyMethod(d);
				d.FillValues(true,false);
				window.UpdatePositionVertexArray(this,d.positions,d.layouts);
			}
		}
		public void UpdateOtherDataOnly(){
			if(UpdateOtherDataOnlyMethod != null){
				SurfaceDefaults d = new SurfaceDefaults(defaults);
				UpdateOtherDataOnlyMethod(d);
				d.FillValues(false,true);
				window.UpdateOtherVertexArray(this,-1,d.sprites,0,d.sprite_types,d.other_data);
			}
		}
	}
}
