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
using System.Reflection;
using StbImageSharp;
using OpenTK.Graphics.OpenGL;
using System.IO;


namespace GameComponents.TKWindow{
	public enum TextureLoadSource { FromFilePath, FromEmbedded, FromByteArray };
	public class Texture{
		public int TextureIndex;
		public int TextureHeightPx;
		public int TextureWidthPx;
		public int DefaultSpriteTypeIndex = 0;
		public List<SpriteType> Sprite = null;

		protected static int next_texture = 0;
		protected static int max_textures = -1; //Currently, max_textures serves only to crash in a better way. Eventually I'll figure out how to swap texture units around, todo!
		protected static Dictionary<string,Texture> texture_info = new Dictionary<string,Texture>(); //the Textures contained herein are used only to store index/height/width
		///<param name="filename">Note that filename is required even if passing a byte[], because filename is used as the key to identify duplicate textures</param>
		///<param name="textureBytes">Used only if source == TextureLoadSource.FromByteArray</param>
		public static Texture Create(string filename, string filenameOfTextureToReplace = null, TextureMinFilter minFilter = TextureMinFilter.Nearest,
			TextureMagFilter magFilter = TextureMagFilter.Nearest, TextureLoadSource source = TextureLoadSource.FromFilePath, byte[] textureBytes = null)
		{
			Texture t = new Texture();
			t.Sprite = new List<SpriteType>();
			t.LoadTexture(filename, filenameOfTextureToReplace, minFilter, magFilter, source, textureBytes);
			return t;
		}
		protected Texture(){}
		protected void LoadTexture(string filename, string filenameOfTextureToReplace = null, TextureMinFilter minFilter = TextureMinFilter.Nearest,
			TextureMagFilter magFilter = TextureMagFilter.Nearest, TextureLoadSource source = TextureLoadSource.FromFilePath, byte[] textureBytes = null)
		{
			if(String.IsNullOrEmpty(filename)){
				throw new ArgumentException(filename);
			}
			if(texture_info.ContainsKey(filename)){
				Texture t = texture_info[filename];
				TextureIndex = t.TextureIndex;
				TextureHeightPx = t.TextureHeightPx;
				TextureWidthPx = t.TextureWidthPx;
			}
			else{
				int num;
				if(filenameOfTextureToReplace != null && texture_info.ContainsKey(filenameOfTextureToReplace)){
					num = texture_info[filenameOfTextureToReplace].TextureIndex;
					texture_info.Remove(filenameOfTextureToReplace);
				}
				else{
					if(max_textures == -1){
						GL.GetInteger(GetPName.MaxTextureImageUnits,out max_textures);
					}
					num = next_texture++;
					if(num == max_textures){ //todo: eventually fix this
						throw new NotSupportedException("This machine only supports " + num + " texture units, and this GL code isn't smart enough to switch them out yet, sorry.");
					}
				}
				GL.ActiveTexture(TextureUnit.Texture0 + num);
				int id = GL.GenTexture(); //todo: eventually i'll want to support more than 16 or 32 textures. At that time I'll need to store this ID somewhere.
				GL.BindTexture(TextureTarget.Texture2D,id); //maybe a list of Scenes which are lists of textures needed, and then i'll bind all those and make sure to track their texture units.
				Stream stream;
				if(source == TextureLoadSource.FromEmbedded){
					stream = Assembly.GetEntryAssembly().GetManifestResourceStream(filename);
				}
				else if(source == TextureLoadSource.FromByteArray){
					if(textureBytes == null) throw new ArgumentNullException(nameof(textureBytes));
					stream = new MemoryStream(textureBytes);
				}
				else{
					stream = File.OpenRead(filename);
				}
				StbImage.stbi_set_flip_vertically_on_load(1);
				ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
				GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,image.Width,image.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,image.Data);
				GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)minFilter);
				GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)magFilter);
				TextureIndex = num;
				TextureHeightPx = image.Height;
				TextureWidthPx = image.Width;
				Texture t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
				t.TextureIndex = num;
				t.TextureHeightPx = image.Height;
				t.TextureWidthPx = image.Width;
				texture_info.Add(filename,t);
			}
		}
	}
}
