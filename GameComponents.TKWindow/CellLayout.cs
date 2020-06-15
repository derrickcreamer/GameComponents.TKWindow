/*Copyright (c) 2020  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;

namespace GameComponents.TKWindow{
	public class CellLayout{
		public PositionFromIndex X;
		public PositionFromIndex Y;
		public PositionFromIndex Z = null; //Z isn't used unless the VBO object has PositionDimensions set to 3.
		public float CellWidth; //in world coords - this value will be used along with the values set by GLWindow.SetWorldUnitsPerScreen to determine vertex positions.
		public float CellHeight;
		public float HorizontalOffset;
		public float VerticalOffset;

		public static CellLayout CreateGrid(Surface s,int cols,int rows,float cell_width = 1.0f,float cell_height = 1.0f,float h_offset = 0.0f,float v_offset = 0.0f,PositionFromIndex z = null){
			CellLayout c = new CellLayout();
			c.CellWidth = cell_width;
			c.CellHeight = cell_height;
			c.HorizontalOffset = h_offset;
			c.VerticalOffset = v_offset;
			c.X = idx => (idx % cols) * cell_width;
			c.Y = idx => (idx / cols) * cell_height;
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
		public static CellLayout CreateIso(Surface s,int cols,int rows,float cell_width,float cell_height,float h_offset,float v_offset,float cell_h_offset,float cell_v_offset,PositionFromIndex z = null,PositionFromIndex elevation = null){
			CellLayout c = new CellLayout();
			c.CellWidth = cell_width;
			c.CellHeight = cell_height;
			c.HorizontalOffset = h_offset;
			c.VerticalOffset = v_offset;
			c.X = idx => (rows - 1 - (idx/cols) + (idx%cols)) * cell_h_offset;
			if(elevation == null){
				c.Y = idx => ((idx/cols) + (idx%cols)) * cell_v_offset;
			}
			else{
				c.Y = idx => ((idx/cols) + (idx%cols)) * cell_v_offset + elevation(idx);
			}
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
		public static CellLayout CreateIsoAtOffset(Surface s,int cols,int rows,int base_start_col,int base_start_row,int base_rows,float cell_width,float cell_height,float h_offset,float v_offset,float cell_h_offset,float cell_v_offset,PositionFromIndex z = null,PositionFromIndex elevation = null){
			CellLayout c = new CellLayout();
			c.CellWidth = cell_width;
			c.CellHeight = cell_height;
			c.HorizontalOffset = h_offset;
			c.VerticalOffset = v_offset;
			c.X = idx => (base_rows - 1 - (idx/cols + base_start_row) + (idx%cols + base_start_col)) * cell_h_offset;
			if(elevation == null){
				c.Y = idx => ((idx/cols + base_start_row) + (idx%cols + base_start_col)) * cell_v_offset;
			}
			else{
				c.Y = idx => ((idx/cols + base_start_row) + (idx%cols + base_start_col)) * cell_v_offset + elevation(idx);
			}
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
		public static CellLayout Create(Surface s,float cell_width,float cell_height,float h_offset,float v_offset,PositionFromIndex x,PositionFromIndex y,PositionFromIndex z = null){
			CellLayout c = new CellLayout();
			c.CellWidth = cell_width;
			c.CellHeight = cell_height;
			c.HorizontalOffset = h_offset;
			c.VerticalOffset = v_offset;
			c.X = x;
			c.Y = y;
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
	}
}
