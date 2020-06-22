/*Copyright (c) 2011-2020  Derrick Creamer
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using GameComponents.TKWindow;
using System.Threading;
using OpenTK.Input;

namespace Todo{
	        public class RNG { //splitmix64
                public ulong RngState;

                public ulong GetNext() {
                        unchecked {
                                ulong z = (RngState += 0x9e3779b97f4a7c15);
                                z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
                                z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
                                return z ^ (z >> 31);
                        }
                }

                public RNG(ulong seed){ RngState = seed; }
                /*public int GetNext(int upperExclusiveBound) => (int)(GetNext() % (ulong)upperExclusiveBound); //todo, improve w/128bit mult?
                public bool CoinFlip() => GetNext() % 2 == 0;
                public bool OneIn(int x) => GetNext() % (ulong)x == 0;*/
                //todo, test this eventually:
                public int GetNext(int upperExclusiveBound) => (int)(((ulong)upperExclusiveBound * (GetNext() & 0xFFFFFFFFUL)) >> 32);
                public bool CoinFlip() => GetNext() < 0x8000000000000000UL;
                public bool OneIn(int x) => GetNext(x) == 0;

        }

	public class Game{
		public static class Screen{
			public static GLWindow gl;
			public static Surface textSurface;
			public static RNG rng;
		}
		const int ROWS = 33; //3840 / 40 is 96
		const int COLS = 96; // 2160 / 40 is 54   (which is 48/27 which is 16/9)
		//static int CELL_W = 40;
		//static int CELL_H = 40;

		static void Main(string[] args){
			Screen.rng = new RNG((ulong)DateTime.Now.Ticks);
			ToolkitOptions.Default.EnableHighResolution = false;
			int height_px = 2048; // Global.SCREEN_H * 16;
			int width_px = 2048; //Global.SCREEN_W * 8;
			//height_px = ROWS * CELL_H;
			//width_px = COLS * CELL_W;
			Screen.gl = new GLWindow(width_px,height_px,"msdf font test");
			Screen.gl.TimerFramesOffset = -(Screen.rng.GetNext(8888888));
			Screen.gl.SetWorldUnitsPerScreen(COLS, ROWS);
			float r16_9 = 16.0f / 9.0f;
			float r4_3 = 4.0f / 3.0f;
			Screen.gl.WindowSizeRules = new ResizeRules{
				//Constant = true, SnapWidth = 400, SnapHeight = -300
				MinHeight = 600, MinWidth = 800,
				//, SnapWidth = 20, SnapHeight = 20
				RatioRequirement = ResizeRules.RatioType.Range,
				RatioMin = Math.Min(r16_9, r4_3),
				RatioMax = Math.Max(r16_9, r4_3)
				//RatioWidth = 1, RatioHeight = 3
			};
			Screen.gl.ViewportSizeRules = new ResizeRules{
				//Constant = true, SnapWidth = 500, SnapHeight = 250,
				//RatioRequirement = ResizeRules.RatioType.Exact, RatioWidth = 1, RatioHeight = 1
				MinHeight = 1000, MinWidth = 1400
			};
			Screen.gl.NoShrinkToFit = true;
			//Screen.gl.ResizingPreference = ResizeOption.SnapWindow; Screen.gl.SnapHeight = Screen.gl.SnapWidth = 100;//todo //ResizeOption.StretchToFit;
			//Screen.gl.ResizingFullScreenPreference = ResizeOption.AddBorder;
			Screen.gl.FinalResize += Screen.gl.DefaultHandleResize;
			//Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/a-starry-msdf.png",TextureMinFilter.Nearest, TextureMagFilter.Linear, false,ShaderCollection.GetMsdfFS/*_todoplasma*/(2048, 1),false,2,4,4);
			//Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/PxPlus_IBM_VGA9-msdf_smaller.png",TextureMinFilter.Nearest, TextureMagFilter.Linear,false,ShaderCollection.GetMsdfFS(2048, 2),false,2,4,4);
			//Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/PxPlus_IBM_VGA9-msdf_smaller.png",TextureMinFilter.Nearest, TextureMagFilter.Linear,false,ShaderCollection.Noise(),false,2,4,4);
			Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/PxPlus_IBM_VGA9-msdf_smaller.png",TextureMinFilter.Nearest, TextureMagFilter.Linear,false,ShaderCollection.GetMsdfFS_Plasma(2048, 2),false,2,4,4);
			Shader sh2 = Shader.Create(ShaderCollection.GetGrayscaleMsdfFS(2048, 1));
			//Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/Iosevka-msdf.png",false,Shader.MsdfFS(),false,2,4,4);
			//SpriteType.DefineSpriteAcross(Screen.textSurface, 28, 50, 51);
			//Screen.textSurface.texture.Sprite.Add(Get8x8FontSprite());
			Screen.textSurface.texture.Sprite.Add(GetFontSprite());
			Screen.textSurface.texture.Sprite[0].CalculateThroughIndex(1000);
			//Screen.textSurface.texture.Sprite.Add(GetFontSprite());
			//SpriteType.DefineSingleRowSprite(Screen.textSurface, 2048);
			CellLayout.CreateGrid(Screen.textSurface, COLS, ROWS);
			Screen.textSurface.InitializePositions(ROWS*COLS);
			Screen.textSurface.InitializeOtherDataForSingleLayout(ROWS*COLS, 0, 32, new List<float>{0.6f, 0.6f,0.6f,0.6f},new List<float>{1f,1f,1f,1f});
			//Screen.gl.Surfaces.Add(Screen.textSurface);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha,BlendingFactor.OneMinusSrcAlpha);
			Screen.gl.Visible = true;
			MakeFakeMap();
			//SetGlyphs();
			//SetupDisplace();
			//TestPerf();
			while(Screen.gl.WindowUpdate()){
				Thread.Sleep(10);
				//Displace();
				if(Screen.gl.IsExiting || Screen.gl.KeyIsDown(Key.Escape)){
					Screen.gl.Close();
					return;
				}
				else if(Screen.gl.KeyIsDown(Key.W)){
					Screen.gl.Viewport = new System.Drawing.Rectangle(Screen.gl.Viewport.X, Screen.gl.Viewport.Y, Screen.gl.Viewport.Width-4, Screen.gl.Viewport.Height-4);
				}
				else if(Screen.gl.KeyIsDown(Key.S)){
					Screen.gl.Viewport = new System.Drawing.Rectangle(Screen.gl.Viewport.X, Screen.gl.Viewport.Y, Screen.gl.Viewport.Width+4, Screen.gl.Viewport.Height+4);

				}
				else if(Screen.gl.KeyIsDown(Key.D)){
					Screen.textSurface.ChangeOffsetInWorldUnits(1, 0);
				}
				else if(Screen.gl.KeyIsDown(Key.B)){
					if(shcounter++ >= 5){
						Shader temp = Screen.textSurface.shader;
						Screen.textSurface.shader = sh2;
						sh2 = temp;
						shcounter = 0;
					}
				}
			}
		}
		static int shcounter =0;
		static void TestPerf(){
			Screen.gl.VSync = VSyncMode.Off;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
			timer.Start();
			int num = 10000;
			while(num-- > 0){
				SetGlyphs();
			}
			while(num-- > 0 && Screen.gl.WindowUpdate()){
				Thread.Sleep(1);
				if(Screen.gl.IsExiting || Screen.gl.KeyIsDown(Key.Escape)){
					Screen.gl.Close();
					return;
				}
				SetGlyphs();
			}
			timer.Stop();
			Console.WriteLine(timer.ElapsedTicks);

			//test 2 is the array init for sprite type...
		}
		/*static float[] x_f, y_f;
		static int[] x_displace, y_displace;
		static int displaceCounter=0;
		static void Displace(){
			if(displaceCounter > 0){
				displaceCounter--;
				for(int n=0;n<29*89;++n){
					if(x_displace[n] > 0) x_displace[n] = x_displace[n] - 1;
					if(x_displace[n] < 0) x_displace[n] = x_displace[n] + 1;
					if(y_displace[n] > 0) y_displace[n] = y_displace[n] - 1;
					if(y_displace[n] < 0) y_displace[n] = y_displace[n] + 1;
					Screen.textSurface.DefaultUpdatePositions();
				}
				return;
			}
			displaceCounter = 10;
			for(int n=0;n<29*89;++n){
				x_displace[n] = Screen.rng.GetNext(21) - 10;
				y_displace[n] = Screen.rng.GetNext(21) - 10;
			}
			Screen.textSurface.DefaultUpdatePositions();
		}
		static void SetupDisplace(){
			x_f = new float[28*88];
			y_f = new float[28*88];
			for(int n=0;n<28*88;++n){
				x_f[n] = Screen.textSurface.layouts[0].X(n);
				y_f[n] = Screen.textSurface.layouts[0].Y(n);
			}
			x_displace = new int[29*89];
			y_displace = new int[29*89];
			Screen.textSurface.layouts[0].X = idx => getNewXY(idx, true);
			Screen.textSurface.layouts[0].Y = idx => getNewXY(idx, false);
		}
		static float getNewXY(int n, bool x){
			float[] origFloat = x? x_f : y_f;
			int[] displace = x? x_displace : y_displace;
			const float pxSize = 1.0f;// / 2048.0f;
			int increment = x ? 1 : 88;
			float diff = (displace[n] + displace[n+increment])/2.0f;
			return origFloat[n] + (diff * pxSize);
		}*/
		static SpriteType Get8x8FontSprite(){
			SpriteType s = new SpriteType();
			float px_width = 1.0f / (float)2048;
			float px_height = 1.0f / (float)2048;
			float texcoord_width = (float)52 * px_width;
			float texcoord_start_horiz = texcoord_width + (float)0 * px_width;
			float texcoord_height = (float)52 * px_height;
			float texcoord_start_vert = texcoord_height + (float)0 * px_height;
			s.X = idx => (idx % 39) * texcoord_start_horiz + (0.5f * px_width);
			s.Y = idx => (idx / 39) * texcoord_start_vert + (0.5f * px_height);
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			return s;
		}
		static SpriteType GetFontSprite(){
			SpriteType s = new SpriteType();
			float px_width = 1.0f / (float)2048;
			float px_height = 1.0f / (float)2048;
			float texcoord_width = (float)32 * px_width;
			float texcoord_start_horiz = texcoord_width + (float)0 * px_width;
			float texcoord_height = (float)54 * px_height;
			float texcoord_start_vert = texcoord_height + (float)0 * px_height;
			s.X = idx => idx * texcoord_start_horiz + (0.5f * px_width);
			s.Y = idx => (idx / 64) * texcoord_start_vert + (0.5f * px_height);
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			return s;
		}
		static SpriteType WorkingGetFontSprite(){
			SpriteType s = new SpriteType();
			float px_width = 1.0f / (float)2048;
			float px_height = 1.0f / (float)2048;
			float texcoord_width = (float)32 * px_width;
			float texcoord_start_horiz = texcoord_width + (float)0 * px_width;
			float texcoord_height = (float)54 * px_height;
			float texcoord_start_vert = texcoord_height + (float)0 * px_height;
			s.X = idx => idx * texcoord_start_horiz + (0.5f * px_width);
			s.Y = idx => (idx / 64) * texcoord_start_vert + (0.5f * px_height);
			s.SpriteWidth = texcoord_width;
			s.SpriteHeight = texcoord_height;
			return s;
		}
		static int nextIdx = 0;
		static int getNextSprite() => nextIdx++;
		static int getNextSprite2() => Screen.rng.GetNext(110); //768
		static float getNextFgColor() => 0.5f + ((float)Screen.rng.GetNext(100)+1) / 200f;
		static float getNextBgColor() => 0.2f + ((float)Screen.rng.GetNext(100)+1) / 333f;
		static float getColor2() => 0.7f;// Screen.rng.OneIn(3)? 1.0f : 0.2f;// 0.5f + ((float)Screen.rng.GetNext(100)+1) / 200f;
		static void SetGlyphs(){
			const int count = ROWS*COLS;
			int[] sprite_cols = new int[count];
			float[][] color_info = new float[2][];
			color_info[0] = new float[4 * count];
			color_info[1] = new float[4 * count];
			for(int n=0;n<count;++n){
				sprite_cols[n] = getNextSprite2(); // 35 is '#'

				int idx4 = n * 4;
				color_info[0][idx4] = getColor2();// getNextFgColor();// 0.5f + (float)(getNextColor() * 0.5f);
				color_info[0][idx4 + 1] = getColor2();//getNextFgColor();//0.5f + (float)(getNextColor() * 0.5f);
				color_info[0][idx4 + 2] = getColor2();//getNextFgColor();//0.5f + (float)(getNextColor() * 0.5f);
				color_info[0][idx4 + 3] = 1.0f;//0.5f + (float)(getNextColor() * 0.5f);
				color_info[1][idx4] = 0.0f;//getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 1] = 0.0f;//getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 2] = 0.0f;//getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 3] = 1.0f;//0.2f + (float)(getNextColor() * 0.3f);
			}
			Screen.gl.UpdateOtherVertexArray(Screen.textSurface, sprite_cols, color_info);
		}
		// '#' is 7?
		static void MakeFakeMap(){
			//let's do a 30x20 fake map, centered on theleft side...
			// do random wall/floors for an area in the right center...
			//then do blanks for the rest
			//48x54...
			char[,] map = new char[ROWS,COLS];
			int rows2 = ROWS/3;
			int cols2 = COLS/2;
			for(int i=0;i<rows2;++i){
				for(int j=0;j<cols2;++j){
					char ch = '.';
					if(Screen.rng.OneIn(3)) ch = '#';
					if(Screen.rng.OneIn(9)) ch = '~';
					map[10+i,COLS/2-5+j] = ch;
				}
			}
			string[] lines = fakeMap.Split(new char[]{'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
			int idx = 0;
			foreach(string line in lines){
				string l = line.PadRight(30);
				for(int n=0;n<l.Length;++n){
					map[idx, (cols2 - (COLS/3))+n] = l[n];
				}
				++idx;
			}
			const int count = ROWS*COLS;
			int[] sprite_cols = new int[count];
			float[][] color_info = new float[2][];
			color_info[0] = new float[4 * count];
			color_info[1] = new float[4 * count];
			for(int n=0;n<count;++n){
				int offset = 32; // offset = 4;
				int spr = offset; // space
				bool blue = false;
				bool gray = false;
				int x = n % COLS;
				int y = n / COLS;
				switch(map[y,x]){
					case '#':
					spr = 3 + offset;
					break;
					case '.':
					spr = 14 + offset;
					break;
					case '~':
					spr = 94 + offset;
					blue = true;
					break;
					case '+':
					spr = 11 + offset;
					gray = true;
					break;
					case ',':
					spr = 12 + offset;
					gray = true;
					break;
				}
				sprite_cols[n] = spr;

				int idx4 = n * 4;
				float rr = 0.8f;
				float gg = 0.8f;
				float bb = 0.8f;
				if(blue){
					rr = 0.1f;
					gg = 0.2f;
					bb = 0.9f;
				}
				else if(gray){
					rr = gg = bb = 0.4f;
				}
				if(spr == 4){
					rr = gg = bb = 0.0f;
				}
				color_info[0][idx4] = rr;
				color_info[0][idx4 + 1] = gg;
				color_info[0][idx4 + 2] = bb;
				color_info[0][idx4 + 3] = 1.0f;//0.5f + (float)(getNextColor() * 0.5f);
				color_info[1][idx4] = 0.0f;//getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 1] = 0.0f;//getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 2] = blue? 0.15f : 0.0f;//getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 3] = 1.0f;//0.2f + (float)(getNextColor() * 0.3f);
			}
			Screen.gl.UpdateOtherVertexArray(Screen.textSurface, sprite_cols, color_info);
		}
		static string fakeMap = @"##############################
#............................#
#.................#######.##.#
#.................###   #....#
#.................+.#   ######
###################.#
                  #.#
                  #.#
        ###########.##########
       ##.....................
       #................######
      ##......................
      #.................######
      #........,,,,,....#
      ##......,,,,,,,...######
       #....,,,,,,,,,,........
       ##....,,,,,,,,,..#.####
        #################.####
                        #.....
                        ######";
		static void WorkingSetGlyphs(){
			const int rows = 28;
			const int cols = 88;
			const int count = rows*cols;
			int[] sprite_cols = new int[count];
			float[][] color_info = new float[2][];
			color_info[0] = new float[4 * count];
			color_info[1] = new float[4 * count];
			for(int n=0;n<count;++n){
				sprite_cols[n] = getNextSprite();

				int idx4 = n * 4;
				color_info[0][idx4] = getNextFgColor();// 0.5f + (float)(getNextColor() * 0.5f);
				color_info[0][idx4 + 1] = getNextFgColor();//0.5f + (float)(getNextColor() * 0.5f);
				color_info[0][idx4 + 2] = getNextFgColor();//0.5f + (float)(getNextColor() * 0.5f);
				color_info[0][idx4 + 3] = 1.0f;//0.5f + (float)(getNextColor() * 0.5f);
				color_info[1][idx4] = getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 1] = getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 2] = getNextBgColor();//0.2f + (float)(getNextColor() * 0.3f);
				color_info[1][idx4 + 3] = 1.0f;//0.2f + (float)(getNextColor() * 0.3f);
			}
			Screen.gl.UpdateOtherVertexArray(Screen.textSurface, sprite_cols, color_info);
		}
			/*public static void UpdateGLBuffer(int start_row,int start_col,colorchar[,] array){
				int array_h = array.GetLength(0);
				int array_w = array.GetLength(1);
				int start_idx = start_col + start_row*Global.SCREEN_W;
				int end_idx = (start_col + array_w - 1) + (start_row + array_h - 1)*Global.SCREEN_W;
				int count = (end_idx - start_idx) + 1;
				int end_row = start_row + array_h - 1;
				int end_col = start_col + array_w - 1;
				//int[] sprite_rows = new int[count];
				int[] sprite_cols = new int[count];
				float[][] color_info = new float[2][];
				color_info[0] = new float[4 * count];
				color_info[1] = new float[4 * count];
				for(int n=0;n<count;++n){
						int row = (n + start_col) / Global.SCREEN_W + start_row; //screen coords
						int col = (n + start_col) % Global.SCREEN_W;
						colorchar cch = (row >= start_row && row <= end_row && col >= start_col && col <= end_col)? array[row-start_row,col-start_col] : memory[row,col];
						Color4 color = Colors.ConvertColor(cch.color);
						Color4 bgcolor = Colors.ConvertColor(cch.bgcolor);
						//sprite_rows[n] = 0;
						sprite_cols[n] = (int)cch.c;
						int idx4 = n * 4;
						color_info[0][idx4] = color.R;
						color_info[0][idx4 + 1] = color.G;
						color_info[0][idx4 + 2] = color.B;
						color_info[0][idx4 + 3] = color.A;
						color_info[1][idx4] = bgcolor.R;
						color_info[1][idx4 + 1] = bgcolor.G;
						color_info[1][idx4 + 2] = bgcolor.B;
						color_info[1][idx4 + 3] = bgcolor.A;
				}
				gl.UpdateOtherVertexArray(textSurface,start_idx,sprite_cols,null,color_info);
				//Game.gl.UpdateVertexArray(start_row,start_col,GLGame.text_surface,sprite_rows,sprite_cols,color_info);
			}*/

		static void WorkingMain(string[] args){
			ToolkitOptions.Default.EnableHighResolution = false;
			int height_px = 2048; // Global.SCREEN_H * 16;
			int width_px = 2048; //Global.SCREEN_W * 8;
			Screen.gl = new GLWindow(width_px,height_px,"msdf font test");
			//Screen.gl.ResizingPreference = ResizeOption.StretchToFit;
			//Screen.gl.ResizingFullScreenPreference = ResizeOption.AddBorder;
			Screen.gl.FinalResize += Screen.gl.DefaultHandleResize;
			//Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/PxPlus_IBM_VGA9-msdf_smaller.png",false,ShaderCollection.MsdfFS(),false,2,4,4);
			//Screen.textSurface = Surface.Create(Screen.gl, @"/home/void/Downloads/Iosevka-msdf.png",false,Shader.MsdfFS(),false,2,4,4);
			//SpriteType.DefineSpriteAcross(Screen.textSurface, 24, 240,2);
			SpriteType.DefineSingleRowSprite(Screen.textSurface, 2048);
			CellLayout.CreateGrid(Screen.textSurface,1, 1, 2048, 2048,0,0);
			/*Screen.textSurface.SetEasyLayoutCounts(1);
			Screen.textSurface.DefaultUpdatePositions();
			Screen.textSurface.SetDefaultSpriteType(0);
			Screen.textSurface.SetDefaultSprite(0);
			Screen.textSurface.SetDefaultOtherData(new List<float>{0.6f, 0.6f,0.6f,0.6f},new List<float>{1f,1f,1f,1f});
			Screen.textSurface.DefaultUpdateOtherData();*/
			Screen.gl.Surfaces.Add(Screen.textSurface);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactor.SrcAlpha,BlendingFactor.OneMinusSrcAlpha);
			Screen.gl.Visible = true;
			while(Screen.gl.WindowUpdate()){
				Thread.Sleep(50);
				if(Screen.gl.IsExiting || Screen.gl.KeyIsDown(Key.Escape)){
					Screen.gl.Close();
					return;
				}
			}
		}
	}
}
