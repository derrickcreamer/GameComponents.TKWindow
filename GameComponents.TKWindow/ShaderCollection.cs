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
uniform int time; // todo, let's try 'frames', where one frame is 10ms so the math is easy.

attribute vec4 position;
attribute vec2 texcoord;
attribute vec4 color;
attribute vec4 bgcolor;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;
varying vec4 position_fs; //todo clean up position in shader...decide how to do this. Probably just keep it all the time, and add time too.

void main(){
 texcoord_fs = texcoord;
 color_fs = color;
 bgcolor_fs = bgcolor;
 position_fs = vec4(position.x + offset.x, -position.y - offset.y, position.z, 1.0);
 gl_Position = position_fs;
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
varying vec4 position_fs; //todo

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	float pxRange = " + pxRange.ToString() + @".0;
	vec2 texture_size = vec2(" + textureSize.ToString() + @".0, " + textureSize.ToString() + @".0);
	vec3 sample = texture2D(texture, texcoord_fs).rgb;
	float sigDist = (median(sample.r, sample.g, sample.b) - 0.5);
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
		///<summary>todo desc.Requires MSDF font texture. Returns a shader for the given texture size and pxRange.</summary>
		public static string GetMsdfFS_todoplasma(int textureSize, int pxRange){
			return
			@"#version 120
uniform sampler2D texture;
uniform int time; // todo, let's try 'frames', where one frame is 10ms so the math is easy.

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;
varying vec4 position_fs; //todo

const float PI = 3.1415926535897932384626433832795;

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

    float time2 = float(time) / 2.0;
	float t = float(time2) / 800.0;

	float v = 0.0;
	v += sin(position_fs.x + t);
	v += sin((position_fs.y + t)/2.0);
	v += sin((position_fs.x + position_fs.y + t)/2.0);
	float r = sin(v * PI);
	float g = cos(v * PI);
	float b = cos(v * v * PI);

	float t2 = float(time2) / 702.0;
	float v2 = 0.0;
	float cx = position_fs.x + 0.5 * sin(t2/3.0);
	float cy = position_fs.y + 0.5 * cos(t2/2.0);
	v2 += sin(sqrt(100 * (cx*cx + cy*cy) + 1.0) + t2);
	float d2 = sin(sqrt(100 * (cx*cx + cy*cy) + 1.0) + t2);

	float t3 = float(time2) / 532.0;
	float cx3 = position_fs.x + 0.5 * sin(t3/3.0);
	float cy3 = position_fs.y + 0.5 * cos(t3/2.0);
	v2 += sin(sqrt(100 * (cx3*cx3 + cy3*cy3) + 1.0) + t3);
	float d3 = sin(sqrt(100 * (cx3*cx3 + cy3*cy3) + 1.0) + t3);

	float t4 = float(time2) / 172.0;
	float cx4 = position_fs.x + 0.5 * sin(t4/3.0);
	float cy4 = position_fs.y + 0.5 * cos(t4/2.0);
	v2 += sin(sqrt(100 * (cx4*cx4 + cy4*cy4) + 1.0) + t4); // multiplying by a bigger number before the sqrt makes more of a zoomed out lattice effect
	float d4 = sin(sqrt(100 * (cx4*cx4 + cy4*cy4) + 1.0) + t4); // multiplying by a bigger number before the sqrt makes more of a zoomed out lattice effect

	//v2 += cos(position_fs.x + t2);
	//v2 += cos((position_fs.y + t2)/2.0);
	//v2 += cos((position_fs.x + position_fs.y + t2)/2.0);
	float a = cos(v2 * PI / 2.0); // Divide by a bigger number to spread the values over a wider area. Multiply by v2 more times to get cells with more defined circles.


	vec4 agl_FragColor = vec4(
		(bgcolor_fs.r * remaining_opacity + color_fs.r * opacity)*0.9 + r*0.1,
		(bgcolor_fs.g * remaining_opacity + color_fs.g * opacity)*0.9 + g*0.1,
		(bgcolor_fs.b * remaining_opacity + color_fs.b * opacity)*0.9 + b*0.1,
		bgcolor_fs.a * remaining_opacity + color_fs.a * opacity);

r = (r+d2)/2.0;
g = (g+d3)/2.0;
b = (b+d4)/2.0;
	//gl_FragColor
	 //plasma
	  //= vec4(d2,d3,d4,a);
	  //= vec4(r,g,b,a);

	  float a1 = a * 0.3;
	  float a9 = 1.0 - a1;

	  if(color_fs.r < 0.1 && color_fs.g < 0.1 && color_fs.b < 0.1){
		  a1 = 0.0;
		  a9 = 1.0;
	  }

	  if(
		  !(		(bgcolor_fs.r * remaining_opacity + color_fs.r * opacity) > 0.39
		|| (bgcolor_fs.g * remaining_opacity + color_fs.g * opacity) > 0.39
		|| (bgcolor_fs.b * remaining_opacity + color_fs.b * opacity) > 0.39)
	  ){
		  a1 = 0.0;
		  a9 = 1.0;
	  }

	  //if(position_fs.x > 0.0)
		  gl_FragColor = vec4(
		(bgcolor_fs.r * remaining_opacity + color_fs.r * opacity)*a9 + r*a1,
		(bgcolor_fs.g * remaining_opacity + color_fs.g * opacity),//*a9 + g*a1,
		(bgcolor_fs.b * remaining_opacity + color_fs.b * opacity),//*a9 + b*a1,
		bgcolor_fs.a * remaining_opacity + color_fs.a * opacity);
	//else
	 vec4 aagl_FragColor = vec4(
		(bgcolor_fs.r * remaining_opacity + color_fs.r * opacity)*0.9 + r*0.1,
		(bgcolor_fs.g * remaining_opacity + color_fs.g * opacity)*0.9 + g*0.1,
		(bgcolor_fs.b * remaining_opacity + color_fs.b * opacity)*0.9 + b*0.1,
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
