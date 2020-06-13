/*Copyright (c) 2020  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using OpenTK.Graphics.OpenGL;

namespace GameComponents.TKWindow{
	public class VBO{
		public int PositionArrayBufferID;
		public int OtherArrayBufferID;
		public int ElementArrayBufferID;
		public VertexAttributes VertexAttribs;
		public int PositionDimensions = 2; //this value controls whether 2 or 3 values are stored for position.
		public int NumElements = 0;
		public int PositionDataSize = 0; //these 2 values track the number of float values in the VBOs.
		public int OtherDataSize = 0;
		protected VBO(){}
		public static VBO Create(){
			VBO v = new VBO();
			GL.GenBuffers(1,out v.PositionArrayBufferID);
			GL.GenBuffers(1,out v.OtherArrayBufferID);
			GL.GenBuffers(1,out v.ElementArrayBufferID);
			return v;
		}
		public static VBO Create(int position_dimensions,VertexAttributes attribs){
			VBO v = new VBO();
			GL.GenBuffers(1,out v.PositionArrayBufferID);
			GL.GenBuffers(1,out v.OtherArrayBufferID);
			GL.GenBuffers(1,out v.ElementArrayBufferID);
			v.PositionDimensions = position_dimensions;
			v.VertexAttribs = attribs;
			return v;
		}
	}
	public class VertexAttributes{
		public float[][] Defaults;
		public int[] Size;
		public int TotalSize;

		public static VertexAttributes Create(params float[][] defaults){
			VertexAttributes v = new VertexAttributes();
			int count = defaults.GetLength(0);
			v.Defaults = new float[count][];
			v.Size = new int[count];
			v.TotalSize = 0;
			int idx = 0;
			foreach(float[] f in defaults){
				v.Defaults[idx] = f;
				v.Size[idx] = f.GetLength(0);
				v.TotalSize += v.Size[idx];
				++idx;
			}
			return v;
		}
		public static VertexAttributes Create(params int[] counts){ //makes zeroed arrays in the given counts.
			VertexAttributes v = new VertexAttributes();
			int count = counts.GetLength(0);
			v.Defaults = new float[count][];
			v.Size = new int[count];
			v.TotalSize = 0;
			int idx = 0;
			foreach(int i in counts){
				v.Defaults[idx] = new float[i]; //todo: this method needs a note:  which attribs are assumed to be here already? if you Create(2), is that texcoords? and what?
				v.Size[idx] = i;
				v.TotalSize += i;
				++idx;
			}
			return v;
		}
	}
}
