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
	public class ShaderCollection{
		public static string DefaultVS(){
			return
@"#version 120
uniform vec2 offset;

attribute vec4 position;
attribute vec2 texcoord;
attribute vec4 color;
attribute vec4 bgcolor;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 texcoord_fs = texcoord;
 color_fs = color;
 bgcolor_fs = bgcolor;
 gl_Position = vec4(position.x + offset.x,-position.y - offset.y,position.z,1);
}
";
		}
		///<summary>Discards if alpha is less than 0.1, but otherwise uses the raw value</summary>
		public static string DefaultFS(){ //todo: I could make a builder for these, kinda. It could make things like alpha testing optional.
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = v;
}
";
		}
		///<summary>Works with font textures that have #FFF text on any background, without any AA</summary>
		public static string FontFS(){
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.r == 1.0 && v.g == 1.0 && v.b == 1.0){
  gl_FragColor = color_fs;
 }
 else{
  gl_FragColor = bgcolor_fs;
 }
}
";
		}
		///<summary>(Inefficiently) reads only the alpha value from the font texture to determine how to combine fg+bg colors</summary>
		public static string AAFontFS(){
			return
				@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 gl_FragColor = vec4(
	 bgcolor_fs.r * (1.0 - v.a) + color_fs.r * v.a,
	 bgcolor_fs.g * (1.0 - v.a) + color_fs.g * v.a,
	 bgcolor_fs.b * (1.0 - v.a) + color_fs.b * v.a,
	 bgcolor_fs.a * (1.0 - v.a) + color_fs.a * v.a);
}
";
		}
		///<summary>Requires MSDF font texture. Returns a shader for the given texture size and pxRange.</summary>
		public static string GetMsdfFS(int textureSize, int pxRange){
			return
			@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	float pxRange = " + pxRange.ToString() + @".0;
	vec2 texture_size = vec2(" + textureSize.ToString() + @".0, " + textureSize.ToString() + @".0);
	vec3 sample = texture2D(texture, texcoord_fs).rgb;
	float sigDist = median(sample.r, sample.g, sample.b) - 0.5;
	sigDist *= dot(pxRange/texture_size, 0.5/fwidth(texcoord_fs));
	float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
	float remaining_opacity = (1.0 - opacity);
	gl_FragColor = vec4(
		bgcolor_fs.r * remaining_opacity + color_fs.r * opacity,
		bgcolor_fs.g * remaining_opacity + color_fs.g * opacity,
		bgcolor_fs.b * remaining_opacity + color_fs.b * opacity,
		bgcolor_fs.a * remaining_opacity + color_fs.a * opacity);
}";
		}
		///<summary>Requires MSDF font texture. Returns a shader for the given texture size and pxRange which makes the result grayscale.</summary>
		public static string GetGrayscaleMsdfFS(int textureSize, int pxRange){
			return
			@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	float pxRange = " + pxRange.ToString() + @".0;
	vec2 texture_size = vec2(" + textureSize.ToString() + @".0, " + textureSize.ToString() + @".0);
	vec3 sample = texture2D(texture, texcoord_fs).rgb;
	float sigDist = median(sample.r, sample.g, sample.b) - 0.5;
	sigDist *= dot(pxRange/texture_size, 0.5/fwidth(texcoord_fs));
	float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
	float remaining_opacity = (1.0 - opacity);
	vec4 v = vec4(
		bgcolor_fs.r * remaining_opacity + color_fs.r * opacity,
		bgcolor_fs.g * remaining_opacity + color_fs.g * opacity,
		bgcolor_fs.b * remaining_opacity + color_fs.b * opacity,
		bgcolor_fs.a * remaining_opacity + color_fs.a * opacity);
	float f = 0.3 * v.r + 0.5 * v.g + 0.2 * v.b;
	gl_FragColor = vec4(f,f,f,v.a);
}";
		}
		///<summary>Discards if alpha from texture is less than 0.1, but otherwise the result for each channel is the result of multiplying the texture's value with the color attribute's value for that channel</summary>
		public static string TintFS(){
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = vec4(
	 v.r * color_fs.r,
	 v.g * color_fs.g,
	 v.b * color_fs.b,
	 v.a * color_fs.a);
}
";
		}
		///<summary>Discards if alpha from texture is less than 0.1, but otherwise the result for each channel is the result of multiplying the texture's value with the color attribute's value for that channel,
		/// and then adding the bgcolor attribute's value for that channel.</summary>
		public static string NewTintFS(){
			return
				@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = vec4(
	 v.r * color_fs.r + bgcolor_fs.r,
	 v.g * color_fs.g + bgcolor_fs.g,
	 v.b * color_fs.b + bgcolor_fs.b,
	 v.a * color_fs.a + bgcolor_fs.a);
}
";
		}
		///<summary>Discards alpha less than 0.1, but otherwise calculates a monochrome value from RGB, skewed to 30% red, 50% green, and 20% blue</summary>
		public static string GrayscaleFS(){
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 float f = 0.3 * v.r + 0.5 * v.g + 0.2 * v.b;
 gl_FragColor = vec4(f,f,f,v.a);
}
";
		}
		///<summary>Same as GrayscaleFS, but keeps the original color if R, G, or B value is high enough while the other 2 are low. Needs work.</summary>
		public static string GrayscaleWithColorsFS(){
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 if(v.a < 0.1){
  discard;
 }
 float f = 0.3 * v.r + 0.5 * v.g + 0.2 * v.b;
 gl_FragColor = vec4(f,f,f,v.a);
 if((v.r > 0.6 && v.g < 0.4 && v.b < 0.4)
 || (v.g > 0.6 && v.r < 0.4 && v.b < 0.4)
 || (v.b > 0.6 && v.r < 0.4 && v.g < 0.4))
 {
  gl_FragColor = v;
 }
}
";
		}
	}

}
