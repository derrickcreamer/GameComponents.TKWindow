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
		public int CellWidthPx; //in pixels
		public int CellHeightPx;
		public int HorizontalOffsetPx;
		public int VerticalOffsetPx;

		public static CellLayout CreateGrid(Surface s,int cols,int rows,int cell_width_px,int cell_height_px,int h_offset_px,int v_offset_px,PositionFromIndex z = null){
			CellLayout c = new CellLayout();
			c.CellWidthPx = cell_width_px;
			c.CellHeightPx = cell_height_px;
			c.HorizontalOffsetPx = h_offset_px;
			c.VerticalOffsetPx = v_offset_px;
			c.X = idx => (idx % cols) * c.CellWidthPx; //todo, does this closure need to access the object each time? what if I used a local var here?
			c.Y = idx => (idx / cols) * c.CellHeightPx;
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
		public static CellLayout CreateIso(Surface s,int cols,int rows,int cell_width_px,int cell_height_px,int h_offset_px,int v_offset_px,int cell_h_offset_px,int cell_v_offset_px,PositionFromIndex z = null,PositionFromIndex elevation = null){
			CellLayout c = new CellLayout();
			c.CellWidthPx = cell_width_px;
			c.CellHeightPx = cell_height_px;
			c.HorizontalOffsetPx = h_offset_px;
			c.VerticalOffsetPx = v_offset_px;
			c.X = idx => (rows - 1 - (idx/cols) + (idx%cols)) * cell_h_offset_px;
			if(elevation == null){
				c.Y = idx => ((idx/cols) + (idx%cols)) * cell_v_offset_px;
			}
			else{
				c.Y = idx => ((idx/cols) + (idx%cols)) * cell_v_offset_px + elevation(idx);
			}
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
		public static CellLayout CreateIsoAtOffset(Surface s,int cols,int rows,int base_start_col,int base_start_row,int base_rows,int cell_width_px,int cell_height_px,int h_offset_px,int v_offset_px,int cell_h_offset_px,int cell_v_offset_px,PositionFromIndex z = null,PositionFromIndex elevation = null){
			CellLayout c = new CellLayout();
			c.CellWidthPx = cell_width_px;
			c.CellHeightPx = cell_height_px;
			c.HorizontalOffsetPx = h_offset_px;
			c.VerticalOffsetPx = v_offset_px;
			c.X = idx => (base_rows - 1 - (idx/cols + base_start_row) + (idx%cols + base_start_col)) * cell_h_offset_px;
			if(elevation == null){
				c.Y = idx => ((idx/cols + base_start_row) + (idx%cols + base_start_col)) * cell_v_offset_px;
			}
			else{
				c.Y = idx => ((idx/cols + base_start_row) + (idx%cols + base_start_col)) * cell_v_offset_px + elevation(idx);
			}
			c.Z = z;
			if(s != null){
				s.layouts.Add(c);
			}
			return c;
		}
		public static CellLayout Create(Surface s,int cell_width_px,int cell_height_px,int h_offset_px,int v_offset_px,PositionFromIndex x,PositionFromIndex y,PositionFromIndex z = null){
			CellLayout c = new CellLayout();
			c.CellWidthPx = cell_width_px;
			c.CellHeightPx = cell_height_px;
			c.HorizontalOffsetPx = h_offset_px;
			c.VerticalOffsetPx = v_offset_px;
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
