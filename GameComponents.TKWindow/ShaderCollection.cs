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
		// uniforms:
		//   texture
		//   offset
		//   time
		//   viewportSize (todo)
		public static string DefaultVS(){
			return
@"#version 120
uniform vec2 offset;

attribute vec4 position_vs;
attribute vec2 texcoord_vs;
attribute vec4 color_vs;
attribute vec4 bgcolor_vs;

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;
varying vec4 position; //todo clean up position in shader...decide how to do this. Probably just keep it all the time, and add time too.

void main(){
 texcoord = texcoord_vs;
 color = color_vs;
 bgcolor = bgcolor_vs;
 position = vec4(position_vs.x + offset.x, -position_vs.y - offset.y, position_vs.z, 1.0);
 gl_Position = position;
}
";
		}
		///<summary>Discards if alpha is less than 0.1, but otherwise uses the raw value</summary>
		public static string DefaultFS(){ //todo: I could make a builder for these, kinda. It could make things like alpha testing optional.
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;

void main(){
 vec4 v = texture2D(texture,texcoord);
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

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;

void main(){
 vec4 v = texture2D(texture,texcoord);
 if(v.r == 1.0 && v.g == 1.0 && v.b == 1.0){
  gl_FragColor = color;
 }
 else{
  gl_FragColor = bgcolor;
 }
}
";
		}
		///<summary>(Inefficiently) reads only the alpha value from the font texture to determine how to combine fg+bg colors</summary>
		public static string AAFontFS(){
			return
				@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;

void main(){
 vec4 v = texture2D(texture,texcoord);
 gl_FragColor = vec4(
	 bgcolor.r * (1.0 - v.a) + color.r * v.a,
	 bgcolor.g * (1.0 - v.a) + color.g * v.a,
	 bgcolor.b * (1.0 - v.a) + color.b * v.a,
	 bgcolor.a * (1.0 - v.a) + color.a * v.a);
}
";
		}
		public static string Noise(){ //todo
			return
			@"#version 120
uniform sampler2D texture;
uniform int time; // todo, let's try 'frames', where one frame is 10ms so the math is easy.

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;
varying vec4 position; //todo

float noiseValue(vec2 p,int frame){
	float t = float(frame)/100.0;
	return fract(sin(p.x*100.0 + p.y*5433.0) * 7223.0);
	//return fract(sin(p.x*100.0 + p.y*(5433.0 + sin(t))) * (7223.0 + cos(t)));
	//return abs(abs(sin(abs(p.x*100.0 + p.y*5433.0 + t))) - abs(cos(abs(p.x*7311.1 + p.y*4174.0 + t))) );
	//return sin(abs(p.x*100.0 + p.y*5433.0 + t)); // drifting lines of clouds version
	//return fract(sin(p.x*311.0 + p.y*5782.0) * 18967.0);
}

float smoothNoise(vec2 uv,int frame){
	vec2 lv = fract(uv);
	vec2 cell = floor(uv);
	lv = lv*lv*(3.0 - 2.0 * lv);
	float BL = noiseValue(cell, frame);
	float BR = noiseValue(cell + vec2(1.0, 0.0), frame);
	float TL = noiseValue(cell + vec2(0.0, 1.0), frame);
	float TR = noiseValue(cell + vec2(1.0, 1.0), frame);
	float bottom = mix(BL, BR, lv.x);
	float top = mix(TL, TR, lv.x);
	return mix(bottom, top, lv.y);
}

float smoothNoise2(vec2 uv, int frame){
	float c = smoothNoise(uv * 4.0, frame);
	c += smoothNoise(uv * 8.0, frame) * 0.5;
	c += smoothNoise(uv * 16.0, frame) * 0.25;
	c += smoothNoise(uv * 32.0, frame) * 0.125;
	c += smoothNoise(uv * 64.0, frame) * 0.0625;
	return c / 2.0;
}

void main() {
	vec2 uv = gl_FragCoord.xy / vec2(3840.0, 2160.0);
	float c2 = smoothNoise2(uv, time);
	float c3 = smoothNoise2(uv, time+10);
	float c4 = mix(c2, c3, float((mod(time, 10.0))) / 10.0);
	gl_FragColor = vec4(c2,c2,c2,1.0);
	//gl_FragColor = vec4(c4,c4,c4,1.0);
	return;
	/*float n = fract(
		sin(uv.x * 0.15125214 + uv.y * 3.364256243) * 5758.0123
	);*/
	float t = float(time/100); // seconds
	float n = fract(
		sin(uv.x * (t * 0.015125214) + uv.y * (t * 0.3364256243)) * 5758.0123257457
	);
	float t2 = float(time/100 + 1);
	float n2 = fract(
		sin(uv.x * (t2 * 0.015125214) + uv.y * (t2 * 0.3364256243)) * 5758.0123257457
	);
	float c = mix(n, n2, float(time)/100.0 - t);
	gl_FragColor = vec4(n2,n2,n2, 1.0);
}";
		}
		///<summary>Requires MSDF font texture. Returns a shader for the given texture size and pxRange.</summary>
		public static string GetMsdfFS(int textureSize, int pxRange){
			return
			@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;
varying vec4 position; //todo

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	float pxRange = " + pxRange.ToString() + @".0;
	vec2 texture_size = vec2(" + textureSize.ToString() + @".0, " + textureSize.ToString() + @".0);
	vec3 sample = texture2D(texture, texcoord).rgb;
	float sigDist = (median(sample.r, sample.g, sample.b) - 0.5);
	sigDist *= dot(pxRange/texture_size, 0.5/fwidth(texcoord));
	float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
	float remaining_opacity = (1.0 - opacity);
	gl_FragColor = vec4(
		bgcolor.r * remaining_opacity + color.r * opacity,
		bgcolor.g * remaining_opacity + color.g * opacity,
		bgcolor.b * remaining_opacity + color.b * opacity,
		bgcolor.a * remaining_opacity + color.a * opacity);
}";
		}
		///<summary>todo desc.Requires MSDF font texture. Returns a shader for the given texture size and pxRange.</summary>
		public static string GetMsdfFS_todoplasma(int textureSize, int pxRange){
			return
			@"#version 120
uniform sampler2D texture;
uniform int time; // todo, let's try 'frames', where one frame is 10ms so the math is easy.

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;
varying vec4 position; //todo

const float PI = 3.1415926535897932384626433832795;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	float pxRange = " + pxRange.ToString() + @".0;
	vec2 texture_size = vec2(" + textureSize.ToString() + @".0, " + textureSize.ToString() + @".0);
	vec3 sample = texture2D(texture, texcoord).rgb;
	float sigDist = median(sample.r, sample.g, sample.b) - 0.5;
	sigDist *= dot(pxRange/texture_size, 0.5/fwidth(texcoord));
	float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
	float remaining_opacity = (1.0 - opacity);

    float time2 = float(time) / 2.0;
	float t = float(time2) / 800.0;

	float v = 0.0;
	v += sin(position.x + t);
	v += sin((position.y + t)/2.0);
	v += sin((position.x + position.y + t)/2.0);
	float r = sin(v * PI);
	float g = cos(v * PI);
	float b = cos(v * v * PI);

	float t2 = float(time2) / 702.0;
	float v2 = 0.0;
	float cx = position.x + 0.5 * sin(t2/3.0);
	float cy = position.y + 0.5 * cos(t2/2.0);
	v2 += sin(sqrt(100 * (cx*cx + cy*cy) + 1.0) + t2);
	float d2 = sin(sqrt(100 * (cx*cx + cy*cy) + 1.0) + t2);

	float t3 = float(time2) / 532.0;
	float cx3 = position.x + 0.5 * sin(t3/3.0);
	float cy3 = position.y + 0.5 * cos(t3/2.0);
	v2 += sin(sqrt(100 * (cx3*cx3 + cy3*cy3) + 1.0) + t3);
	float d3 = sin(sqrt(100 * (cx3*cx3 + cy3*cy3) + 1.0) + t3);

	float t4 = float(time2) / 172.0;
	float cx4 = position.x + 0.5 * sin(t4/3.0);
	float cy4 = position.y + 0.5 * cos(t4/2.0);
	v2 += sin(sqrt(100 * (cx4*cx4 + cy4*cy4) + 1.0) + t4); // multiplying by a bigger number before the sqrt makes more of a zoomed out lattice effect
	float d4 = sin(sqrt(100 * (cx4*cx4 + cy4*cy4) + 1.0) + t4); // multiplying by a bigger number before the sqrt makes more of a zoomed out lattice effect

	//v2 += cos(position.x + t2);
	//v2 += cos((position.y + t2)/2.0);
	//v2 += cos((position.x + position.y + t2)/2.0);
	float a = cos(v2 * PI / 2.0); // Divide by a bigger number to spread the values over a wider area. Multiply by v2 more times to get cells with more defined circles.


	vec4 agl_FragColor = vec4(
		(bgcolor.r * remaining_opacity + color.r * opacity)*0.9 + r*0.1,
		(bgcolor.g * remaining_opacity + color.g * opacity)*0.9 + g*0.1,
		(bgcolor.b * remaining_opacity + color.b * opacity)*0.9 + b*0.1,
		bgcolor.a * remaining_opacity + color.a * opacity);

r = (r+d2)/2.0;
g = (g+d3)/2.0;
b = (b+d4)/2.0;
	//gl_FragColor
	 //plasma
	  //= vec4(d2,d3,d4,a);
	  //= vec4(r,g,b,a);

	  float a1 = a * 0.3;
	  float a9 = 1.0 - a1;

	  if(color.r < 0.1 && color.g < 0.1 && color.b < 0.1){
		  a1 = 0.0;
		  a9 = 1.0;
	  }

	  if(
		  !(		(bgcolor.r * remaining_opacity + color.r * opacity) > 0.39
		|| (bgcolor.g * remaining_opacity + color.g * opacity) > 0.39
		|| (bgcolor.b * remaining_opacity + color.b * opacity) > 0.39)
	  ){
		  a1 = 0.0;
		  a9 = 1.0;
	  }

	  //if(position.x > 0.0)
		  gl_FragColor = vec4(
		(bgcolor.r * remaining_opacity + color.r * opacity)*a9 + r*a1,
		(bgcolor.g * remaining_opacity + color.g * opacity),//*a9 + g*a1,
		(bgcolor.b * remaining_opacity + color.b * opacity),//*a9 + b*a1,
		bgcolor.a * remaining_opacity + color.a * opacity);
	//else
	 vec4 aagl_FragColor = vec4(
		(bgcolor.r * remaining_opacity + color.r * opacity)*0.9 + r*0.1,
		(bgcolor.g * remaining_opacity + color.g * opacity)*0.9 + g*0.1,
		(bgcolor.b * remaining_opacity + color.b * opacity)*0.9 + b*0.1,
		bgcolor.a * remaining_opacity + color.a * opacity);
}";
		}
		///<summary>Requires MSDF font texture. Returns a shader for the given texture size and pxRange which makes the result grayscale.</summary>
		public static string GetGrayscaleMsdfFS(int textureSize, int pxRange){
			return
			@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;

float median(float r, float g, float b) {
	return max(min(r, g), min(max(r, g), b));
}

void main() {
	float pxRange = " + pxRange.ToString() + @".0;
	vec2 texture_size = vec2(" + textureSize.ToString() + @".0, " + textureSize.ToString() + @".0);
	vec3 sample = texture2D(texture, texcoord).rgb;
	float sigDist = median(sample.r, sample.g, sample.b) - 0.5;
	sigDist *= dot(pxRange/texture_size, 0.5/fwidth(texcoord));
	float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
	float remaining_opacity = (1.0 - opacity);
	vec4 v = vec4(
		bgcolor.r * remaining_opacity + color.r * opacity,
		bgcolor.g * remaining_opacity + color.g * opacity,
		bgcolor.b * remaining_opacity + color.b * opacity,
		bgcolor.a * remaining_opacity + color.a * opacity);
	float f = 0.3 * v.r + 0.5 * v.g + 0.2 * v.b;
	gl_FragColor = vec4(f,f,f,v.a);
}";
		}
		///<summary>Discards if alpha from texture is less than 0.1, but otherwise the result for each channel is the result of multiplying the texture's value with the color attribute's value for that channel</summary>
		public static string TintFS(){
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;
varying vec4 color;

void main(){
 vec4 v = texture2D(texture,texcoord);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = vec4(
	 v.r * color.r,
	 v.g * color.g,
	 v.b * color.b,
	 v.a * color.a);
}
";
		}
		///<summary>Discards if alpha from texture is less than 0.1, but otherwise the result for each channel is the result of multiplying the texture's value with the color attribute's value for that channel,
		/// and then adding the bgcolor attribute's value for that channel.</summary>
		public static string NewTintFS(){
			return
				@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;
varying vec4 color;
varying vec4 bgcolor;

void main(){
 vec4 v = texture2D(texture,texcoord);
 if(v.a < 0.1){
  discard;
 }
 gl_FragColor = vec4(
	 v.r * color.r + bgcolor.r,
	 v.g * color.g + bgcolor.g,
	 v.b * color.b + bgcolor.b,
	 v.a * color.a + bgcolor.a);
}
";
		}
		///<summary>Discards alpha less than 0.1, but otherwise calculates a monochrome value from RGB, skewed to 30% red, 50% green, and 20% blue</summary>
		public static string GrayscaleFS(){
			return
@"#version 120
uniform sampler2D texture;

varying vec2 texcoord;

void main(){
 vec4 v = texture2D(texture,texcoord);
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

varying vec2 texcoord;

void main(){
 vec4 v = texture2D(texture,texcoord);
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
