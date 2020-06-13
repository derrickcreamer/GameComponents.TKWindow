using System;
using System.Collections.Generic;
using System.Reflection;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;


namespace GameComponents.TKWindow{
	public class Texture{
		public int TextureIndex;
		public int TextureHeightPx;
		public int TextureWidthPx;
		public int DefaultSpriteTypeIndex = 0;
		public List<SpriteType> Sprite = null;

		protected static int next_texture = 0;
		protected static int max_textures = -1; //Currently, max_textures serves only to crash in a better way. Eventually I'll figure out how to swap texture units around, todo!
		protected static Dictionary<string,Texture> texture_info = new Dictionary<string,Texture>(); //the Textures contained herein are used only to store index/height/width
		public static Texture Create(string filename,string textureToReplace = null,bool loadFromEmbeddedResource = false){
			Texture t = new Texture();
			t.Sprite = new List<SpriteType>();
			if(textureToReplace != null){
				t.ReplaceTexture(filename,textureToReplace,loadFromEmbeddedResource);
			}
			else{
				t.LoadTexture(filename,loadFromEmbeddedResource);
			}
			return t;
		}
		protected Texture(){}
		protected void LoadTexture(string filename,bool loadFromEmbeddedResource = false){
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
				if(max_textures == -1){
					GL.GetInteger(GetPName.MaxTextureImageUnits,out max_textures);
				}
				int num = next_texture++;
				if(num == max_textures){ //todo: eventually fix this
					throw new NotSupportedException("This machine only supports " + num + " texture units, and this GL code isn't smart enough to switch them out yet, sorry.");
				}
				GL.ActiveTexture(TextureUnit.Texture0 + num);
				int id = GL.GenTexture(); //todo: eventually i'll want to support more than 16 or 32 textures. At that time I'll need to store this ID somewhere.
				GL.BindTexture(TextureTarget.Texture2D,id); //maybe a list of Scenes which are lists of textures needed, and then i'll bind all those and make sure to track their texture units.
				Bitmap bmp;
				if(loadFromEmbeddedResource){
					bmp = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(filename));
				}
				else{
					bmp = new Bitmap(filename);
				}
				BitmapData bmp_data = bmp.LockBits(new Rectangle(0,0,bmp.Width,bmp.Height),ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,bmp_data.Width,bmp_data.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,bmp_data.Scan0);
				bmp.UnlockBits(bmp_data);
				GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Linear); // CHANGED! todo
				TextureIndex = num;
				TextureHeightPx = bmp.Height;
				TextureWidthPx = bmp.Width;
				Texture t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
				t.TextureIndex = num;
				t.TextureHeightPx = bmp.Height;
				t.TextureWidthPx = bmp.Width;
				texture_info.Add(filename,t);
			}
		}
		protected void ReplaceTexture(string filename,string replaced,bool loadFromEmbeddedResource = false){
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
				if(texture_info.ContainsKey(replaced)){
					num = texture_info[replaced].TextureIndex;
					texture_info.Remove(replaced);
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
				Bitmap bmp;
				if(loadFromEmbeddedResource){
					bmp = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(filename));
				}
				else{
					bmp = new Bitmap(filename);
				}
				BitmapData bmp_data = bmp.LockBits(new Rectangle(0,0,bmp.Width,bmp.Height),ImageLockMode.ReadOnly,System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D,0,PixelInternalFormat.Rgba,bmp_data.Width,bmp_data.Height,0,OpenTK.Graphics.OpenGL.PixelFormat.Bgra,PixelType.UnsignedByte,bmp_data.Scan0);
				bmp.UnlockBits(bmp_data);
				GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMinFilter,(int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureTarget.Texture2D,TextureParameterName.TextureMagFilter,(int)TextureMagFilter.Nearest);
				TextureIndex = num;
				TextureHeightPx = bmp.Height;
				TextureWidthPx = bmp.Width;
				Texture t = new Texture(); //this one goes into the dictionary as an easy way to store the index/height/width of this filename.
				t.TextureIndex = num;
				t.TextureHeightPx = bmp.Height;
				t.TextureWidthPx = bmp.Width;
				texture_info.Add(filename,t);
			}
		}
	}
}
