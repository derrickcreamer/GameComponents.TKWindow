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
 //gl_FragColor = texture2D(texture,texcoord_fs);
 gl_FragColor = v;
}
";
		}
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
		public static string AAFontFS(){
			return
				@"#version 120
uniform sampler2D texture;

varying vec2 texcoord_fs;
varying vec4 color_fs;
varying vec4 bgcolor_fs;

void main(){
 vec4 v = texture2D(texture,texcoord_fs);
 gl_FragColor = vec4(bgcolor_fs.r * (1.0 - v.a) + color_fs.r * v.a,bgcolor_fs.g * (1.0 - v.a) + color_fs.g * v.a,bgcolor_fs.b * (1.0 - v.a) + color_fs.b * v.a,bgcolor_fs.a * (1.0 - v.a) + color_fs.a * v.a);
}
";
		}
		public static string MsdfFS(){
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
	vec4 color2 = vec4(1.0,0.0,1.0,1.0);
	vec4 bgcolor2 = vec4(0.0,0.0,0.1,1.0);
	bgcolor2 = bgcolor_fs;
	color2 = color_fs; //todo fix
	float pxRange = 4.0;
	vec2 texture_size = vec2(2048.0, 2048.0);
    vec3 sample = texture2D(texture, texcoord_fs).rgb;
    float sigDist = median(sample.r, sample.g, sample.b) - 0.5;
    sigDist *= dot(pxRange/texture_size, 0.5/fwidth(texcoord_fs));
    float opacity = clamp(sigDist + 0.5, 0.0, 1.0);
	float va = opacity;
	float mva = (1.0 - va);
    gl_FragColor = vec4(bgcolor2.r * mva + color2.r * va,bgcolor2.g * mva + color2.g * va,bgcolor2.b * mva + color2.b * va,bgcolor2.a * mva + color2.a * va);
}";
//    gl_FragColor = mix(bgcolor_fs, color_fs, opacity);
		}
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
	float va = opacity;
	float mva = (1.0 - va);
    gl_FragColor = vec4(bgcolor_fs.r * mva + color_fs.r * va,bgcolor_fs.g * mva + color_fs.g * va,bgcolor_fs.b * mva + color_fs.b * va,bgcolor_fs.a * mva + color_fs.a * va);
}";
		}
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
 gl_FragColor = vec4(v.r * color_fs.r,v.g * color_fs.g,v.b * color_fs.b,v.a * color_fs.a);
}
";
		}
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
 gl_FragColor = vec4(v.r * color_fs.r + bgcolor_fs.r,v.g * color_fs.g + bgcolor_fs.g,v.b * color_fs.b + bgcolor_fs.b,v.a * color_fs.a + bgcolor_fs.a);
}
";
		}
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
 float f = 0.1 * v.r + 0.2 * v.b + 0.7 * v.g;
 gl_FragColor = vec4(f,f,f,v.a);
}
";
		}
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
 float f = 0.1 * v.r + 0.7 * v.g + 0.2 * v.b;
 gl_FragColor = vec4(f,f,f,v.a);
 if((v.r > 0.6 && v.g < 0.4 && v.b < 0.4) || (v.g > 0.6 && v.r < 0.4 && v.b < 0.4) || (v.b > 0.6 && v.r < 0.4 && v.g < 0.4)){
  gl_FragColor = v;
 }
}
";
		}
	}

}
