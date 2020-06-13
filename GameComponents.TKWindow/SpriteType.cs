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
	public class SpriteType{ //each different arrangement of sprites on a sheet gets its own SpriteType. Many, like fonts, will use only a single SpriteType for the whole sheet.
		public PositionFromIndex X; //SpriteType is pretty similar to CellLayout. Any chance they could ever be combined?
		public PositionFromIndex Y;
		public float SpriteHeight; //0 to 1, not pixels //todo xml, explain all units
		public float SpriteWidth;
		public int DefaultSpriteIndex;
		public static SpriteType DefineSingleRowSprite(Surface surface,int sprite_width_px){
			SpriteType s = new SpriteType();
			float texcoord_width = (float)sprite_width_px * 1.0f / (float)surface.texture.TextureWidthPx;
			s.X = idx => idx * texcoord_width;
			s.Y = idx => 0;
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = 1.0f;
			if(surface != null){
				surface.texture.Sprite.Add(s);
			}
			return s;
		}
		public static SpriteType DefineSingleRowSprite(Surface surface,int sprite_width_px,int padding_between_sprites_px){
			SpriteType s = new SpriteType();
			float px_width = 1.0f / (float)surface.texture.TextureWidthPx;
			float texcoord_width = (float)sprite_width_px * px_width;
			float texcoord_start = texcoord_width + (float)padding_between_sprites_px * px_width;
			s.X = idx => idx * texcoord_start;
			s.Y = idx => 0;
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = 1.0f;
			if(surface != null){
				surface.texture.Sprite.Add(s);
			}
			return s;
		}
		public static SpriteType DefineSpriteAcross(Surface surface,int sprite_width_px,int sprite_height_px,int num_columns){
			SpriteType s = new SpriteType();
			float texcoord_width = (float)sprite_width_px * 1.0f / (float)surface.texture.TextureWidthPx;
			float texcoord_height = (float)sprite_height_px * 1.0f / (float)surface.texture.TextureHeightPx;
			s.X = idx => (idx % num_columns) * texcoord_width;
			s.Y = idx => (idx / num_columns) * texcoord_height;
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			if(surface != null){
				surface.texture.Sprite.Add(s);
			}
			return s;
		}
		public static SpriteType DefineSpriteAcross(Surface surface,int sprite_width_px,int sprite_height_px,int num_columns,int h_offset_px,int v_offset_px){
			SpriteType s = new SpriteType();
			float texcoord_width = (float)sprite_width_px * 1.0f / (float)surface.texture.TextureWidthPx;
			float texcoord_height = (float)sprite_height_px * 1.0f / (float)surface.texture.TextureHeightPx;
			s.X = idx => ((idx % num_columns) * sprite_width_px + h_offset_px) * 1.0f / (float)surface.texture.TextureWidthPx;
			s.Y = idx => ((idx / num_columns) * sprite_height_px + v_offset_px) * 1.0f / (float)surface.texture.TextureHeightPx;
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			if(surface != null){
				surface.texture.Sprite.Add(s);
			}
			return s;
		}
		public static SpriteType DefineSpriteDown(Surface surface,int sprite_width_px,int sprite_height_px,int num_rows){
			SpriteType s = new SpriteType();
			float texcoord_width = (float)sprite_width_px * 1.0f / (float)surface.texture.TextureWidthPx;
			float texcoord_height = (float)sprite_height_px * 1.0f / (float)surface.texture.TextureHeightPx;
			s.X = idx => (idx / num_rows) * texcoord_width;
			s.Y = idx => (idx % num_rows) * texcoord_height;
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			if(surface != null){
				surface.texture.Sprite.Add(s);
			}
			return s;
		}
		public static SpriteType DefineSpriteDown(Surface surface,int sprite_width_px,int sprite_height_px,int num_rows,int h_offset_px,int v_offset_px){
			SpriteType s = new SpriteType();
			float texcoord_width = (float)sprite_width_px * 1.0f / (float)surface.texture.TextureWidthPx;
			float texcoord_height = (float)sprite_height_px * 1.0f / (float)surface.texture.TextureHeightPx;
			s.X = idx => ((idx / num_rows) * sprite_width_px + h_offset_px) * 1.0f / (float)surface.texture.TextureWidthPx;
			s.Y = idx => ((idx % num_rows) * sprite_height_px + v_offset_px) * 1.0f / (float)surface.texture.TextureHeightPx;
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			if(surface != null){
				surface.texture.Sprite.Add(s);
			}
			return s;
		}
	}
}
