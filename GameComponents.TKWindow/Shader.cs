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
using OpenTK.Graphics.OpenGL;

namespace GameComponents.TKWindow{
	public class Shader{
		public int ShaderProgramID;
		public int OffsetUniformLocation;
		public int TextureUniformLocation;

		protected class id_and_programs{
			public int id;
			public Dictionary<int,Shader> programs = null;
		}
		protected static Dictionary<string,id_and_programs> compiled_vs = new Dictionary<string,id_and_programs>();
		protected static Dictionary<string,int> compiled_fs = new Dictionary<string,int>();

		public static Shader Create(string frag_shader){
			return Create(ShaderCollection.DefaultVS(),frag_shader);
		}
		public static Shader Create(string vert_shader,string frag_shader){
			Shader s = new Shader();
			int vertex_shader = -1;
			if(compiled_vs.ContainsKey(vert_shader)){
				vertex_shader = compiled_vs[vert_shader].id;
			}
			else{
				vertex_shader = GL.CreateShader(ShaderType.VertexShader);
				GL.ShaderSource(vertex_shader,vert_shader);
				GL.CompileShader(vertex_shader);
				int compiled;
				GL.GetShader(vertex_shader,ShaderParameter.CompileStatus,out compiled);
				if(compiled < 1){
					Console.Error.WriteLine(GL.GetShaderInfoLog(vertex_shader));
					throw new Exception("vertex shader compilation failed");
				}
				id_and_programs v = new id_and_programs();
				v.id = vertex_shader;
				compiled_vs.Add(vert_shader,v);
			}
			int fragment_shader = -1;
			if(compiled_fs.ContainsKey(frag_shader)){
				fragment_shader = compiled_fs[frag_shader];
			}
			else{
				fragment_shader = GL.CreateShader(ShaderType.FragmentShader);
				GL.ShaderSource(fragment_shader,frag_shader);
				GL.CompileShader(fragment_shader);
				int compiled;
				GL.GetShader(fragment_shader,ShaderParameter.CompileStatus,out compiled);
				if(compiled < 1){
					Console.Error.WriteLine(GL.GetShaderInfoLog(fragment_shader));
					throw new Exception("fragment shader compilation failed");
				}
				compiled_fs.Add(frag_shader,fragment_shader);
			}
			if(compiled_vs[vert_shader].programs != null && compiled_vs[vert_shader].programs.ContainsKey(fragment_shader)){
				s.ShaderProgramID = compiled_vs[vert_shader].programs[fragment_shader].ShaderProgramID;
				s.OffsetUniformLocation = compiled_vs[vert_shader].programs[fragment_shader].OffsetUniformLocation;
				s.TextureUniformLocation = compiled_vs[vert_shader].programs[fragment_shader].TextureUniformLocation;
			}
			else{
				int shader_program = GL.CreateProgram();
				GL.AttachShader(shader_program,vertex_shader);
				GL.AttachShader(shader_program,fragment_shader);
				int attrib_index = 0;
				foreach(string attr in new string[]{"position","texcoord","color","bgcolor"}){
					GL.BindAttribLocation(shader_program,attrib_index++,attr);
				}
				GL.LinkProgram(shader_program);
				s.ShaderProgramID = shader_program;
				s.OffsetUniformLocation = GL.GetUniformLocation(shader_program,"offset");
				s.TextureUniformLocation = GL.GetUniformLocation(shader_program,"texture");
				if(compiled_vs[vert_shader].programs == null){
					compiled_vs[vert_shader].programs = new Dictionary<int,Shader>();
				}
				Shader p = new Shader();
				p.ShaderProgramID = shader_program;
				p.OffsetUniformLocation = s.OffsetUniformLocation;
				p.TextureUniformLocation = s.TextureUniformLocation;
				compiled_vs[vert_shader].programs.Add(fragment_shader,p);
			}
			return s;
		}
	}
}
