/*Copyright (c) 2020  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Drawing;

namespace GameComponents.TKWindow{
	public interface IResizeRules{
		Size CalculateResize(Size p);
	}
	public class ResizeRules : IResizeRules{
		public enum RatioType { None, Exact, Range };

		public bool Constant;
		//todo xml notes for all:
		public int SnapWidth, SnapHeight;
		public RatioType RatioRequirement;
		public int RatioWidth, RatioHeight;
		// todo note that this is width over height, so 16.0f / 9.0f will approximate 16:9
		public float RatioMin, RatioMax;
		public int MinWidth, MinHeight;
		public int MaxWidth, MaxHeight;

		public Size CalculateResize(Size p) => CalculateResize(p.Width, p.Height);
		public Size CalculateResize(int width, int height){
			if(Constant){
				width = SnapWidth > 0? SnapWidth : width;
				height = SnapHeight > 0? SnapHeight : height;
				return new Size(width, height);
			}
			// MAX
			if(MaxWidth > 0 && MaxWidth < width) width = MaxWidth;
			if(MaxHeight > 0 && MaxHeight < height) height = MaxHeight;
			// RATIO
			if(RatioRequirement == RatioType.Exact){
				int widthMultiple = width / RatioWidth;
				int heightMultiple = height / RatioHeight;
				int minMultiple = Math.Min(widthMultiple, heightMultiple);
				if(minMultiple < 1) minMultiple = 1;
				width = RatioWidth * minMultiple;
				height = RatioHeight * minMultiple;
			}
			else if(RatioRequirement == RatioType.Range){
				float ratio = (float)width / (float)height;
				if(ratio < RatioMin){
					// Since we only want to decrease size here, if the ratio is too low,
					// then we divided the width X by a height Y that was too large.
					// Divide the width by the min ratio and then floor it for a new Y.
					float floatHeight = (float)width / RatioMin;
					height = (int)floatHeight;
				}
				else if(ratio > RatioMax){
					// But if the ratio is too high, then it's the width that was too large.
					// Multiply the height by the max ratio and then floor it for a new X.
					float floatWidth = (float)height * RatioMax;
					width = (int)floatWidth;
				}
			}
			// SNAP
			if(SnapWidth > 1) width -= width % SnapWidth;
			if(SnapHeight > 1) height -= height % SnapHeight;
			// MIN
			if(MinWidth > 0 && MinWidth > width) width = MinWidth;
			if(MinHeight > 0 && MinHeight > height) height = MinHeight;
			return new Size(width, height);
		}
	}
}
