using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

// @Debug
// using System.Diagnostics;
// using System.Runtime.ExceptionServices;
// using System.Security.Cryptography;

//
// Todo:
// - Add an 8BMT executable icon.
// - Add a credits sequence after completing every level.
// - Extensions, if we have too much time on our hands:
// -   ???: use yoku as umbrellas to cover for rain
// -   swi 2: timer bar trolling?
// -   aero: maze level with snakes
//

class HaiGame: Game {
  [STAThread]
  static void Main(string[] arguments) {
    SDL2.SDL.SDL_SetHint("SDL_WINDOWS_DPI_AWARENESS", "permonitorv2");

    using(HaiGame g = new HaiGame()) {
      g.Run();
    }
  }

  struct Config {
    public float mainVolume;
  }

  //
  //
  //

  const byte TILE_SIZE = 16;
  const byte TILE_MAP_SIZE = 19;
  const byte TILE_MAP_COLUMNS = 8;
  static Vector2 tileHalfSize = new Vector2((float)TILE_SIZE * 0.5f);

  struct KerningPair {
    public short c0;
    public short c1;
    public float kern;

    public KerningPair(short c0, short c1, float kern) {
      this.c0 = c0;
      this.c1 = c1;
      this.kern = kern;
    }
  };

  struct FontGlyph {
    public int x0;
    public int y0;
    public int x1;
    public int y1;
    public float offsetX;
    public float offsetY;
    public float advanceX;

    public FontGlyph(int x0, int y0, int x1, int y1, float offsetX, float offsetY, float advanceX) {
      this.x0 = x0;
      this.y0 = y0;
      this.x1 = x1;
      this.y1 = y1;
      this.offsetX = offsetX;
      this.offsetY = offsetY;
      this.advanceX = advanceX;
    }
  }

  struct Font {
    public KerningPair[] kerning;
    public FontGlyph[] glyphs;

    public Texture2D texture;

    public int pixelHeight;
    public float ascent;
    public float descent;
    public float lineGap;

    public Font(Texture2D texture, KerningPair[] kerning, FontGlyph[] glyphs, float ascent, float descent, float lineGap) {
      this.texture = texture;
      this.kerning = kerning;
      this.glyphs = glyphs;
      this.pixelHeight = (int)(ascent - descent);
      this.ascent = ascent;
      this.descent = descent;
      this.lineGap = lineGap;
    }

    public float GetLineYAdvance() {
      return ascent - descent + lineGap;
    }
  }

  const float COMMAND_FONT_ASCENT = 50.666668f;
  const float COMMAND_FONT_DESCENT = -13.333334f;
  const float COMMAND_FONT_LINE_GAP = 0.000000f;
  static readonly KerningPair[] commandFontKerning = {
    new KerningPair(32,84,-1.600000f), new KerningPair(34,34,-0.986667f), new KerningPair(34,39,-0.986667f), new KerningPair(34,65,-3.200000f), new KerningPair(34,97,-1.333333f), new KerningPair(34,99,-1.573333f), new KerningPair(34,100,-1.573333f), new KerningPair(34,101,-1.573333f), new KerningPair(34,103,-1.573333f), new KerningPair(34,109,-0.533333f), new KerningPair(34,110,-0.533333f), new KerningPair(34,111,-2.426667f), new KerningPair(34,112,-0.533333f), new KerningPair(34,113,-1.573333f), new KerningPair(34,115,-2.453333f), 
    new KerningPair(34,119,0.293333f), new KerningPair(39,34,-0.986667f), new KerningPair(39,39,-0.986667f), new KerningPair(39,65,-3.200000f), new KerningPair(39,97,-1.333333f), new KerningPair(39,99,-1.573333f), new KerningPair(39,100,-1.573333f), new KerningPair(39,101,-1.573333f), new KerningPair(39,103,-1.573333f), new KerningPair(39,109,-0.533333f), new KerningPair(39,110,-0.533333f), new KerningPair(39,111,-2.426667f), new KerningPair(39,112,-0.533333f), new KerningPair(39,113,-1.573333f), new KerningPair(39,115,-2.453333f), new KerningPair(39,119,0.293333f), 
    new KerningPair(40,86,0.533333f), new KerningPair(40,87,1.013333f), new KerningPair(40,89,0.586667f), new KerningPair(44,34,-7.600000f), new KerningPair(44,39,-7.600000f), new KerningPair(46,34,-7.600000f), new KerningPair(46,39,-7.600000f), new KerningPair(47,47,-6.613334f), new KerningPair(65,34,-3.200000f), new KerningPair(65,39,-3.200000f), new KerningPair(65,63,-2.160000f), new KerningPair(65,67,-0.560000f), new KerningPair(65,71,-0.560000f), new KerningPair(65,79,-0.560000f), new KerningPair(65,81,-0.560000f), new KerningPair(65,84,-3.200000f), 
    new KerningPair(65,85,-0.453333f), new KerningPair(65,86,-2.053333f), new KerningPair(65,87,-1.040000f), new KerningPair(65,89,-4.000000f), new KerningPair(65,109,-1.280000f), new KerningPair(65,110,-1.280000f), new KerningPair(65,111,-0.320000f), new KerningPair(65,112,-1.280000f), new KerningPair(65,116,-0.453333f), new KerningPair(65,117,-0.293333f), new KerningPair(65,118,-1.333333f), new KerningPair(65,119,-0.880000f), new KerningPair(65,121,-1.333333f), new KerningPair(65,122,0.320000f), new KerningPair(66,84,-0.720000f), new KerningPair(66,86,-0.640000f), 
    new KerningPair(66,89,-1.466667f), new KerningPair(67,41,-0.693333f), new KerningPair(67,84,-0.773333f), new KerningPair(67,93,-0.320000f), new KerningPair(67,125,-0.453333f), new KerningPair(68,44,-3.253333f), new KerningPair(68,46,-3.253333f), new KerningPair(68,65,-0.560000f), new KerningPair(68,84,-2.266667f), new KerningPair(68,86,-0.586667f), new KerningPair(68,88,-0.586667f), new KerningPair(68,89,-1.146667f), new KerningPair(68,90,-0.613333f), new KerningPair(69,84,0.533333f), new KerningPair(69,99,-0.506667f), new KerningPair(69,100,-0.506667f), 
    new KerningPair(69,101,-0.506667f), new KerningPair(69,102,-0.480000f), new KerningPair(69,103,-0.506667f), new KerningPair(69,111,-0.506667f), new KerningPair(69,113,-0.506667f), new KerningPair(69,117,-0.453333f), new KerningPair(69,118,-0.693333f), new KerningPair(69,119,-0.586667f), new KerningPair(69,121,-0.693333f), new KerningPair(70,44,-7.306667f), new KerningPair(70,46,-7.306667f), new KerningPair(70,65,-5.120000f), new KerningPair(70,74,-5.546667f), new KerningPair(70,84,0.533333f), new KerningPair(70,97,-0.906667f), new KerningPair(70,99,-0.560000f), 
    new KerningPair(70,100,-0.560000f), new KerningPair(70,101,-0.560000f), new KerningPair(70,103,-0.560000f), new KerningPair(70,111,-0.560000f), new KerningPair(70,113,-0.560000f), new KerningPair(70,114,-0.693333f), new KerningPair(70,117,-0.586667f), new KerningPair(70,118,-0.640000f), new KerningPair(70,121,-0.640000f), new KerningPair(72,65,0.480000f), new KerningPair(72,84,-0.773333f), new KerningPair(72,88,0.453333f), new KerningPair(72,89,-0.746667f), new KerningPair(73,65,0.480000f), new KerningPair(73,84,-0.773333f), new KerningPair(73,88,0.453333f), 
    new KerningPair(73,89,-0.746667f), new KerningPair(74,65,-0.586667f), new KerningPair(75,45,-4.373333f), new KerningPair(75,67,-0.826667f), new KerningPair(75,71,-0.826667f), new KerningPair(75,79,-0.826667f), new KerningPair(75,81,-0.826667f), new KerningPair(75,99,-0.693333f), new KerningPair(75,100,-0.693333f), new KerningPair(75,101,-0.693333f), new KerningPair(75,103,-0.693333f), new KerningPair(75,111,-0.720000f), new KerningPair(75,113,-0.693333f), new KerningPair(75,117,-0.613333f), new KerningPair(75,118,-1.066667f), new KerningPair(75,119,-1.680000f), 
    new KerningPair(75,121,-1.066667f), new KerningPair(76,34,-7.680000f), new KerningPair(76,39,-7.680000f), new KerningPair(76,65,0.506667f), new KerningPair(76,67,-1.706667f), new KerningPair(76,71,-1.706667f), new KerningPair(76,79,-1.706667f), new KerningPair(76,81,-1.706667f), new KerningPair(76,84,-5.466667f), new KerningPair(76,85,-0.640000f), new KerningPair(76,86,-5.493333f), new KerningPair(76,87,-2.480000f), new KerningPair(76,89,-7.440000f), new KerningPair(76,117,-0.373333f), new KerningPair(76,118,-3.280000f), new KerningPair(76,119,-1.386667f), 
    new KerningPair(76,121,-3.280000f), new KerningPair(77,65,0.480000f), new KerningPair(77,84,-0.773333f), new KerningPair(77,88,0.453333f), new KerningPair(77,89,-0.746667f), new KerningPair(78,65,0.480000f), new KerningPair(78,84,-0.773333f), new KerningPair(78,88,0.453333f), new KerningPair(78,89,-0.746667f), new KerningPair(79,44,-3.253333f), new KerningPair(79,46,-3.253333f), new KerningPair(79,65,-0.560000f), new KerningPair(79,84,-2.266667f), new KerningPair(79,86,-0.586667f), new KerningPair(79,88,-0.586667f), new KerningPair(79,89,-1.146667f), 
    new KerningPair(79,90,-0.613333f), new KerningPair(80,44,-10.773334f), new KerningPair(80,46,-10.773334f), new KerningPair(80,65,-4.746667f), new KerningPair(80,74,-4.906667f), new KerningPair(80,88,-1.360000f), new KerningPair(80,90,-0.960000f), new KerningPair(80,97,-0.293333f), new KerningPair(80,99,-0.346667f), new KerningPair(80,100,-0.346667f), new KerningPair(80,101,-0.346667f), new KerningPair(80,103,-0.346667f), new KerningPair(80,111,-0.346667f), new KerningPair(80,113,-0.346667f), new KerningPair(80,116,0.373333f), new KerningPair(80,118,0.400000f), 
    new KerningPair(80,121,0.400000f), new KerningPair(81,84,-0.880000f), new KerningPair(81,86,-0.746667f), new KerningPair(81,87,-0.533333f), new KerningPair(81,89,-0.933333f), new KerningPair(82,84,-1.333333f), new KerningPair(82,86,-0.506667f), new KerningPair(82,89,-1.280000f), new KerningPair(84,32,-1.600000f), new KerningPair(84,44,-6.880000f), new KerningPair(84,45,-7.253334f), new KerningPair(84,46,-6.880000f), new KerningPair(84,65,-3.200000f), new KerningPair(84,67,-0.746667f), new KerningPair(84,71,-0.746667f), new KerningPair(84,74,-5.760000f), 
    new KerningPair(84,79,-0.746667f), new KerningPair(84,81,-0.746667f), new KerningPair(84,83,-0.426667f), new KerningPair(84,84,0.426667f), new KerningPair(84,86,0.426667f), new KerningPair(84,87,0.400000f), new KerningPair(84,89,0.426667f), new KerningPair(84,97,-4.480000f), new KerningPair(84,99,-2.373333f), new KerningPair(84,100,-2.373333f), new KerningPair(84,101,-2.373333f), new KerningPair(84,103,-2.373333f), new KerningPair(84,109,-2.373333f), new KerningPair(84,110,-2.373333f), new KerningPair(84,111,-5.546667f), new KerningPair(84,112,-2.373333f), 
    new KerningPair(84,113,-2.373333f), new KerningPair(84,114,-1.733333f), new KerningPair(84,115,-2.026667f), new KerningPair(84,117,-1.733333f), new KerningPair(84,118,-2.186667f), new KerningPair(84,119,-1.253333f), new KerningPair(84,120,-2.053333f), new KerningPair(84,121,-2.186667f), new KerningPair(84,122,-1.600000f), new KerningPair(85,65,-0.586667f), new KerningPair(86,41,0.533333f), new KerningPair(86,44,-5.733334f), new KerningPair(86,45,-4.186667f), new KerningPair(86,46,-5.733334f), new KerningPair(86,65,-2.000000f), new KerningPair(86,67,-0.346667f), 
    new KerningPair(86,71,-0.346667f), new KerningPair(86,79,-0.346667f), new KerningPair(86,81,-0.346667f), new KerningPair(86,93,0.453333f), new KerningPair(86,97,-1.226667f), new KerningPair(86,99,-1.173333f), new KerningPair(86,100,-1.173333f), new KerningPair(86,101,-1.173333f), new KerningPair(86,103,-1.173333f), new KerningPair(86,111,-1.226667f), new KerningPair(86,113,-1.173333f), new KerningPair(86,114,-0.800000f), new KerningPair(86,117,-0.746667f), new KerningPair(86,118,-0.293333f), new KerningPair(86,121,-0.293333f), new KerningPair(86,125,0.506667f), 
    new KerningPair(87,41,0.400000f), new KerningPair(87,44,-3.813334f), new KerningPair(87,45,-1.600000f), new KerningPair(87,46,-3.813334f), new KerningPair(87,65,-1.146667f), new KerningPair(87,84,0.373333f), new KerningPair(87,93,0.320000f), new KerningPair(87,97,-0.880000f), new KerningPair(87,99,-0.826667f), new KerningPair(87,100,-0.826667f), new KerningPair(87,101,-0.826667f), new KerningPair(87,103,-0.826667f), new KerningPair(87,111,-0.826667f), new KerningPair(87,113,-0.826667f), new KerningPair(87,114,-0.560000f), new KerningPair(87,117,-0.506667f), 
    new KerningPair(87,125,0.373333f), new KerningPair(88,45,-4.160000f), new KerningPair(88,67,-0.666667f), new KerningPair(88,71,-0.666667f), new KerningPair(88,79,-0.666667f), new KerningPair(88,81,-0.666667f), new KerningPair(88,86,0.373333f), new KerningPair(88,99,-0.693333f), new KerningPair(88,100,-0.693333f), new KerningPair(88,101,-0.693333f), new KerningPair(88,103,-0.693333f), new KerningPair(88,111,-0.560000f), new KerningPair(88,113,-0.693333f), new KerningPair(88,117,-0.560000f), new KerningPair(88,118,-0.826667f), new KerningPair(88,121,-0.826667f), 
    new KerningPair(89,38,-0.800000f), new KerningPair(89,41,0.533333f), new KerningPair(89,42,-1.306667f), new KerningPair(89,44,-6.160000f), new KerningPair(89,45,-4.053333f), new KerningPair(89,46,-6.160000f), new KerningPair(89,65,-4.000000f), new KerningPair(89,67,-0.773333f), new KerningPair(89,71,-0.773333f), new KerningPair(89,74,-2.560000f), new KerningPair(89,79,-0.773333f), new KerningPair(89,81,-0.773333f), new KerningPair(89,83,-0.426667f), new KerningPair(89,84,0.453333f), new KerningPair(89,85,-2.560000f), new KerningPair(89,86,0.480000f), 
    new KerningPair(89,87,0.453333f), new KerningPair(89,88,0.346667f), new KerningPair(89,89,0.480000f), new KerningPair(89,93,0.480000f), new KerningPair(89,97,-1.680000f), new KerningPair(89,99,-1.733333f), new KerningPair(89,100,-1.733333f), new KerningPair(89,101,-1.733333f), new KerningPair(89,102,-0.586667f), new KerningPair(89,103,-1.733333f), new KerningPair(89,109,-1.066667f), new KerningPair(89,110,-1.066667f), new KerningPair(89,111,-1.733333f), new KerningPair(89,112,-1.066667f), new KerningPair(89,113,-1.733333f), new KerningPair(89,114,-1.066667f), 
    new KerningPair(89,115,-1.546667f), new KerningPair(89,116,-0.586667f), new KerningPair(89,117,-1.040000f), new KerningPair(89,118,-0.533333f), new KerningPair(89,120,-0.613333f), new KerningPair(89,121,-0.533333f), new KerningPair(89,122,-0.800000f), new KerningPair(89,125,0.506667f), new KerningPair(90,65,0.346667f), new KerningPair(90,67,-0.693333f), new KerningPair(90,71,-0.693333f), new KerningPair(90,79,-0.693333f), new KerningPair(90,81,-0.693333f), new KerningPair(90,99,-0.560000f), new KerningPair(90,100,-0.560000f), new KerningPair(90,101,-0.560000f), 
    new KerningPair(90,103,-0.560000f), new KerningPair(90,111,-0.560000f), new KerningPair(90,113,-0.560000f), new KerningPair(90,117,-0.506667f), new KerningPair(90,118,-0.720000f), new KerningPair(90,119,-0.720000f), new KerningPair(90,121,-0.720000f), new KerningPair(91,74,-0.480000f), new KerningPair(91,85,-0.480000f), new KerningPair(97,34,-0.453333f), new KerningPair(97,39,-0.453333f), new KerningPair(97,118,-0.400000f), new KerningPair(97,121,-0.400000f), new KerningPair(98,34,-0.773333f), new KerningPair(98,39,-0.773333f), new KerningPair(98,118,-0.293333f), 
    new KerningPair(98,120,-0.400000f), new KerningPair(98,121,-0.293333f), new KerningPair(98,122,-0.400000f), new KerningPair(99,34,-0.293333f), new KerningPair(99,39,-0.293333f), new KerningPair(101,34,-0.373333f), new KerningPair(101,39,-0.373333f), new KerningPair(101,118,-0.346667f), new KerningPair(101,121,-0.346667f), new KerningPair(102,34,0.426667f), new KerningPair(102,39,0.426667f), new KerningPair(102,41,0.533333f), new KerningPair(102,93,0.480000f), new KerningPair(102,99,-0.640000f), new KerningPair(102,100,-0.640000f), new KerningPair(102,101,-0.640000f), 
    new KerningPair(102,103,-0.640000f), new KerningPair(102,113,-0.640000f), new KerningPair(102,125,0.506667f), new KerningPair(104,34,-2.133333f), new KerningPair(104,39,-2.133333f), new KerningPair(107,99,-0.533333f), new KerningPair(107,100,-0.533333f), new KerningPair(107,101,-0.533333f), new KerningPair(107,103,-0.533333f), new KerningPair(107,113,-0.533333f), new KerningPair(109,34,-2.133333f), new KerningPair(109,39,-2.133333f), new KerningPair(110,34,-2.133333f), new KerningPair(110,39,-2.133333f), new KerningPair(111,34,-2.346667f), new KerningPair(111,39,-2.346667f), 
    new KerningPair(111,118,-0.400000f), new KerningPair(111,120,-0.560000f), new KerningPair(111,121,-0.400000f), new KerningPair(111,122,-0.426667f), new KerningPair(112,34,-0.773333f), new KerningPair(112,39,-0.773333f), new KerningPair(112,118,-0.293333f), new KerningPair(112,120,-0.400000f), new KerningPair(112,121,-0.293333f), new KerningPair(112,122,-0.400000f), new KerningPair(114,34,0.426667f), new KerningPair(114,39,0.426667f), new KerningPair(114,44,-4.613333f), new KerningPair(114,46,-4.613333f), new KerningPair(114,97,-0.800000f), new KerningPair(114,99,-0.506667f), 
    new KerningPair(114,100,-0.506667f), new KerningPair(114,101,-0.506667f), new KerningPair(114,102,0.533333f), new KerningPair(114,103,-0.506667f), new KerningPair(114,111,-0.960000f), new KerningPair(114,113,-0.506667f), new KerningPair(114,116,1.333333f), new KerningPair(114,118,0.480000f), new KerningPair(114,119,0.453333f), new KerningPair(114,121,0.480000f), new KerningPair(116,111,-0.800000f), new KerningPair(118,34,0.400000f), new KerningPair(118,39,0.400000f), new KerningPair(118,44,-4.453333f), new KerningPair(118,46,-4.453333f), new KerningPair(118,97,-0.400000f), 
    new KerningPair(118,99,-0.346667f), new KerningPair(118,100,-0.346667f), new KerningPair(118,101,-0.346667f), new KerningPair(118,102,0.346667f), new KerningPair(118,103,-0.346667f), new KerningPair(118,111,-0.400000f), new KerningPair(118,113,-0.346667f), new KerningPair(119,44,-3.306667f), new KerningPair(119,46,-3.306667f), new KerningPair(120,99,-0.533333f), new KerningPair(120,100,-0.533333f), new KerningPair(120,101,-0.533333f), new KerningPair(120,103,-0.533333f), new KerningPair(120,111,-1.066667f), new KerningPair(120,113,-0.533333f), new KerningPair(121,34,0.400000f), 
    new KerningPair(121,39,0.400000f), new KerningPair(121,44,-4.453333f), new KerningPair(121,46,-4.453333f), new KerningPair(121,97,-0.400000f), new KerningPair(121,99,-0.346667f), new KerningPair(121,100,-0.346667f), new KerningPair(121,101,-0.346667f), new KerningPair(121,102,0.346667f), new KerningPair(121,103,-0.346667f), new KerningPair(121,111,-0.400000f), new KerningPair(121,113,-0.346667f), new KerningPair(122,99,-0.426667f), new KerningPair(122,100,-0.426667f), new KerningPair(122,101,-0.426667f), new KerningPair(122,103,-0.426667f), new KerningPair(122,111,-0.426667f), 
    new KerningPair(122,113,-0.426667f), new KerningPair(123,74,-0.533333f), new KerningPair(123,85,-0.533333f), 
  };
  static readonly FontGlyph[] commandFontGlyphs = {
    new FontGlyph(283, 167, 283, 167, 0.000000f, 0.000000f, 13.600000f), new FontGlyph(169, 53, 178, 93, 3.000000f, -39.000000f, 14.853333f), new FontGlyph(496, 184, 511, 199, 1.000000f, -41.000000f, 17.520000f), new FontGlyph(458, 126, 489, 165, 1.000000f, -39.000000f, 32.533333f), new FontGlyph(159, 1, 186, 52, 2.000000f, -45.000000f, 31.333334f), new FontGlyph(439, 1, 475, 42, 2.000000f, -40.000000f, 40.320000f), new FontGlyph(476, 1, 511, 42, 1.000000f, -40.000000f, 35.840000f), 
    new FontGlyph(275, 167, 282, 182, 1.000000f, -41.000000f, 8.826667f), new FontGlyph(18, 1, 34, 58, 3.000000f, -44.000000f, 19.173334f), new FontGlyph(1, 1, 17, 58, 1.000000f, -44.000000f, 19.253334f), new FontGlyph(77, 175, 102, 199, 0.000000f, -39.000000f, 24.746668f), new FontGlyph(413, 166, 441, 196, 1.000000f, -33.000000f, 29.813334f), new FontGlyph(496, 166, 507, 183, 0.000000f, -7.000000f, 13.333334f), new FontGlyph(395, 197, 411, 204, 2.000000f, -20.000000f, 21.173334f), new FontGlyph(200, 171, 210, 180, 3.000000f, -8.000000f, 15.866667f), 
    new FontGlyph(255, 1, 276, 44, -1.000000f, -39.000000f, 20.400000f), new FontGlyph(187, 49, 214, 90, 2.000000f, -40.000000f, 31.333334f), new FontGlyph(490, 126, 508, 165, 4.000000f, -39.000000f, 31.333334f), new FontGlyph(439, 85, 468, 125, 1.000000f, -40.000000f, 31.333334f), new FontGlyph(400, 44, 428, 85, 1.000000f, -40.000000f, 31.333334f), new FontGlyph(157, 131, 186, 170, 1.000000f, -39.000000f, 31.333334f), new FontGlyph(469, 85, 497, 125, 2.000000f, -39.000000f, 31.333334f), new FontGlyph(229, 45, 257, 86, 2.000000f, -40.000000f, 31.333334f), 
    new FontGlyph(246, 127, 275, 166, 1.000000f, -39.000000f, 31.333334f), new FontGlyph(113, 53, 140, 94, 2.000000f, -40.000000f, 31.333334f), new FontGlyph(141, 53, 168, 94, 2.000000f, -40.000000f, 31.333334f), new FontGlyph(142, 135, 152, 166, 3.000000f, -30.000000f, 15.413334f), new FontGlyph(215, 49, 226, 89, 1.000000f, -30.000000f, 14.320001f), new FontGlyph(27, 172, 51, 199, 1.000000f, -30.000000f, 27.786667f), new FontGlyph(103, 196, 128, 215, 3.000000f, -27.000000f, 31.253334f), new FontGlyph(52, 175, 76, 202, 3.000000f, -30.000000f, 28.213333f), 
    new FontGlyph(35, 58, 60, 99, 1.000000f, -40.000000f, 27.173334f), new FontGlyph(113, 1, 158, 52, 2.000000f, -38.000000f, 48.880001f), new FontGlyph(388, 86, 425, 125, 0.000000f, -39.000000f, 36.746666f), new FontGlyph(215, 127, 245, 166, 3.000000f, -39.000000f, 34.853333f), new FontGlyph(277, 44, 309, 85, 2.000000f, -40.000000f, 35.733334f), new FontGlyph(362, 126, 393, 165, 3.000000f, -39.000000f, 35.493336f), new FontGlyph(187, 131, 214, 170, 3.000000f, -39.000000f, 30.720001f), new FontGlyph(96, 135, 122, 174, 3.000000f, -39.000000f, 29.920000f), 
    new FontGlyph(474, 43, 507, 84, 2.000000f, -40.000000f, 37.200001f), new FontGlyph(35, 100, 68, 139, 3.000000f, -39.000000f, 38.586666f), new FontGlyph(276, 127, 285, 166, 3.000000f, -39.000000f, 15.920000f), new FontGlyph(269, 86, 296, 126, 1.000000f, -39.000000f, 30.506668f), new FontGlyph(1, 100, 34, 139, 3.000000f, -39.000000f, 34.666668f), new FontGlyph(69, 135, 95, 174, 3.000000f, -39.000000f, 29.573334f), new FontGlyph(345, 86, 387, 125, 3.000000f, -39.000000f, 47.840000f), new FontGlyph(123, 95, 156, 134, 3.000000f, -39.000000f, 38.560001f), 
    new FontGlyph(439, 43, 473, 84, 2.000000f, -40.000000f, 37.706669f), new FontGlyph(394, 126, 425, 165, 3.000000f, -39.000000f, 35.226669f), new FontGlyph(187, 1, 221, 48, 2.000000f, -40.000000f, 37.706669f), new FontGlyph(330, 126, 361, 165, 3.000000f, -39.000000f, 34.853333f), new FontGlyph(310, 44, 341, 85, 1.000000f, -40.000000f, 33.573334f), new FontGlyph(297, 126, 329, 165, 1.000000f, -39.000000f, 33.786667f), new FontGlyph(1, 59, 31, 99, 3.000000f, -39.000000f, 35.946667f), new FontGlyph(227, 87, 263, 126, 0.000000f, -39.000000f, 35.706669f), 
    new FontGlyph(297, 86, 344, 125, 0.000000f, -39.000000f, 47.760002f), new FontGlyph(179, 91, 214, 130, 0.000000f, -39.000000f, 34.693333f), new FontGlyph(88, 95, 122, 134, 0.000000f, -39.000000f, 33.760002f), new FontGlyph(426, 126, 457, 165, 1.000000f, -39.000000f, 33.093334f), new FontGlyph(48, 1, 60, 57, 3.000000f, -46.000000f, 15.173334f), new FontGlyph(229, 1, 254, 44, 0.000000f, -39.000000f, 23.040001f), new FontGlyph(35, 1, 47, 57, 0.000000f, -46.000000f, 15.173334f), new FontGlyph(103, 175, 125, 195, 1.000000f, -39.000000f, 23.866667f), 
    new FontGlyph(369, 197, 394, 204, 0.000000f, 0.000000f, 24.373333f), new FontGlyph(266, 197, 281, 206, 1.000000f, -41.000000f, 18.053333f), new FontGlyph(314, 166, 341, 197, 1.000000f, -30.000000f, 29.280001f), new FontGlyph(334, 1, 361, 43, 2.000000f, -41.000000f, 30.746668f), new FontGlyph(286, 166, 313, 197, 1.000000f, -30.000000f, 28.480001f), new FontGlyph(390, 1, 417, 43, 1.000000f, -41.000000f, 30.773335f), new FontGlyph(31, 140, 59, 171, 1.000000f, -30.000000f, 29.520000f), new FontGlyph(418, 1, 438, 43, 0.000000f, -42.000000f, 19.573334f), 
    new FontGlyph(277, 1, 305, 43, 1.000000f, -30.000000f, 31.173334f), new FontGlyph(61, 55, 87, 96, 2.000000f, -41.000000f, 30.560001f), new FontGlyph(258, 45, 268, 86, 2.000000f, -41.000000f, 14.480000f), new FontGlyph(97, 1, 112, 54, -3.000000f, -41.000000f, 14.186667f), new FontGlyph(371, 44, 399, 85, 2.000000f, -41.000000f, 29.173334f), new FontGlyph(429, 44, 438, 85, 3.000000f, -41.000000f, 14.480000f), new FontGlyph(369, 166, 412, 196, 2.000000f, -30.000000f, 47.280003f), new FontGlyph(442, 166, 468, 196, 2.000000f, -30.000000f, 30.586668f), 
    new FontGlyph(1, 140, 30, 171, 1.000000f, -30.000000f, 30.880001f), new FontGlyph(362, 1, 389, 43, 2.000000f, -30.000000f, 30.746668f), new FontGlyph(306, 1, 333, 43, 1.000000f, -30.000000f, 30.853334f), new FontGlyph(215, 167, 233, 197, 2.000000f, -30.000000f, 19.920000f), new FontGlyph(342, 166, 368, 197, 1.000000f, -30.000000f, 28.080000f), new FontGlyph(123, 135, 141, 172, 0.000000f, -36.000000f, 18.453333f), new FontGlyph(469, 166, 495, 196, 2.000000f, -29.000000f, 30.560001f), new FontGlyph(142, 171, 170, 200, 0.000000f, -29.000000f, 27.600000f), 
    new FontGlyph(234, 167, 274, 196, 0.000000f, -29.000000f, 40.133335f), new FontGlyph(171, 171, 199, 200, 0.000000f, -29.000000f, 27.786667f), new FontGlyph(342, 44, 370, 85, 0.000000f, -29.000000f, 27.413334f), new FontGlyph(1, 172, 26, 201, 1.000000f, -29.000000f, 27.786667f), new FontGlyph(61, 1, 78, 54, 1.000000f, -43.000000f, 18.026667f), new FontGlyph(222, 1, 228, 48, 4.000000f, -39.000000f, 13.813334f), new FontGlyph(79, 1, 96, 54, 0.000000f, -43.000000f, 18.026667f), new FontGlyph(234, 197, 265, 210, 2.000000f, -23.000000f, 35.413334f), 
  };

  const float TEXT_FONT_ASCENT = 25.333334f;
  const float TEXT_FONT_DESCENT = -6.666667f;
  const float TEXT_FONT_LINE_GAP = 0.000000f;
  static readonly KerningPair[] textFontKerning = {
    new KerningPair(32,84,-0.533333f), new KerningPair(34,34,-1.426667f), new KerningPair(34,39,-1.426667f), new KerningPair(34,65,-1.600000f), new KerningPair(34,97,-0.666667f), new KerningPair(34,99,-0.786667f), new KerningPair(34,100,-0.786667f), new KerningPair(34,101,-0.786667f), new KerningPair(34,103,-0.786667f), new KerningPair(34,109,-0.266667f), new KerningPair(34,110,-0.266667f), new KerningPair(34,111,-0.813333f), new KerningPair(34,112,-0.266667f), new KerningPair(34,113,-0.786667f), new KerningPair(34,115,-1.066667f), 
    new KerningPair(34,119,0.146667f), new KerningPair(39,34,-1.426667f), new KerningPair(39,39,-1.426667f), new KerningPair(39,65,-1.600000f), new KerningPair(39,97,-0.666667f), new KerningPair(39,99,-0.786667f), new KerningPair(39,100,-0.786667f), new KerningPair(39,101,-0.786667f), new KerningPair(39,103,-0.786667f), new KerningPair(39,109,-0.266667f), new KerningPair(39,110,-0.266667f), new KerningPair(39,111,-0.813333f), new KerningPair(39,112,-0.266667f), new KerningPair(39,113,-0.786667f), new KerningPair(39,115,-1.066667f), new KerningPair(39,119,0.146667f), 
    new KerningPair(40,86,0.266667f), new KerningPair(40,87,0.240000f), new KerningPair(40,89,0.293333f), new KerningPair(44,34,-2.266667f), new KerningPair(44,39,-2.266667f), new KerningPair(46,34,-2.266667f), new KerningPair(46,39,-2.266667f), new KerningPair(47,47,-2.986667f), new KerningPair(65,34,-1.600000f), new KerningPair(65,39,-1.600000f), new KerningPair(65,63,-0.813333f), new KerningPair(65,67,-0.146667f), new KerningPair(65,71,-0.146667f), new KerningPair(65,79,-0.146667f), new KerningPair(65,81,-0.146667f), new KerningPair(65,84,-1.720000f), 
    new KerningPair(65,85,-0.226667f), new KerningPair(65,86,-1.160000f), new KerningPair(65,87,-0.920000f), new KerningPair(65,89,-1.253333f), new KerningPair(65,111,-0.160000f), new KerningPair(65,116,-0.226667f), new KerningPair(65,117,-0.146667f), new KerningPair(65,118,-0.666667f), new KerningPair(65,119,-0.440000f), new KerningPair(65,121,-0.666667f), new KerningPair(65,122,0.160000f), new KerningPair(66,84,-0.360000f), new KerningPair(66,86,-0.320000f), new KerningPair(66,89,-0.733333f), new KerningPair(67,41,-0.346667f), new KerningPair(67,84,-0.386667f), 
    new KerningPair(67,93,-0.160000f), new KerningPair(67,125,-0.226667f), new KerningPair(68,44,-1.360000f), new KerningPair(68,46,-1.360000f), new KerningPair(68,65,-0.280000f), new KerningPair(68,84,-0.360000f), new KerningPair(68,86,-0.293333f), new KerningPair(68,88,-0.293333f), new KerningPair(68,89,-0.573333f), new KerningPair(68,90,-0.306667f), new KerningPair(69,84,0.266667f), new KerningPair(69,99,-0.253333f), new KerningPair(69,100,-0.253333f), new KerningPair(69,101,-0.253333f), new KerningPair(69,102,-0.240000f), new KerningPair(69,103,-0.253333f), 
    new KerningPair(69,111,-0.253333f), new KerningPair(69,113,-0.253333f), new KerningPair(69,117,-0.226667f), new KerningPair(69,118,-0.346667f), new KerningPair(69,119,-0.293333f), new KerningPair(69,121,-0.346667f), new KerningPair(70,44,-3.120000f), new KerningPair(70,46,-3.120000f), new KerningPair(70,65,-2.266667f), new KerningPair(70,74,-3.520000f), new KerningPair(70,84,0.266667f), new KerningPair(70,97,-0.453333f), new KerningPair(70,99,-0.280000f), new KerningPair(70,100,-0.280000f), new KerningPair(70,101,-0.280000f), new KerningPair(70,103,-0.280000f), 
    new KerningPair(70,111,-0.280000f), new KerningPair(70,113,-0.280000f), new KerningPair(70,114,-0.346667f), new KerningPair(70,117,-0.293333f), new KerningPair(70,118,-0.320000f), new KerningPair(70,121,-0.320000f), new KerningPair(72,65,0.240000f), new KerningPair(72,84,-0.386667f), new KerningPair(72,88,0.226667f), new KerningPair(72,89,-0.373333f), new KerningPair(73,65,0.240000f), new KerningPair(73,84,-0.386667f), new KerningPair(73,88,0.226667f), new KerningPair(73,89,-0.373333f), new KerningPair(74,65,-0.293333f), new KerningPair(75,45,-0.853333f), 
    new KerningPair(75,67,-0.413333f), new KerningPair(75,71,-0.413333f), new KerningPair(75,79,-0.413333f), new KerningPair(75,81,-0.413333f), new KerningPair(75,99,-0.346667f), new KerningPair(75,100,-0.346667f), new KerningPair(75,101,-0.346667f), new KerningPair(75,103,-0.346667f), new KerningPair(75,109,-0.306667f), new KerningPair(75,110,-0.306667f), new KerningPair(75,111,-0.360000f), new KerningPair(75,112,-0.306667f), new KerningPair(75,113,-0.346667f), new KerningPair(75,117,-0.306667f), new KerningPair(75,118,-0.533333f), new KerningPair(75,119,-0.840000f), 
    new KerningPair(75,121,-0.533333f), new KerningPair(76,34,-4.480000f), new KerningPair(76,39,-4.480000f), new KerningPair(76,65,0.253333f), new KerningPair(76,67,-0.866667f), new KerningPair(76,71,-0.866667f), new KerningPair(76,79,-0.866667f), new KerningPair(76,81,-0.866667f), new KerningPair(76,84,-3.666667f), new KerningPair(76,85,-0.720000f), new KerningPair(76,86,-2.333333f), new KerningPair(76,87,-1.906667f), new KerningPair(76,89,-3.186667f), new KerningPair(76,117,-0.586667f), new KerningPair(76,118,-1.773333f), new KerningPair(76,119,-1.226667f), 
    new KerningPair(76,121,-1.773333f), new KerningPair(77,65,0.240000f), new KerningPair(77,84,-0.386667f), new KerningPair(77,88,0.226667f), new KerningPair(77,89,-0.373333f), new KerningPair(78,65,0.240000f), new KerningPair(78,84,-0.386667f), new KerningPair(78,88,0.226667f), new KerningPair(78,89,-0.373333f), new KerningPair(79,44,-1.360000f), new KerningPair(79,46,-1.360000f), new KerningPair(79,65,-0.280000f), new KerningPair(79,84,-0.360000f), new KerningPair(79,86,-0.293333f), new KerningPair(79,88,-0.293333f), new KerningPair(79,89,-0.573333f), 
    new KerningPair(79,90,-0.306667f), new KerningPair(80,44,-4.320000f), new KerningPair(80,46,-4.320000f), new KerningPair(80,65,-1.840000f), new KerningPair(80,74,-2.666667f), new KerningPair(80,88,-0.413333f), new KerningPair(80,90,-0.346667f), new KerningPair(80,97,-0.146667f), new KerningPair(80,99,-0.173333f), new KerningPair(80,100,-0.173333f), new KerningPair(80,101,-0.173333f), new KerningPair(80,103,-0.173333f), new KerningPair(80,111,-0.173333f), new KerningPair(80,113,-0.173333f), new KerningPair(80,116,0.186667f), new KerningPair(80,118,0.200000f), 
    new KerningPair(80,121,0.200000f), new KerningPair(81,84,-0.573333f), new KerningPair(81,86,-0.373333f), new KerningPair(81,87,-0.266667f), new KerningPair(81,89,-0.466667f), new KerningPair(82,84,-1.066667f), new KerningPair(82,86,-0.253333f), new KerningPair(82,89,-0.640000f), new KerningPair(84,32,-0.533333f), new KerningPair(84,44,-2.906667f), new KerningPair(84,45,-3.093333f), new KerningPair(84,46,-2.906667f), new KerningPair(84,65,-1.053333f), new KerningPair(84,67,-0.373333f), new KerningPair(84,71,-0.373333f), new KerningPair(84,74,-3.200000f), 
    new KerningPair(84,79,-0.373333f), new KerningPair(84,81,-0.373333f), new KerningPair(84,83,-0.213333f), new KerningPair(84,84,0.213333f), new KerningPair(84,86,0.213333f), new KerningPair(84,87,0.200000f), new KerningPair(84,89,0.213333f), new KerningPair(84,97,-1.506667f), new KerningPair(84,99,-1.320000f), new KerningPair(84,100,-1.320000f), new KerningPair(84,101,-1.320000f), new KerningPair(84,103,-1.320000f), new KerningPair(84,109,-1.453333f), new KerningPair(84,110,-1.453333f), new KerningPair(84,111,-1.320000f), new KerningPair(84,112,-1.453333f), 
    new KerningPair(84,113,-1.320000f), new KerningPair(84,114,-1.000000f), new KerningPair(84,115,-1.546667f), new KerningPair(84,117,-1.266667f), new KerningPair(84,118,-0.960000f), new KerningPair(84,119,-0.760000f), new KerningPair(84,120,-1.026667f), new KerningPair(84,121,-0.960000f), new KerningPair(84,122,-0.800000f), new KerningPair(85,65,-0.293333f), new KerningPair(86,41,0.266667f), new KerningPair(86,44,-3.000000f), new KerningPair(86,45,-0.493333f), new KerningPair(86,46,-3.000000f), new KerningPair(86,65,-1.000000f), new KerningPair(86,67,-0.173333f), 
    new KerningPair(86,71,-0.173333f), new KerningPair(86,79,-0.173333f), new KerningPair(86,81,-0.173333f), new KerningPair(86,93,0.226667f), new KerningPair(86,97,-0.613333f), new KerningPair(86,99,-0.586667f), new KerningPair(86,100,-0.586667f), new KerningPair(86,101,-0.586667f), new KerningPair(86,103,-0.586667f), new KerningPair(86,111,-0.613333f), new KerningPair(86,113,-0.586667f), new KerningPair(86,114,-0.400000f), new KerningPair(86,117,-0.373333f), new KerningPair(86,118,-0.146667f), new KerningPair(86,121,-0.146667f), new KerningPair(86,125,0.253333f), 
    new KerningPair(87,41,0.200000f), new KerningPair(87,44,-1.640000f), new KerningPair(87,45,-0.800000f), new KerningPair(87,46,-1.640000f), new KerningPair(87,65,-0.573333f), new KerningPair(87,84,0.186667f), new KerningPair(87,93,0.160000f), new KerningPair(87,97,-0.440000f), new KerningPair(87,99,-0.413333f), new KerningPair(87,100,-0.413333f), new KerningPair(87,101,-0.413333f), new KerningPair(87,103,-0.413333f), new KerningPair(87,111,-0.413333f), new KerningPair(87,113,-0.413333f), new KerningPair(87,114,-0.280000f), new KerningPair(87,117,-0.253333f), 
    new KerningPair(87,125,0.186667f), new KerningPair(88,45,-0.613333f), new KerningPair(88,67,-0.333333f), new KerningPair(88,71,-0.333333f), new KerningPair(88,79,-0.333333f), new KerningPair(88,81,-0.333333f), new KerningPair(88,86,0.186667f), new KerningPair(88,99,-0.346667f), new KerningPair(88,100,-0.346667f), new KerningPair(88,101,-0.346667f), new KerningPair(88,103,-0.346667f), new KerningPair(88,111,-0.280000f), new KerningPair(88,113,-0.346667f), new KerningPair(88,117,-0.280000f), new KerningPair(88,118,-0.413333f), new KerningPair(88,121,-0.413333f), 
    new KerningPair(89,38,-0.400000f), new KerningPair(89,41,0.266667f), new KerningPair(89,42,-0.653333f), new KerningPair(89,44,-2.813334f), new KerningPair(89,45,-0.693333f), new KerningPair(89,46,-2.813334f), new KerningPair(89,65,-1.253333f), new KerningPair(89,67,-0.386667f), new KerningPair(89,71,-0.386667f), new KerningPair(89,74,-1.280000f), new KerningPair(89,79,-0.386667f), new KerningPair(89,81,-0.386667f), new KerningPair(89,83,-0.213333f), new KerningPair(89,84,0.226667f), new KerningPair(89,85,-1.280000f), new KerningPair(89,86,0.240000f), 
    new KerningPair(89,87,0.226667f), new KerningPair(89,88,0.173333f), new KerningPair(89,89,0.240000f), new KerningPair(89,93,0.240000f), new KerningPair(89,97,-0.973333f), new KerningPair(89,99,-0.866667f), new KerningPair(89,100,-0.866667f), new KerningPair(89,101,-0.866667f), new KerningPair(89,102,-0.293333f), new KerningPair(89,103,-0.866667f), new KerningPair(89,109,-0.533333f), new KerningPair(89,110,-0.533333f), new KerningPair(89,111,-0.866667f), new KerningPair(89,112,-0.533333f), new KerningPair(89,113,-0.866667f), new KerningPair(89,114,-0.533333f), 
    new KerningPair(89,115,-0.773333f), new KerningPair(89,116,-0.293333f), new KerningPair(89,117,-0.520000f), new KerningPair(89,118,-0.266667f), new KerningPair(89,120,-0.306667f), new KerningPair(89,121,-0.266667f), new KerningPair(89,122,-0.400000f), new KerningPair(89,125,0.253333f), new KerningPair(90,65,0.173333f), new KerningPair(90,67,-0.346667f), new KerningPair(90,71,-0.346667f), new KerningPair(90,79,-0.346667f), new KerningPair(90,81,-0.346667f), new KerningPair(90,99,-0.280000f), new KerningPair(90,100,-0.280000f), new KerningPair(90,101,-0.280000f), 
    new KerningPair(90,103,-0.280000f), new KerningPair(90,111,-0.280000f), new KerningPair(90,113,-0.280000f), new KerningPair(90,117,-0.253333f), new KerningPair(90,118,-0.360000f), new KerningPair(90,119,-0.360000f), new KerningPair(90,121,-0.360000f), new KerningPair(91,74,-0.240000f), new KerningPair(91,85,-0.240000f), new KerningPair(97,34,-0.893333f), new KerningPair(97,39,-0.893333f), new KerningPair(97,118,-0.200000f), new KerningPair(97,121,-0.200000f), new KerningPair(98,34,-0.386667f), new KerningPair(98,39,-0.386667f), new KerningPair(98,118,-0.146667f), 
    new KerningPair(98,120,-0.200000f), new KerningPair(98,121,-0.146667f), new KerningPair(98,122,-0.200000f), new KerningPair(99,34,-0.146667f), new KerningPair(99,39,-0.146667f), new KerningPair(101,34,-0.186667f), new KerningPair(101,39,-0.186667f), new KerningPair(101,118,-0.173333f), new KerningPair(101,121,-0.173333f), new KerningPair(102,34,0.213333f), new KerningPair(102,39,0.213333f), new KerningPair(102,41,0.266667f), new KerningPair(102,93,0.240000f), new KerningPair(102,99,-0.320000f), new KerningPair(102,100,-0.320000f), new KerningPair(102,101,-0.320000f), 
    new KerningPair(102,103,-0.320000f), new KerningPair(102,113,-0.320000f), new KerningPair(102,125,0.253333f), new KerningPair(104,34,-1.386667f), new KerningPair(104,39,-1.386667f), new KerningPair(107,99,-0.266667f), new KerningPair(107,100,-0.266667f), new KerningPair(107,101,-0.266667f), new KerningPair(107,103,-0.266667f), new KerningPair(107,113,-0.266667f), new KerningPair(109,34,-1.386667f), new KerningPair(109,39,-1.386667f), new KerningPair(110,34,-1.386667f), new KerningPair(110,39,-1.386667f), new KerningPair(111,34,-1.813333f), new KerningPair(111,39,-1.813333f), 
    new KerningPair(111,118,-0.200000f), new KerningPair(111,120,-0.280000f), new KerningPair(111,121,-0.200000f), new KerningPair(111,122,-0.213333f), new KerningPair(112,34,-0.386667f), new KerningPair(112,39,-0.386667f), new KerningPair(112,118,-0.146667f), new KerningPair(112,120,-0.200000f), new KerningPair(112,121,-0.146667f), new KerningPair(112,122,-0.200000f), new KerningPair(114,34,0.213333f), new KerningPair(114,39,0.213333f), new KerningPair(114,44,-1.640000f), new KerningPair(114,46,-1.640000f), new KerningPair(114,97,-0.533333f), new KerningPair(114,99,-0.253333f), 
    new KerningPair(114,100,-0.253333f), new KerningPair(114,101,-0.253333f), new KerningPair(114,102,0.200000f), new KerningPair(114,103,-0.253333f), new KerningPair(114,111,-0.266667f), new KerningPair(114,113,-0.253333f), new KerningPair(114,116,0.666667f), new KerningPair(114,118,0.240000f), new KerningPair(114,119,0.226667f), new KerningPair(114,121,0.240000f), new KerningPair(116,111,-0.266667f), new KerningPair(118,34,0.200000f), new KerningPair(118,39,0.200000f), new KerningPair(118,44,-1.426667f), new KerningPair(118,46,-1.426667f), new KerningPair(118,97,-0.200000f), 
    new KerningPair(118,99,-0.173333f), new KerningPair(118,100,-0.173333f), new KerningPair(118,101,-0.173333f), new KerningPair(118,102,0.173333f), new KerningPair(118,103,-0.173333f), new KerningPair(118,111,-0.200000f), new KerningPair(118,113,-0.173333f), new KerningPair(119,44,-1.653333f), new KerningPair(119,46,-1.653333f), new KerningPair(120,99,-0.266667f), new KerningPair(120,100,-0.266667f), new KerningPair(120,101,-0.266667f), new KerningPair(120,103,-0.266667f), new KerningPair(120,111,-0.266667f), new KerningPair(120,113,-0.266667f), new KerningPair(121,34,0.200000f), 
    new KerningPair(121,39,0.200000f), new KerningPair(121,44,-1.426667f), new KerningPair(121,46,-1.426667f), new KerningPair(121,97,-0.200000f), new KerningPair(121,99,-0.173333f), new KerningPair(121,100,-0.173333f), new KerningPair(121,101,-0.173333f), new KerningPair(121,102,0.173333f), new KerningPair(121,103,-0.173333f), new KerningPair(121,111,-0.200000f), new KerningPair(121,113,-0.173333f), new KerningPair(122,99,-0.213333f), new KerningPair(122,100,-0.213333f), new KerningPair(122,101,-0.213333f), new KerningPair(122,103,-0.213333f), new KerningPair(122,111,-0.213333f), 
    new KerningPair(122,113,-0.213333f), new KerningPair(123,74,-0.266667f), new KerningPair(123,85,-0.266667f), 
  };
  static readonly FontGlyph[] textFontGlyphs = {
    new FontGlyph(118, 26, 118, 26, 0.000000f, 0.000000f, 6.760000f), new FontGlyph(500, 1, 504, 22, 2.000000f, -20.000000f, 7.026667f), new FontGlyph(357, 44, 364, 52, 1.000000f, -21.000000f, 8.733334f), new FontGlyph(361, 23, 377, 43, 1.000000f, -20.000000f, 16.813334f), new FontGlyph(77, 1, 90, 27, 1.000000f, -23.000000f, 15.333334f), new FontGlyph(173, 1, 191, 22, 1.000000f, -20.000000f, 20.000000f), new FontGlyph(244, 1, 260, 22, 1.000000f, -20.000000f, 16.973333f), 
    new FontGlyph(15, 31, 18, 38, 1.000000f, -21.000000f, 4.760000f), new FontGlyph(10, 1, 18, 30, 1.000000f, -22.000000f, 9.333334f), new FontGlyph(1, 1, 9, 30, 0.000000f, -22.000000f, 9.493334f), new FontGlyph(319, 44, 331, 56, 0.000000f, -20.000000f, 11.760000f), new FontGlyph(33, 29, 47, 45, 1.000000f, -17.000000f, 15.480000f), new FontGlyph(92, 28, 97, 35, 0.000000f, -3.000000f, 5.360000f), new FontGlyph(404, 44, 411, 47, 0.000000f, -10.000000f, 7.533333f), new FontGlyph(92, 36, 96, 40, 1.000000f, -3.000000f, 7.186667f), 
    new FontGlyph(149, 1, 160, 23, 0.000000f, -20.000000f, 11.253334f), new FontGlyph(463, 1, 476, 22, 1.000000f, -20.000000f, 15.333334f), new FontGlyph(503, 23, 511, 43, 2.000000f, -20.000000f, 15.333334f), new FontGlyph(488, 23, 502, 43, 1.000000f, -20.000000f, 15.333334f), new FontGlyph(323, 1, 336, 22, 1.000000f, -20.000000f, 15.333334f), new FontGlyph(442, 23, 457, 43, 0.000000f, -20.000000f, 15.333334f), new FontGlyph(365, 1, 378, 22, 2.000000f, -20.000000f, 15.333334f), new FontGlyph(293, 1, 307, 22, 1.000000f, -20.000000f, 15.333334f), 
    new FontGlyph(458, 23, 472, 43, 1.000000f, -20.000000f, 15.333334f), new FontGlyph(393, 1, 406, 22, 1.000000f, -20.000000f, 15.333334f), new FontGlyph(449, 1, 462, 22, 1.000000f, -20.000000f, 15.333334f), new FontGlyph(113, 26, 117, 42, 1.000000f, -15.000000f, 6.613334f), new FontGlyph(107, 26, 112, 45, 0.000000f, -15.000000f, 5.773334f), new FontGlyph(306, 44, 318, 57, 0.000000f, -15.000000f, 13.880000f), new FontGlyph(344, 44, 356, 52, 2.000000f, -13.000000f, 14.986667f), new FontGlyph(292, 44, 305, 57, 1.000000f, -15.000000f, 14.266667f), 
    new FontGlyph(477, 1, 488, 22, 1.000000f, -20.000000f, 12.893333f), new FontGlyph(33, 1, 56, 28, 1.000000f, -20.000000f, 24.520000f), new FontGlyph(238, 23, 256, 43, 0.000000f, -20.000000f, 17.813334f), new FontGlyph(473, 23, 487, 43, 2.000000f, -20.000000f, 17.000000f), new FontGlyph(210, 1, 226, 22, 1.000000f, -20.000000f, 17.773335f), new FontGlyph(378, 23, 393, 43, 2.000000f, -20.000000f, 17.906668f), new FontGlyph(120, 24, 133, 44, 2.000000f, -20.000000f, 15.520000f), new FontGlyph(134, 24, 147, 44, 2.000000f, -20.000000f, 15.093333f), 
    new FontGlyph(227, 1, 243, 22, 1.000000f, -20.000000f, 18.600000f), new FontGlyph(327, 23, 343, 43, 2.000000f, -20.000000f, 19.466667f), new FontGlyph(167, 24, 170, 44, 2.000000f, -20.000000f, 7.426667f), new FontGlyph(421, 1, 434, 22, 0.000000f, -20.000000f, 15.066667f), new FontGlyph(310, 23, 326, 43, 2.000000f, -20.000000f, 17.120001f), new FontGlyph(148, 24, 161, 44, 2.000000f, -20.000000f, 14.693334f), new FontGlyph(198, 23, 218, 43, 2.000000f, -20.000000f, 23.840000f), new FontGlyph(293, 23, 309, 43, 2.000000f, -20.000000f, 19.466667f), 
    new FontGlyph(192, 1, 209, 22, 1.000000f, -20.000000f, 18.773335f), new FontGlyph(394, 23, 409, 43, 2.000000f, -20.000000f, 17.226667f), new FontGlyph(98, 1, 115, 25, 1.000000f, -20.000000f, 18.773335f), new FontGlyph(426, 23, 441, 43, 2.000000f, -20.000000f, 16.813334f), new FontGlyph(261, 1, 276, 22, 1.000000f, -20.000000f, 16.200001f), new FontGlyph(344, 23, 360, 43, 0.000000f, -20.000000f, 16.293333f), new FontGlyph(277, 1, 292, 22, 1.000000f, -20.000000f, 17.706667f), new FontGlyph(219, 23, 237, 43, 0.000000f, -20.000000f, 17.373333f), 
    new FontGlyph(173, 23, 197, 43, 0.000000f, -20.000000f, 24.226667f), new FontGlyph(275, 23, 292, 43, 0.000000f, -20.000000f, 17.120001f), new FontGlyph(257, 23, 274, 43, 0.000000f, -20.000000f, 16.400000f), new FontGlyph(410, 23, 425, 43, 1.000000f, -20.000000f, 16.346666f), new FontGlyph(19, 1, 25, 29, 1.000000f, -23.000000f, 7.240000f), new FontGlyph(161, 1, 172, 23, 0.000000f, -20.000000f, 11.200000f), new FontGlyph(26, 1, 32, 29, 0.000000f, -23.000000f, 7.240000f), new FontGlyph(332, 44, 343, 55, 0.000000f, -20.000000f, 11.413334f), 
    new FontGlyph(390, 44, 403, 47, 0.000000f, 0.000000f, 12.320001f), new FontGlyph(382, 44, 389, 49, 0.000000f, -21.000000f, 8.440001f), new FontGlyph(1, 31, 14, 47, 1.000000f, -15.000000f, 14.853333f), new FontGlyph(120, 1, 134, 23, 1.000000f, -21.000000f, 15.320001f), new FontGlyph(19, 30, 32, 46, 1.000000f, -15.000000f, 14.293334f), new FontGlyph(135, 1, 148, 23, 1.000000f, -21.000000f, 15.400001f), new FontGlyph(48, 29, 61, 45, 1.000000f, -15.000000f, 14.466667f), new FontGlyph(489, 1, 499, 22, 0.000000f, -21.000000f, 9.480000f), 
    new FontGlyph(379, 1, 392, 22, 1.000000f, -15.000000f, 15.320001f), new FontGlyph(351, 1, 364, 22, 1.000000f, -21.000000f, 15.040000f), new FontGlyph(162, 24, 166, 44, 1.000000f, -20.000000f, 6.626667f), new FontGlyph(91, 1, 97, 27, -1.000000f, -20.000000f, 6.520000f), new FontGlyph(337, 1, 350, 22, 1.000000f, -21.000000f, 13.840000f), new FontGlyph(505, 1, 508, 22, 2.000000f, -21.000000f, 6.626667f), new FontGlyph(184, 44, 206, 59, 1.000000f, -15.000000f, 23.933334f), new FontGlyph(242, 44, 255, 59, 1.000000f, -15.000000f, 15.066667f), 
    new FontGlyph(77, 28, 91, 44, 1.000000f, -15.000000f, 15.573334f), new FontGlyph(308, 1, 322, 22, 1.000000f, -15.000000f, 15.320001f), new FontGlyph(435, 1, 448, 22, 1.000000f, -15.000000f, 15.520000f), new FontGlyph(283, 44, 291, 59, 1.000000f, -15.000000f, 9.240000f), new FontGlyph(171, 44, 183, 60, 1.000000f, -15.000000f, 14.080000f), new FontGlyph(98, 26, 106, 45, 0.000000f, -18.000000f, 8.920000f), new FontGlyph(62, 29, 75, 45, 1.000000f, -15.000000f, 15.053333f), new FontGlyph(228, 44, 241, 59, 0.000000f, -15.000000f, 13.226667f), 
    new FontGlyph(207, 44, 227, 59, 0.000000f, -15.000000f, 20.520000f), new FontGlyph(256, 44, 269, 59, 0.000000f, -15.000000f, 13.533334f), new FontGlyph(407, 1, 420, 22, 0.000000f, -15.000000f, 12.920000f), new FontGlyph(270, 44, 282, 59, 1.000000f, -15.000000f, 13.533334f), new FontGlyph(67, 1, 76, 28, 0.000000f, -22.000000f, 9.240000f), new FontGlyph(116, 1, 119, 25, 2.000000f, -20.000000f, 6.653334f), new FontGlyph(57, 1, 66, 28, 0.000000f, -22.000000f, 9.240000f), new FontGlyph(365, 44, 381, 50, 1.000000f, -11.000000f, 18.573334f), 
  };

  //
  //
  //
  
  static readonly string[] itemMessage_Soap = {
    "You eat soap. Delicious!",
    "You feel slippery.",
  };
  bool soap = false;

  //
  //
  //

  static readonly string[] introMessage = {
    "Twice a year, composers of the 8BMT gather to take part in a sacred ritual: the Secret Santa.",
    "In this holy tradition, observed since the ancient year of 2021, their best soundmancers",
    "focus their creative energy toward a common goal: making the holidays enjoyable for everyone.",
    "Every winter, they dedicate their music making to the legendary Brass Otter.",
    "Every summer, they make offerings to the great Lamp of Funk.",
    "In return, these two gods help preserve peace and creativity throughout the land.",
    "However, in the winter of the year 2023, one of their soundmancers could not attend the ceremony.",
    "The cycle of harmony was in peril.",
    "Fortunately, some of their peers set out to save the day and make things right.",
    "They did so the only way they knew how: by making music together.",
    "They called themselves the Super Hyper Secret Santa Rescue Team (or SHSSRT, for short),",
    "and this is what they came up with.",
  };
  bool inIntroCutscene = false;
  float introFadeoutTimer = 2.0f;
  float introFadeinTimer = 1.0f;

  //
  //
  //

  bool inVolumeConfig = true;

  //
  //
  //

  static readonly string[] outroMessage = {
    "Congratulations! You've beaten every level!",
    "Don't worry if you've SKIPped any of them, you will be able to replay them freely.",
    "(Don't skip through this dialogue if you want to hear this arrangement in full, though!)",
    "But first, let's take a moment to thank the SHSSRT crew:",
    "Keeba (me!): Programming, game design, art, arranging,",
    "aero: Arranging, Secret Santa secrets, organising,",
    "time: Arranging, recruiting, HELP messages, playtesting (\"I give the best advice.\"),",
    "Lampie: Arranging, recruiting, art, playtesting, moral support,",
    "UNKL: Arranging (and being the first to submit a finished arrangement),",
    "Toby: Arranging (\"this is why i didn't enter ostinato week\"),",
    "Moebius: Arranging,",
    "Mel: Arranging,",
    "Swi: Arranging,",
    "Some of my close friends (G, P: you know who you are): Playtesting,",
    "The entire 8BMT community: Inspiration and memes.",
    "Y'all did awesome, thank you for participating in this!",
    "As promised, from now on, you can use X and C to select any level at any time.",
    "Have fun!",
  };
  bool inOutroCutscene = false;
  float outroFadeinTimer = 1.0f;
  float outroFadeoutTimer = 2.0f;

  //
  //
  //

  enum InputKind {
    Menu,
    Action,
    Item,
    Text,
  }

  Config config = new Config();

  KeyboardState lastKeyboard = new KeyboardState();

  //
  //
  //

  struct Arranger {
    public string name;
    public string displayName;
    public Texture2D texture;
    public SoundEffect music;
    public SoundEffectInstance musicInstance;
    public float jumpTimer;
    public float yOffset;
    public float dy;
    public Vector2 displayP;
    public uint miniGameStartIndex;
    public uint miniGameOnePastLastIndex;

    public Arranger(string name, string displayName, uint miniGameStartIndex, uint miniGameOnePastLastIndex) {
      this.name = name;
      this.displayName = displayName;
      this.texture = null;
      this.music = null;
      this.musicInstance = null;
      this.jumpTimer = 0.0f;
      this.yOffset = 0.0f;
      this.dy = 0.0f;
      this.displayP = Vector2.Zero;
      this.miniGameStartIndex = miniGameStartIndex;
      this.miniGameOnePastLastIndex = miniGameOnePastLastIndex;
    }
  }

  const int ARRANGER_KEEBA = 0;
  const int ARRANGER_AERO = 1;
  const int ARRANGER_UNKL = 2;
  const int ARRANGER_TIME = 3;
  const int ARRANGER_SWI = 4;
  const int ARRANGER_MOEBIUS = 5;
  const int ARRANGER_TOBY = 6;
  const int ARRANGER_COUNT = 7;

  static Arranger[] arrangers = {
    new Arranger("keeba", "Keeba", MINIGAME_KEEBA_TUTORIAL1, MINIGAME_AERO_MARCH1),
    new Arranger("aero", "aero", MINIGAME_AERO_MARCH1, MINIGAME_UNKL_STRUM1),
    new Arranger("unkl", "UNKL_WYFU", MINIGAME_UNKL_STRUM1, MINIGAME_TIME_YOKU1),
    new Arranger("time", "timelovesahero", MINIGAME_TIME_YOKU1, MINIGAME_SWI_NOTHING1),
    new Arranger("swi", "Swi", MINIGAME_SWI_NOTHING1, MINIGAME_MOEBIUS_KNOB1),
    new Arranger("moebius", "Moebius", MINIGAME_MOEBIUS_KNOB1, MINIGAME_TOBY1),
    new Arranger("toby", "Toby", MINIGAME_TOBY1, MINIGAME_COUNT),
  };

  Texture2D textCursorTexture;
  Texture2D textFontTexture;
  Texture2D commandFontTexture;
  Texture2D circleTexture;
  Texture2D blankTexture;
  Texture2D knobTexture;
  Texture2D moebius2_1Texture;
  Texture2D moebius2_2Texture;
  Texture2D moebius3_1Texture;
  Texture2D moebius3_2Texture;
  Texture2D moebius3_3Texture;
  Texture2D moebius3_4Texture;
  Texture2D boatTexture;
  Texture2D spotlightTexture;
  Texture2D guitarTexture;
  Texture2D flagTexture;
  Texture2D fourTexture;
  SpriteBatch batch;
  SoundEffect shootSfx;
  SoundEffect guitarSfx;
  SoundEffect introMusic;
  SoundEffectInstance introMusicInstance;
  SoundEffect outroMusic;
  SoundEffectInstance outroMusicInstance;
  SoundEffect optionsMusic;
  SoundEffectInstance optionsMusicInstance;

  //
  //
  //

  const float CROSSFADE_CREDITS_TIME = 5.0f;

  int currentMiniGameIndex = -1;
  int crossFadePrevArrangerIndex = -1;
  float crossFadeLerpFactor = 1;
  float crossFadeCreditsTimer = CROSSFADE_CREDITS_TIME;
  float crossFadeAlpha = 1.0f;
  float miniGameChangeTimer = CROSSFADE_CREDITS_TIME;
  float miniGameChangeAlpha = 1.0f;
  
  static bool AllBitsSet(uint x, uint start, uint end) {
    uint mask = ((uint)1 << (byte)end) - ((uint)1 << (byte)start);
    bool result = (x & mask) == mask;
    return result;
  }

  //
  //
  //

  const float SECONDS_PER_CHARACTER_FAST   = 0.018f;
  const float SECONDS_PER_CHARACTER_MEDIUM = 0.08f;

  Font commandFont;
  Font textFont;

  static readonly string[] helpText_Jesse = {
    "You ask Jesse what to do.",
    "(Jesse is typing...)",
    "\"write music eat soap\"",
  };

  static readonly string[] helpText_time = {
    "You ask time what to do.",
    "(time is typing...)",
    "\"Shoot the trumpet.\"",
    "You feel like you've learned something about revolvers.",
  };

  static readonly string[] helpText_StoredJumps = {
    "You ask Keeba for help.",
    "(Keeba is typing...)",
    "\"Remember that you can jump if you walk off a platform!\"",
    "\"This is needed to complete some levels!\"",
  };

  static readonly string[] helpText_Skip = {
    "You ask Keeba for help.",
    "(Keeba is typing...)",
    "\"How about you just SKIP instead? ;)\"",
    "\"I won't judge~\"",
  };

  static readonly string[] helpText_DerHealsPiano = {
    "You ask DerHealsmann for help.",
    "(Some time passes...)",
    "\"I've done a few takes of comping and soloing, here ya go!\"",
    "The piano is immaculate.",
    "You don't feel particularly helped, but you feel inspired.",
  };

  static readonly string[] helpText_MTaur = {
    "You ask MTaur for help.",
    "(MTaur is typing...)",
    "\"I only have $exposure, does that help?\"",
    "You also receive a bunch of cow and dollar bill emojis.",
  };

  static readonly string[] helpText_trme = {
    "You ask the 8BMT Discord what to do.",
    "(UNKL_WYFU is typing...)",
    "\"trme\"",
    "(8tacko is typing...)",
    "\"trme\"",
    "(jesse is typing...)",
    "\"trme\"",
    "(metamoogle is typing...)",
    "\"trme\"",
    "(Cloud is typing...)",
    "\"trme\"",
    "(R is typing...)",
    "\"trme\"",
    "(aero is typing...)",
    "\"trme\"",
    "(hands is typing...)",
    "\"trme\"",
    "This goes on for a while.",
    "You're not sure what you've learned."
  };

  static readonly string[] helpText_Toby2 = {
    "You ask Keeba for help.",
    "(Keeba is typing...)",
    "\"Fun fact: your hitbox is smaller in Toby's second level!\"",
    "You are not sure how this helps you.",
  };

  static readonly string[] helpText_Four = {
    "You ask time for help.",
    "He starts giving you tips.",
    "Very quickly, the discussion is flooded with 4 emojis.",
    "time's advice is lost in the chaos...",
  };

  static readonly string[] helpText_Lampie = {
    "You ask Lampie for help.",
    "(Lampie is typing...)",
    "\"just do it 10.000 times and one will work\"",
    "\"its just like the weekly\"",
    "...",
    "\"actually, make that 4,444 times\"",
  };

  static readonly string[] helpText_Unkl = {
    "You ask UNKL_WYFU for help.",
    "(UNKL_WYFU is typing...)",
    "\"Zoinky the boinky\"",
    "You're not sure how that helped.",
    "You feel silly for asking UNKL...",
  };

  static readonly string[] helpText_Nerf = {
    "You ask Keeba for help.",
    "He shrugs. \"What do you want me to do?\"",
    "\"I've already nerfed Toby once!\"",
  };

  static readonly string[] helpText_Roast = {
    "You want to ask #roast-then-post for help...",
    "... but you don't have permission to do that.",
    "You should read #rules-and-directory carefully...",
  };

  static readonly string[] helpText_Moebius = {
    "You ask Moebius for help.",
    "(Moebius is typing...)",
    "\"Dont trust Time's advice.\"",
    "\"Only trust in number 4.\"",
    "You want to revolt, but are not sure that is going to help.",
  };

  const int HELP_TEXT_JESSE = 0;
  const int HELP_TEXT_TIME  = 1;
  const int HELP_TEXT_TRME  = 2;
  const int HELP_TEXT_STORED_JUMPS  = 3;
  const int HELP_TEXT_SKIP  = 4;
  const int HELP_TEXT_DERHEALS_PIANO  = 5;
  const int HELP_TEXT_MTAUR  = 6;
  const int HELP_TEXT_TOBY2  = 7;
  const int HELP_TEXT_FOUR = 8;
  const int HELP_TEXT_LAMPIE = 9;
  const int HELP_TEXT_UNKL = 10;
  const int HELP_TEXT_NERF = 11;
  const int HELP_TEXT_ROAST = 12;
  const int HELP_TEXT_MOEBIUS = 13;
  const int HELP_TEXT_COUNT = 14;

  static readonly string[][] helpText = {
    helpText_Jesse, helpText_time, helpText_trme, helpText_StoredJumps, helpText_Skip, helpText_DerHealsPiano, helpText_MTaur, helpText_Toby2, helpText_Four, helpText_Lampie, helpText_Unkl, helpText_Nerf, helpText_Roast, helpText_Moebius
  };

  ulong helpTextRead = 0;

  string[] textMessage;
  int textMessageCursor;

  struct TextBox {
    public string str;
    public float timer;
    public int charactersVisible;
    public bool preventFastScrolling;

    public TextBox(string str, float timer = 0, int charactersVisible = 0, bool preventFastScrolling = true) {
      this.str = str;
      this.timer = timer;
      this.charactersVisible = charactersVisible;
      this.preventFastScrolling = preventFastScrolling;
    }

    public void SetText(string str) {
      this.str = str;
      this.timer = 0;
      this.charactersVisible = 0;
      this.preventFastScrolling = true;
    }
  }

  TextBox textBox;
  bool setTextThisFrame = false;

  //
  //
  //

  const int MINIGAME_FLAG_GRAVITY        = (1 << 0);
  const int MINIGAME_FLAG_CANNOT_MOVE    = (1 << 1);
  const int MINIGAME_FLAG_WIN_ON_TIMEOUT = (1 << 2);

  const int MINIGAME_KEEBA_TUTORIAL1 = 0;
  const int MINIGAME_KEEBA_TUTORIAL2 = 1;
  const int MINIGAME_KEEBA_TUTORIAL3 = 2;
  const int MINIGAME_KEEBA_TUTORIAL4 = 3;
  const int MINIGAME_KEEBA_TUTORIAL5 = 4;
  const int MINIGAME_AERO_MARCH1 = 5;
  const int MINIGAME_AERO_MARCH2 = 6;
  const int MINIGAME_AERO_MARCH3 = 7;
  const int MINIGAME_UNKL_STRUM1 = 8;
  const int MINIGAME_UNKL_STRUM2 = 9;
  const int MINIGAME_UNKL_STRUM3 = 10;
  const int MINIGAME_TIME_YOKU1 = 11;
  const int MINIGAME_TIME_YOKU2 = 12;
  const int MINIGAME_TIME_YOKU3 = 13;
  const int MINIGAME_TIME_YOKU4 = 14;
  const int MINIGAME_SWI_NOTHING1 = 15;
  const int MINIGAME_SWI_NOTHING2 = 16;
  const int MINIGAME_MOEBIUS_KNOB1 = 17;
  const int MINIGAME_MOEBIUS_KNOB2 = 18;
  const int MINIGAME_MOEBIUS_TRUMPET1 = 19;
  const int MINIGAME_TOBY1 = 20;
  const int MINIGAME_TOBY2 = 21;
  const int MINIGAME_COUNT = 22;

  static readonly Vector2 flagHalfSize = new Vector2(TILE_SIZE, TILE_SIZE);
  struct Flag {
    public byte tileX;
    public byte tileY;

    public Flag(byte tileX, byte tileY) {
      this.tileX = tileX;
      this.tileY = tileY;
    }
  }

  static readonly Vector2 guitarHalfSize = new Vector2(TILE_SIZE * 2, TILE_SIZE);
  struct Guitar {
    public byte tileX;
    public byte tileY;

    public Guitar(byte tileX, byte tileY) {
      this.tileX = tileX;
      this.tileY = tileY;
    }
  }

  struct MiniGame {
    public int arranger;
    public string hint;
    public string postMessage;
    public Texture2D background;
    public int gameplayFlags;
    public float timeOut;
    public int maxJumps;
    public Vector2 startPos;
    public Enemy[] enemies;
    public Yoku[] yoku;
    public Guitar[] guitars;
    public Flag[] flags;
    public byte[] tiles;

    public MiniGame(int arranger, string hint, string postMessage, int gameplayFlags, float timeOut, int maxJumps, Vector2 startPos, Enemy[] enemies, Yoku[] yoku, Guitar[] guitars, Flag[] flags, byte[] tiles) {
      this.arranger = arranger;
      this.hint = hint;
      this.postMessage = postMessage;
      this.gameplayFlags = gameplayFlags;
      this.timeOut = timeOut;
      this.maxJumps = maxJumps;
      this.startPos = startPos;
      this.enemies = enemies;
      this.yoku = yoku;
      this.guitars = guitars;
      this.flags = flags;
      this.tiles = tiles;

      this.background = null;
    }
  }

  static readonly Enemy[] noEnemies = {};
  static readonly Yoku[] noYoku = {};
  static readonly Guitar[] noGuitars = {};
  static readonly Flag[] noFlags = {};
  static readonly byte[] noTiles = {};
 
  const uint ALL_MINIGAMES_COMPLETED = ((uint)1 << MINIGAME_COUNT) - 1;
  uint miniGamesCompleted = 0;
  bool allMiniGamesCompleted = false;
  MiniGame[] miniGames = {
    new MiniGame(ARRANGER_KEEBA, "Use WASD or the arrow keys to move! Try reaching the flag!", "", 0, 60.0f, 0, new Vector2(0, 125), noEnemies, noYoku, noGuitars,
      new Flag[]{new Flag(11, 10)},
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        0,0,0,1,1,1,1,1,1,1,1,1,1,1,1,1,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
      }),
    new MiniGame(ARRANGER_KEEBA, "When there's gravity, use SPACE to jump! Try reaching the flag!", "", 0, 60.0f, 1, new Vector2(-miniGameInnerHalfSize.X + 8, 0), noEnemies, noYoku, noGuitars,
      new Flag[]{new Flag(16,11)},
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,1,1,
      }),
    new MiniGame(ARRANGER_KEEBA, "If you get stuck, press ESCAPE to give up!", "Press ESCAPE to give up!", 0, 10.0f, 1, new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8), noEnemies, noYoku, noGuitars,
      new Flag[]{new Flag(16,11)},
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
      }),
    new MiniGame(ARRANGER_KEEBA, "If you walk off a ledge, you can jump in midair!", "Midair jumps are cool!", 0, 10.0f, 1, new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8), noEnemies, noYoku, noGuitars,
      new Flag[]{new Flag(16,11)},
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
        1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,
      }),
    new MiniGame(ARRANGER_KEEBA, "Good job! In the menu, press X or C to change arranger!", "Press X or C to change arranger!", 0, 10.0f, 0, new Vector2(-115, 0), noEnemies, noYoku, noGuitars,
      new Flag[]{new Flag(11, 8)},
      noTiles),

    new MiniGame(ARRANGER_AERO, "March!", "", MINIGAME_FLAG_WIN_ON_TIMEOUT, 6.0f, 0, new Vector2(-miniGameInnerHalfSize.X + 8 + 50, 0),
      new Enemy[]{
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, 0), new Vector2(miniGameInnerHalfSize.X - 8, 0)}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 100, 0), new Vector2(miniGameInnerHalfSize.X - 8, 0)}),
      }, noYoku, noGuitars, noFlags,
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
      }),
    new MiniGame(ARRANGER_AERO, "March!", "", MINIGAME_FLAG_WIN_ON_TIMEOUT, 20.0f, 0, new Vector2(-miniGameInnerHalfSize.X + 8, 0),
      new Enemy[]{
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, -50), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, 50), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8)}),
      }, noYoku, noGuitars, noFlags,
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,
        0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
      }),
    new MiniGame(ARRANGER_AERO, "March!", "", MINIGAME_FLAG_WIN_ON_TIMEOUT, 13.0f, 0, new Vector2(-miniGameInnerHalfSize.X + 8 + 24, miniGameInnerHalfSize.Y - 8 - 16),
      new Enemy[]{
        // Row 1
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),

        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24),}),
        
        // Row 2
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*11, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*11, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*12, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*12, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*4, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*4, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*5, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*5, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*6, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*6, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*7, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*7, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),

        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),

        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*2), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*2),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*3), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*3),}),
        
        // Row 3
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),

        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*4),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*5),}),
                
        // Row 4
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*11, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*11, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*12, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*12, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*4, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*4, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*5, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*5, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*6, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*6, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*7, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*7, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),

        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),

        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*6), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*6),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*7), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*7),}),
        
        // Row 5
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),

        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*8), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*8),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*9), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*9),}),

        // Row 6
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*11, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*11, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*12, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*12, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*4, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*4, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*5, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*5, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*6, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*6, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*7, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 24*7, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),

        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 0.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 1.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 1.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 1.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),

        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 4.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 5.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 5.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 5.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),

        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 8.0f, 80.0f, 1.0f, 1.0f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 9.3f, 80.0f, 1.0f, 0.3f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 9.6f, 80.0f, 1.0f, 0.6f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*10), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*10),}),
        new Enemy(8.0f, 9.9f, 80.0f, 1.0f, 0.9f, 1.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 24*11), new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 24*11),}),
      }, noYoku, noGuitars, noFlags,
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
      }),

    new MiniGame(ARRANGER_UNKL, "Strum!", "", 0, 7.5f, 1, new Vector2(0, 100), noEnemies, noYoku,
      new Guitar[]{
        new Guitar(2, 14),
        new Guitar(13, 14),
      }, noFlags, noTiles),
    new MiniGame(ARRANGER_UNKL, "Strum!", "", 0, 12.5f, 1, new Vector2(0, 100), noEnemies, noYoku,
      new Guitar[]{
        new Guitar(1, 4),
        new Guitar(14, 4),
      }, noFlags,
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,1,1,1,0,0,0,1,1,1,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,
      }),
    new MiniGame(ARRANGER_UNKL, "Strum!", "", 0, 12.5f, 1, new Vector2(0, 100), noEnemies, noYoku,
      new Guitar[]{
        new Guitar(1, 4),
        new Guitar(14, 4),
      }, noFlags,
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,1,1,1,0,0,0,1,1,1,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,
      }),

    new MiniGame(ARRANGER_TIME, "Reach the flag!", "", 0, 5.0f, 1, new Vector2(-100, 130), noEnemies,
      new Yoku[]{
        new Yoku(2.0f, 3.5f, 9, 17),
        new Yoku(3.0f, 4.5f, 11, 14),
      }, noGuitars,
      new Flag[]{
        new Flag(7, 10),
      }, noTiles),
    new MiniGame(ARRANGER_TIME, "Reach the flag!", "", 0, 7.5f, 1, new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8), noEnemies,
      new Yoku[]{
        new Yoku(1.5f, 4.0f, 5, 5),
        new Yoku(1.5f, 4.0f, 6, 5),
        new Yoku(2.5f, 5.0f, 11, 5),
        new Yoku(2.5f, 5.0f, 12, 5),
      }, noGuitars,
      new Flag[]{
        new Flag(17, 1),
      },
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
      }),
    new MiniGame(ARRANGER_TIME, "Reach the flag!", "", 0, 7.5f, 1, new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8), noEnemies,
      new Yoku[]{
        new Yoku(1.5f, 3.0f, 5, 5),
        new Yoku(1.5f, 3.0f, 6, 5),
        new Yoku(3.5f, 5.0f, 11, 5),
        new Yoku(3.5f, 5.0f, 12, 5),
      }, noGuitars,
      new Flag[]{
        new Flag(17, 1),
      },
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
      }),
    new MiniGame(ARRANGER_TIME, "Reach the flag!", "", 0, 7.5f, 1, new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8), noEnemies,
      new Yoku[]{
        new Yoku(1.5f, 3.0f, 5, 5),
        new Yoku(1.5f, 3.0f, 6, 5),
      }, noGuitars,
      new Flag[]{
        new Flag(17, 1),
      },
      new byte[]{
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
        1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,
      }),

    new MiniGame(ARRANGER_SWI, "Don't move!", "It's not about the notes you play, it's about those you don't.", MINIGAME_FLAG_CANNOT_MOVE|MINIGAME_FLAG_WIN_ON_TIMEOUT, 3.0f, 0, new Vector2(0, 115),
      new Enemy[]{
        new Enemy(8.0f, 2.2f, 200.0f, 0.0f, 0.0f, 0, new Vector2[]{Vector2.Zero, new Vector2(0, 90.0f)})
      }, noYoku, noGuitars, noFlags, noTiles),
    new MiniGame(ARRANGER_SWI, "Don't move!", "You've been playing for a while. Don't forget to take a break!", MINIGAME_FLAG_CANNOT_MOVE|MINIGAME_FLAG_WIN_ON_TIMEOUT, 60.0f, 0, new Vector2(50, 100), noEnemies, noYoku, noGuitars, noFlags, noTiles),

    new MiniGame(ARRANGER_MOEBIUS, "Turn it up!", "", 0, 8.0f, 0, new Vector2(0, 100), noEnemies, noYoku, noGuitars, noFlags, noTiles),
    new MiniGame(ARRANGER_MOEBIUS, "Turn it up!", "", 0, 8.0f, 0, new Vector2(0, 100), noEnemies, noYoku, noGuitars, noFlags, noTiles),
    new MiniGame(ARRANGER_MOEBIUS, "Shoot the trumpet!", "time would be proud.", 0, 12.0f, 0, new Vector2(0, 115), noEnemies, noYoku, noGuitars, noFlags, noTiles),

    new MiniGame(ARRANGER_TOBY, "???", "???", MINIGAME_FLAG_WIN_ON_TIMEOUT, 9.5f, 0, new Vector2(0, 115),
      new Enemy[]{
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8)}),
        
        new Enemy(8.0f, 0.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8)}),
        

        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8)}),
        
        new Enemy(8.0f, 1.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 1.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8)}),
        

        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8)}),
      
        new Enemy(8.0f, 2.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8)}),
        

        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8)}),
      
        new Enemy(8.0f, 3.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8)}),


        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8)}),
      
        new Enemy(8.0f, 4.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 4.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8)}),


        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*6, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*8, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*10, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.0f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*12, miniGameInnerHalfSize.Y - 8)}),
      
        new Enemy(8.0f, 5.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*7, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*9, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 5.5f, 80.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 24*11, miniGameInnerHalfSize.Y - 8)}),
      }, noYoku, noGuitars, noFlags, noTiles),

    new MiniGame(ARRANGER_TOBY, "???", "???", MINIGAME_FLAG_WIN_ON_TIMEOUT, 7.5f, 0, new Vector2(0, 115),
      new Enemy[]{
        new Enemy(8.0f, 0.0f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.1f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.2f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.3f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.4f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.5f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 0.6f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*6, miniGameInnerHalfSize.Y - 8)}),
        
        new Enemy(8.0f, 0.6f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*0), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*0)}),
        new Enemy(8.0f, 0.7f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*1), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*1)}),
        new Enemy(8.0f, 0.8f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*2), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*2)}),
        new Enemy(8.0f, 0.9f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*3), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*3)}),
        new Enemy(8.0f, 1.0f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*4), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*4)}),
        new Enemy(8.0f, 1.1f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*5), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*5)}),
        new Enemy(8.0f, 1.2f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*6), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*6)}),
        
        new Enemy(8.0f, 1.3f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*0, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*0, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 1.4f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*1, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*1, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 1.5f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*2, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*2, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 1.6f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*3, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*3, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 1.7f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*4, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*4, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 1.8f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*5, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*5, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 1.9f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*6, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*6, -miniGameInnerHalfSize.Y + 8)}),
        
        new Enemy(8.0f, 2.0f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*0), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*0)}),
        new Enemy(8.0f, 2.1f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*1), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*1)}),
        new Enemy(8.0f, 2.2f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*2), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*2)}),
        new Enemy(8.0f, 2.3f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*3), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*3)}),
        new Enemy(8.0f, 2.4f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*4)}),
        new Enemy(8.0f, 2.5f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*5)}),
        new Enemy(8.0f, 2.6f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*6), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*6)}),

        new Enemy(8.0f, 2.7f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*0, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*0, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.8f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*1, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*1, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 2.9f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*2, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*2, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.0f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*3, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*3, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.1f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*4, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*4, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.2f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*5, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*5, miniGameInnerHalfSize.Y - 8)}),
        new Enemy(8.0f, 3.3f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8 + 44*6, -miniGameInnerHalfSize.Y + 8), new Vector2(-miniGameInnerHalfSize.X + 8 + 44*6, miniGameInnerHalfSize.Y - 8)}),
        
        new Enemy(8.0f, 3.4f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*0), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*0)}),
        new Enemy(8.0f, 3.5f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*1), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*1)}),
        new Enemy(8.0f, 3.6f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*2), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*2)}),
        new Enemy(8.0f, 3.7f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*3), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*3)}),
        new Enemy(8.0f, 3.8f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*4), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*4)}),
        new Enemy(8.0f, 3.9f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*5), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*5)}),
        new Enemy(8.0f, 4.0f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8, -miniGameInnerHalfSize.Y + 8 + 44*6), new Vector2(-miniGameInnerHalfSize.X + 8, -miniGameInnerHalfSize.Y + 8 + 44*6)}),
        
        new Enemy(8.0f, 4.1f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*0, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*0, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 4.2f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*1, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*1, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 4.3f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*2, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*2, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 4.4f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*3, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*3, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 4.5f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*4, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*4, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 4.6f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*5, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*5, -miniGameInnerHalfSize.Y + 8)}),
        new Enemy(8.0f, 4.7f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(miniGameInnerHalfSize.X - 8 - 44*6, miniGameInnerHalfSize.Y - 8), new Vector2(miniGameInnerHalfSize.X - 8 - 44*6, -miniGameInnerHalfSize.Y + 8)}),
        
        new Enemy(8.0f, 4.8f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*0), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*0)}),
        new Enemy(8.0f, 4.9f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*1), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*1)}),
        new Enemy(8.0f, 5.0f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*2), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*2)}),
        new Enemy(8.0f, 5.1f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*3), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*3)}),
        new Enemy(8.0f, 5.2f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*4), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*4)}),
        new Enemy(8.0f, 5.3f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*5), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*5)}),
        new Enemy(8.0f, 5.4f, 160.0f, 0.0f, 0.0f, 0.0f, new Vector2[]{new Vector2(-miniGameInnerHalfSize.X + 8, miniGameInnerHalfSize.Y - 8 - 44*6), new Vector2(miniGameInnerHalfSize.X - 8, miniGameInnerHalfSize.Y - 8 - 44*6)}),
      }, noYoku, noGuitars, noFlags, noTiles),
  };

  struct Enemy {
    public float radius;
    public float spawnTime;
    public float speed;
    public float pauseInterval;
    public float startingPauseInterval;
    public float pauseLength;
    public Vector2[] points;

    public Enemy(float radius, float spawnTime, float speed, float pauseInterval, float startingPauseInterval, float pauseLength, Vector2[] points) {
      this.radius = radius;
      this.spawnTime = spawnTime;
      this.speed = speed;
      this.points = points;
      this.pauseInterval = pauseInterval;
      this.startingPauseInterval = startingPauseInterval;
      this.pauseLength = pauseLength;
    }
  }

  struct Yoku {
    public float spawnTime;
    public float despawnTime;
    public byte x;
    public byte y;

    public Yoku(float spawnTime, float despawnTime, byte x, byte y) {
      this.spawnTime = spawnTime;
      this.despawnTime = despawnTime;
      this.x = x;
      this.y = y;
    }
  }

  Random rng;

  static Color screenColor = Color.Black;
  static Vector2 playerHalfSize = new Vector2(8, 8);
  const int MINIGAME_RECT_THICKNESS = 5;
  static Vector2 miniGameCenter = new Vector2(1280/2, 720*3/8);
  static Vector2 miniGameHalfSize = new Vector2(152 + MINIGAME_RECT_THICKNESS, 152 + MINIGAME_RECT_THICKNESS);
  static Vector2 miniGameInnerHalfSize = new Vector2(152, 152);
  static Vector2 miniGameHintP  = new Vector2(miniGameCenter.X, miniGameCenter.Y - miniGameHalfSize.Y - 8);

  struct LiveEnemy {
    public Vector2 p;
    public float pauseIntervalTimer;
    public float pauseTimer;
    public byte pointIndex;
    public int enemyIndex;
  }

  const int MAX_ENEMIES = 128;
  static ulong[] enemiesAlive = {0, 0};
  static LiveEnemy[] liveEnemies = new LiveEnemy[MAX_ENEMIES];

  const int MAX_GUITARS = 32;
  uint guitarsHit = 0;
  int guitarsLeft = 0;

  // Dynamic game state.
  const float playerRadius = 8.0f;
  const float playerRadius_Toby2 = 4.0f;
  Vector2 playerP;
  Vector2 lastPlayerMove;
  float playerDy;
  int playerJumps = 0;
  bool playerOnGround = false;
  
  float miniGameTimer = 0.0f;
  float miniGameIntroTimer = 0.0f;
  float miniGameOutroTimer = 0.0f;
  bool miniGameWon = false;
  int miniGameState = 0;

  // For MOEBIUS_KNOB
  const float KNOB_RADIUS = 15;
  const float MAX_KNOB_ANGLE = 2.05f;
  static readonly Vector2 knobP = new Vector2(-80, -43);
  Vector2 knobRot = new Vector2(1, 0);
  float knobAngle = 0.0f;
  bool ioButton = false;
  static readonly Vector2 ioButtonP = new Vector2(-91, -136);
  const float IO_BUTTON_RADIUS = 16.0f;


  // For MOEBIUS_TRUMPET
  Vector2[] revolverPs = {
    new Vector2(-53, 40),
    new Vector2(-50, 20),
    new Vector2(10, -25),
    new Vector2(30, -25),
    new Vector2(50, -25),
    new Vector2(70, -25),
  };

  Vector2 hammerP   = new Vector2(-50, -40);
  float hammerAngle = 0.0f;

  Vector2 triggerP   = new Vector2(-25, 10);
  float triggerAngle = 0.0f;
  
  const float handleRadius  = 8.0f;
  const float hammerRadius  = 8.0f;
  const float triggerRadius = 8.0f;
  const float barrelRadius  = 8.0f;

  float revolverAngle = 0.0f;
  Vector2 revolverRot = new Vector2(1, 0);

  static bool CircleIntersect(Vector2 aP, float aRadius, Vector2 bP, float bRadius) {
    float radius = aRadius+bRadius;
    float radiusSq = radius*radius;

    bool Result = (aP-bP).LengthSquared() <= radiusSq;

    return Result;
  }

  //
  //
  //

  int commandCursor; // 0bYX
  const int COMMAND_ATTACK = 0b00;
  const int COMMAND_ITEM   = 0b01;
  const int COMMAND_HELP   = 0b10;
  const int COMMAND_RUN    = 0b11;

  InputKind inputKind;

  //
  //
  //

  static Vector2 MoveToward(Vector2 a, Vector2 b, float amount) {
    Vector2 dp = b - a;
    float dpLength = dp.Length();

    Vector2 result = b;
    if(dpLength > amount) {
      result = a + NormalizeOrDefault(dp, Vector2.Zero) * amount;
    }

    return result;
  }

  static Vector2 ComplexProd(Vector2 a, Vector2 b) {
    Vector2 Result = new Vector2(a.X * b.X - a.Y * b.Y, a.X * b.Y + a.Y * b.X);
    return Result;
  }

  static float Outer(Vector2 a, Vector2 b) {
    float Result = a.X * b.Y - a.Y * b.X;
    return Result;
  }

  Vector2 TileToGame(byte x, byte y) {
    Vector2 result = -miniGameInnerHalfSize;
    result.X += (float)(x * TILE_SIZE);
    result.Y += (float)(y * TILE_SIZE);
    return result;
  }

  Vector2 GameToScreen(Vector2 gameP) {
    Vector2 result = gameP + miniGameCenter;
    return result;
  }

  Vector2 TileToScreen(byte x, byte y) {
    Vector2 result = miniGameCenter - miniGameInnerHalfSize;
    result.X += (float)(x * TILE_SIZE);
    result.Y += (float)(y * TILE_SIZE);
    return result;
  }

  void ChangeMiniGame(int deltaIndex) {
    if((inputKind == InputKind.Menu) && (crossFadeLerpFactor == 1.0f)) {
      int newIndex = currentMiniGameIndex + deltaIndex;

      if(newIndex < 0) {
        newIndex += MINIGAME_COUNT;
      }
      newIndex %= MINIGAME_COUNT;

      if(!allMiniGamesCompleted && ((miniGamesCompleted & ((uint)1 << (byte)newIndex)) != 0)) {
        ChangeArranger(deltaIndex);
      } else {
        ref MiniGame currentMiniGame = ref miniGames[currentMiniGameIndex];
        ref MiniGame newMiniGame     = ref miniGames[newIndex];
        if(newMiniGame.arranger != currentMiniGame.arranger) {
          crossFadePrevArrangerIndex = currentMiniGame.arranger;
          crossFadeLerpFactor = 0.0f;
          crossFadeCreditsTimer = 0.0f;
          crossFadeAlpha = 0.0f;
        }

        miniGameChangeTimer = 0.0f;

        currentMiniGameIndex = newIndex;
      }
    }
  }

  void ChangeArranger(int delta) {
    if((inputKind == InputKind.Menu) && (crossFadeLerpFactor == 1.0f)) {
      int currentArrangerIndex = miniGames[currentMiniGameIndex].arranger;
      int arrangerIndex = currentArrangerIndex;
      int newMiniGameIndex = currentMiniGameIndex;

      while(true) {
        arrangerIndex += delta;
        if(arrangerIndex < 0) {
          arrangerIndex += ARRANGER_COUNT;
        } else if(arrangerIndex >= ARRANGER_COUNT) {
          arrangerIndex -= ARRANGER_COUNT;
        }

        if(arrangerIndex == currentArrangerIndex) {
          break;
        }

        ref Arranger arranger = ref arrangers[arrangerIndex];
        if(!AllBitsSet(miniGamesCompleted, arranger.miniGameStartIndex, arranger.miniGameOnePastLastIndex)) {
          newMiniGameIndex = (int)arranger.miniGameStartIndex;
          while((miniGamesCompleted & ((uint)1 << newMiniGameIndex)) != 0) {
            newMiniGameIndex += 1;
          }

          break;
        }
      }

      if(arrangerIndex != currentArrangerIndex) {
        currentMiniGameIndex = newMiniGameIndex;
        crossFadePrevArrangerIndex = currentArrangerIndex;
        crossFadeLerpFactor = 0.0f;
        crossFadeCreditsTimer = 0.0f;
        crossFadeAlpha = 0.0f;
        miniGameChangeTimer = 0.0f;
      }
    }
  }

  private HaiGame() {
    GraphicsDeviceManager gdm = new GraphicsDeviceManager(this);
    gdm.PreferredBackBufferWidth = 1280;
    gdm.PreferredBackBufferHeight = 720;
    gdm.IsFullScreen = false;
    gdm.SynchronizeWithVerticalRetrace = true;

    this.IsMouseVisible = false;

    Content.RootDirectory = "Content";

    config.mainVolume = 0.1f;

    commandCursor = 0;
    inputKind = InputKind.Menu;
    textBox = new TextBox("Let's jam! Use SPACE to start playing!", 0, 0, false);
  }

  protected override void Initialize() {
    rng = new Random();

    playerP = new Vector2(100, 100);

    // @Debug
    // Debug.Assert(miniGames.Length == MINIGAME_COUNT);
    // Debug.Assert(arrangers.Length == ARRANGER_COUNT);

    base.Initialize();
  }

  protected override void LoadContent() {
    batch = new SpriteBatch(GraphicsDevice);
    blankTexture = Content.Load<Texture2D>("blank");
    circleTexture = Content.Load<Texture2D>("circle16");
    commandFontTexture = Content.Load<Texture2D>("commandFont");
    textFontTexture = Content.Load<Texture2D>("textFont");
    textCursorTexture = Content.Load<Texture2D>("textCursor");
    shootSfx = Content.Load<SoundEffect>("shoot");
    boatTexture = Content.Load<Texture2D>("boat");
    spotlightTexture = Content.Load<Texture2D>("spotlight");
    guitarTexture = Content.Load<Texture2D>("guitar");
    guitarSfx = Content.Load<SoundEffect>("guitar");
    flagTexture = Content.Load<Texture2D>("flag");
    fourTexture = Content.Load<Texture2D>("4");
    introMusic = Content.Load<SoundEffect>("mel");
    introMusicInstance = introMusic.CreateInstance();
    outroMusic = Content.Load<SoundEffect>("lampie");
    outroMusicInstance = outroMusic.CreateInstance();
    optionsMusic = Content.Load<SoundEffect>("options");
    optionsMusicInstance = optionsMusic.CreateInstance();

    for(int arrangerIndex = 0; arrangerIndex < ARRANGER_COUNT; ++arrangerIndex) {
      ref Arranger arranger = ref arrangers[arrangerIndex];

      arranger.music   = Content.Load<SoundEffect>(arranger.name);
      arranger.texture = Content.Load<Texture2D>(  arranger.name);

      // Premultiply alpha, because we don't have an easy way of baking PNGs to premultiplied...
      Color[] buffer = new Color[arranger.texture.Width * arranger.texture.Height];
      arranger.texture.GetData(buffer);
      for (int i = 0; i < buffer.Length; i++)
      {
          buffer[i] = Color.FromNonPremultiplied(buffer[i].R, buffer[i].G, buffer[i].B, buffer[i].A);
      }
      arranger.texture.SetData(buffer);

      arranger.displayP.X = arrangerIndex * (1280 / ARRANGER_COUNT);
      arranger.displayP.Y = 400;
    }

    miniGames[MINIGAME_SWI_NOTHING1].background = Content.Load<Texture2D>("swi1");
    miniGames[MINIGAME_SWI_NOTHING2].background = Content.Load<Texture2D>("swi2");
    miniGames[MINIGAME_MOEBIUS_KNOB1].background = Content.Load<Texture2D>("moebius1");
    miniGames[MINIGAME_MOEBIUS_TRUMPET1].background = Content.Load<Texture2D>("moebius3");

    knobTexture       = Content.Load<Texture2D>("knob");
    moebius2_1Texture = Content.Load<Texture2D>("moebius2_1");
    moebius2_2Texture = Content.Load<Texture2D>("moebius2_2");
    moebius3_1Texture = Content.Load<Texture2D>("moebius3_1");
    moebius3_2Texture = Content.Load<Texture2D>("moebius3_2");
    moebius3_3Texture = Content.Load<Texture2D>("moebius3_3");
    moebius3_4Texture = Content.Load<Texture2D>("moebius3_4");

    commandFont = new Font(commandFontTexture, commandFontKerning, commandFontGlyphs, COMMAND_FONT_ASCENT, COMMAND_FONT_DESCENT, COMMAND_FONT_LINE_GAP);
    textFont = new Font(textFontTexture, textFontKerning, textFontGlyphs, TEXT_FONT_ASCENT, TEXT_FONT_DESCENT, TEXT_FONT_LINE_GAP);


    // Start playing the options music right away.
    config.mainVolume = 0.0f;
    optionsMusicInstance.Volume = 0.0f;
    optionsMusicInstance.IsLooped = true;
    optionsMusicInstance.Play();



    base.LoadContent();
  }

  protected override void UnloadContent() {
    base.UnloadContent();
  }

  static Vector2 NormalizeOrDefault(Vector2 v, Vector2 defaultValue) {
    Vector2 result = defaultValue;

    if((v.X != 0.0f) || (v.Y != 0.0f)) {
      result = Vector2.Normalize(v);
    }

    return result;
  }

  static bool KeyPressed(KeyboardState last, KeyboardState current, Keys key) {
    return !last.IsKeyDown(key) && current.IsKeyDown(key);
  }

  static int Clamp(int x, int low, int high) {
    return Math.Min(high, Math.Max(low, x));
  }

  static float Clamp(float x, float low, float high) {
    return Math.Min(high, Math.Max(low, x));
  }

  struct CollideResult {
    public Vector2 correct;
    public float   penetration;

    public CollideResult(Vector2 correct, float penetration) {
      this.correct = correct;
      this.penetration = penetration;
    }
  }

  static CollideResult CollideBoxBox(Vector2 aP, Vector2 aHalfSize, Vector2 bP, Vector2 bHalfSize) {
    CollideResult result = new CollideResult(Vector2.Zero, 0.0f);

    Vector2 halfSize = aHalfSize + bHalfSize;
    Vector2 p = aP - bP;
    Vector2 closest = p;

    closest.X = Clamp(closest.X, -halfSize.X, halfSize.X);
    closest.Y = Clamp(closest.Y, -halfSize.Y, halfSize.Y);

    if(closest == p) {
      if(Math.Abs(closest.X) > Math.Abs(closest.Y)) {
        if(closest.X < 0.0f) {
          result.correct.X = -1.0f;
          result.penetration = closest.X + halfSize.X;
        } else {
          result.correct.X = 1.0f;
          result.penetration = halfSize.X - closest.X;
        }
      } else {
        if(closest.Y < 0.0f) {
          result.correct.Y = -1.0f;
          result.penetration = closest.Y + halfSize.Y;
        } else {
          result.correct.Y = 1.0f;
          result.penetration = halfSize.Y - closest.Y;
        }
      }
    }

    return result;
  }

  static CollideResult CollideCircleBox(Vector2 p, float radius, Vector2 boxP, Vector2 boxHalfSize) {
    CollideResult Result = new CollideResult(Vector2.Zero, 0.0f);

    Vector2 closest = p;
    Vector2 boxMin = boxP - boxHalfSize;
    Vector2 boxMax = boxP + boxHalfSize;

    closest.X = Clamp(closest.X, boxMin.X, boxMax.X);
    closest.Y = Clamp(closest.Y, boxMin.Y, boxMax.Y);

    Vector2 toCircle = p - closest;
    float toCircleLength = toCircle.Length();
    if(toCircleLength < radius) {
      Result.correct = toCircle / toCircleLength;
      Result.penetration = radius - toCircleLength;
    }

    return Result;
  }

  bool CollidePlayerTile(ref MiniGame miniGame, int tileX, int tileY) {
    bool result = false;

    if((tileX >= 0) && (tileX < TILE_MAP_SIZE) && (tileY >= 0) && (tileY < TILE_MAP_SIZE)) {
      Vector2 playerLocalP = playerP - miniGameCenter;
      
      if(miniGame.tiles[tileY * TILE_MAP_SIZE + tileX] == 1) {
        result = true;
        Vector2 p = TileToGame((byte)tileX, (byte)tileY);
        p += tileHalfSize;

        CollideResult collide = CollideBoxBox(playerLocalP, new Vector2(playerRadius), p, tileHalfSize);
        if(collide.penetration > 0.0f) {
          playerP += collide.correct * (collide.penetration + 1.0f);

          if(collide.correct.Y != 0.0f) {
            playerDy = 0.0f;
          }

          if(collide.correct.Y < 0.0f) {
            playerJumps = 0;
            playerOnGround = true;
          }
        }
      }
    }

    return result;
  }

  void StartTextMessage(string[] message) {
    textMessage = message;
    textMessageCursor = -1;
    textBox.SetText("     ");
    inputKind = InputKind.Text;
  }

  void CompleteCurrentLevel() {
    uint bit = (uint)1 << (byte)currentMiniGameIndex;
    if((miniGamesCompleted & bit) == 0) {
      miniGamesCompleted |= bit;

      if(miniGamesCompleted == ALL_MINIGAMES_COMPLETED) {
        allMiniGamesCompleted = true;

        inOutroCutscene = true;
        outroFadeinTimer = 0.0f;
        StartTextMessage(outroMessage);
        outroMusicInstance.Volume = 0.0f;
        outroMusicInstance.IsLooped = true;
        outroMusicInstance.Play();
      }
    }
  }

  protected override void Update(GameTime gameTime) {
    float frameDt = (float)gameTime.ElapsedGameTime.TotalSeconds;
    Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) * 0.5f;
    setTextThisFrame = false;
    
    KeyboardState keyboard = Keyboard.GetState();

    if(inVolumeConfig) {
      float move = 0.0f;
      if(keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left)) {
        move -= 1.0f;
      }
      if(keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) {
        move += 1.0f;
      }

      const float moveSpeed = 0.6f;
      config.mainVolume += move * moveSpeed * frameDt;
      config.mainVolume = Clamp(config.mainVolume, 0.0f, 1.0f);

      optionsMusicInstance.Volume = config.mainVolume*config.mainVolume;

      if(KeyPressed(lastKeyboard, keyboard, Keys.Space)) {
        // We set the main volume here and never touch it again.
        config.mainVolume = config.mainVolume * config.mainVolume;
        inVolumeConfig = false;
        introFadeinTimer = 0.0f;


        // Initialize more stuff.
        for(int arrangerIndex = 0; arrangerIndex < ARRANGER_COUNT; ++arrangerIndex) {
          ref Arranger arranger = ref arrangers[arrangerIndex];

          arranger.musicInstance = arranger.music.CreateInstance();
          arranger.musicInstance.IsLooped = true;
          arranger.musicInstance.Volume = 0.0f;
        }

        currentMiniGameIndex = 0;

        // Start the intro cutscene.
        inIntroCutscene = true;
        StartTextMessage(introMessage);
        introMusicInstance.IsLooped = true;
        introMusicInstance.Volume = 1.0f * config.mainVolume;
        introMusicInstance.Play();
      }
    } else {
      if(crossFadeLerpFactor < 1.0f) {
        // Fade over 0.25s.
        crossFadeLerpFactor += frameDt * 4.0f;
        crossFadeLerpFactor = Math.Min(crossFadeLerpFactor, 1.0f);

        float fromVolumePerceptual = 1.0f - crossFadeLerpFactor;
        float toVolumePerceptual = crossFadeLerpFactor;
        float fromVolume = fromVolumePerceptual * fromVolumePerceptual * config.mainVolume;
        float toVolume   = toVolumePerceptual   * toVolumePerceptual   * config.mainVolume;

        int crossFadeCurrentArrangerIndex = miniGames[currentMiniGameIndex].arranger;
        // If both indices are the same, we simply fade the target music in.
        // (This is used when exiting the outro cutscene.)
        if(crossFadePrevArrangerIndex != crossFadeCurrentArrangerIndex) {
          arrangers[crossFadePrevArrangerIndex].musicInstance.Volume = fromVolume;
        }
        arrangers[crossFadeCurrentArrangerIndex].musicInstance.Volume = toVolume;
      }
      if(crossFadeCreditsTimer < CROSSFADE_CREDITS_TIME) {
        crossFadeCreditsTimer += frameDt;

        if(crossFadeCreditsTimer < 0.5f) {
          // Fade in.
          crossFadeAlpha = crossFadeCreditsTimer / 0.5f;
        } else if(crossFadeCreditsTimer < 4.5f) {
          // Stay up.
          crossFadeAlpha = 1.0f;
        } else {
          // Fade out.
          crossFadeAlpha = 1.0f - ((crossFadeCreditsTimer - 4.5f) / 0.5f);
        }
      }
      if(miniGameChangeTimer < CROSSFADE_CREDITS_TIME) {
        miniGameChangeTimer += frameDt;

        if(miniGameChangeTimer < 0.5f) {
          // Fade in.
          miniGameChangeAlpha = miniGameChangeTimer / 0.5f;
        } else if(miniGameChangeTimer < 4.5f) {
          // Stay up.
          miniGameChangeAlpha = 1.0f;
        } else {
          // Fade out.
          miniGameChangeAlpha = 1.0f - ((miniGameChangeTimer - 4.5f) / 0.5f);
        }
      }
      if(introFadeinTimer < 1.0f) {
        introFadeinTimer += frameDt * 0.5f;
        introFadeinTimer = Math.Min(1.0f, introFadeinTimer);

        float volumeLinear = introFadeinTimer;
        float volumePerceptual = volumeLinear*volumeLinear;

        optionsMusicInstance.Volume = config.mainVolume * (1.0f - volumePerceptual);
        introMusicInstance.Volume = config.mainVolume * volumePerceptual;
      }
      if(introFadeoutTimer < 1.0f) {
        introFadeoutTimer += frameDt * 0.5f;
        introFadeoutTimer = Math.Min(1.0f, introFadeoutTimer);

        float volumeLinear = 1.0f - introFadeoutTimer;
        float volumePerceptual = volumeLinear*volumeLinear;

        introMusicInstance.Volume = config.mainVolume * volumePerceptual;
      }
      if(outroFadeinTimer < 1.0f) {
        outroFadeinTimer += frameDt;
        outroFadeinTimer = Math.Min(1.0f, outroFadeinTimer);

        float volumePerceptual = outroFadeinTimer*outroFadeinTimer;

        arrangers[miniGames[currentMiniGameIndex].arranger].musicInstance.Volume = config.mainVolume * (1.0f - volumePerceptual);
        outroMusicInstance.Volume = config.mainVolume * volumePerceptual;
      }
      if(outroFadeoutTimer < 1.0f) {
        outroFadeoutTimer += frameDt * 0.5f;
        outroFadeoutTimer = Math.Min(1.0f, outroFadeoutTimer);

        float volumeLinear = 1.0f - outroFadeoutTimer;
        float volumePerceptual = volumeLinear*volumeLinear;

        outroMusicInstance.Volume = config.mainVolume * volumePerceptual;
      }


      if((miniGamesCompleted == (((uint)1 << (byte)MINIGAME_COUNT) - 1)) || keyboard.IsKeyDown(Keys.LeftControl)) {
        if(KeyPressed(lastKeyboard, keyboard, Keys.X)) {
          ChangeMiniGame(-1);
        }
        if(KeyPressed(lastKeyboard, keyboard, Keys.C)) {
          ChangeMiniGame(1);
        }
      } else {
        if(KeyPressed(lastKeyboard, keyboard, Keys.X)) {
          ChangeArranger(-1);
        }
        if(KeyPressed(lastKeyboard, keyboard, Keys.C)) {
          ChangeArranger(1);
        }
      }

      ref MiniGame miniGame = ref miniGames[currentMiniGameIndex];

      if(inputKind == InputKind.Menu) {
        if(KeyPressed(lastKeyboard, keyboard, Keys.A) || KeyPressed(lastKeyboard, keyboard, Keys.Q) || KeyPressed(lastKeyboard, keyboard, Keys.Left)) {
          commandCursor &= ~1;
        }
        if(KeyPressed(lastKeyboard, keyboard, Keys.D) || KeyPressed(lastKeyboard, keyboard, Keys.Right)) {
          commandCursor |= 1;
        }
        if(KeyPressed(lastKeyboard, keyboard, Keys.W) || KeyPressed(lastKeyboard, keyboard, Keys.Z) || KeyPressed(lastKeyboard, keyboard, Keys.Up)) {
          commandCursor &= ~2;
        }
        if(KeyPressed(lastKeyboard, keyboard, Keys.S) || KeyPressed(lastKeyboard, keyboard, Keys.Down)) {
          commandCursor |= 2;
        }

        if(KeyPressed(lastKeyboard, keyboard, Keys.Space) || KeyPressed(lastKeyboard, keyboard, Keys.Enter)) {
          // Confirm selection.
          switch(commandCursor) {
            case COMMAND_ATTACK: {
              inputKind = InputKind.Action;

              lastPlayerMove = Vector2.Zero;
              miniGameIntroTimer  = 0.0f;
              miniGameTimer       = 0.0f;
              miniGameOutroTimer  = -1.0f;
              miniGameWon         = false;
              enemiesAlive[0]     = 0;
              enemiesAlive[1]     = 0;
              guitarsHit          = 0;
              guitarsLeft         = miniGame.guitars.Length;
              playerP             = miniGameCenter + miniGame.startPos;
              playerDy            = 0;
              miniGameState       = 0;
              playerJumps         = 0;
              playerOnGround      = false;
              hammerAngle         = 0.0f;
              triggerAngle        = 0.0f;
              revolverRot         = new Vector2(1, 0);
              revolverAngle       = 0.0f;
              knobAngle           = 0.0f;
              ioButton            = false;
              knobRot             = new Vector2(1, 0);
            } break;

            case COMMAND_ITEM: {
              inputKind = InputKind.Item;
            } break;

            case COMMAND_HELP: {
              int index = rng.Next(HELP_TEXT_COUNT);

              for(int i = 0; i < HELP_TEXT_COUNT; ++i) {
                ulong bit = (ulong)1 << (byte)index;
                if((helpTextRead & bit) != 0) {
                  index = (index + 1) % HELP_TEXT_COUNT;
                } else {
                  helpTextRead |= bit;
                  break;
                }
              }

              StartTextMessage(helpText[index]);
            } break;

            case COMMAND_RUN: {
              CompleteCurrentLevel();
              ChangeMiniGame(1);
            } break;
          }
        }
      } else if(inputKind == InputKind.Action) {
        if(miniGameIntroTimer < 1.0f) {
          miniGameIntroTimer += frameDt * 2.0f;
          miniGameIntroTimer = Math.Min(1.0f, miniGameIntroTimer);
        } else if(miniGameOutroTimer < 0.0f) {
          // Spawn enemies.
          for(int enemyIndex = 0; enemyIndex < miniGame.enemies.Length; ++enemyIndex) {
            ref Enemy enemy = ref miniGame.enemies[enemyIndex];

            float deltaSpawnTime = miniGameTimer - enemy.spawnTime;
            if((deltaSpawnTime >= 0.0f) && (deltaSpawnTime < frameDt)) {
              // Find a spawn index.
              for(int spawnIndex = 0; spawnIndex < MAX_ENEMIES; ++spawnIndex) {
                if((enemiesAlive[spawnIndex/64] & ((ulong)1 << (byte)(spawnIndex % 64))) == 0) {
                  enemiesAlive[spawnIndex/64] |= (ulong)1 << (byte)(spawnIndex % 64);

                  ref LiveEnemy live = ref liveEnemies[spawnIndex];
                  live.p = enemy.points[0];
                  live.pauseIntervalTimer = enemy.startingPauseInterval;
                  live.enemyIndex = enemyIndex;
                  live.pauseTimer = 0.0f;
                  live.pointIndex = 0;

                  break;
                }
              }
            }
          }

          bool win = false;
          bool lose = false;

          // Update the player.
          float playerSpeed = 220.0f;
          Vector2 playerMove = Vector2.Zero;
          if(soap) {
            playerMove = lastPlayerMove;

            // Here, we use the crappy bad feeling key handling just so we don't interfere with the last
            // move vector and end up accidentally zeroing it out.
            if(keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Q) || keyboard.IsKeyDown(Keys.Left)) {
              playerMove.X = -1.0f;
            }
            if(keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) {
              playerMove.X =  1.0f;
            }
            if(keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Z) || keyboard.IsKeyDown(Keys.Up)) {
              playerMove.Y = -1.0f;
            }
            if(keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) {
              playerMove.Y =  1.0f;
            }
          } else {
            if(keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Q) || keyboard.IsKeyDown(Keys.Left)) {
              playerMove.X -= 1.0f;
            }
            if(keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right)) {
              playerMove.X += 1.0f;
            }
            if(keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Z) || keyboard.IsKeyDown(Keys.Up)) {
              playerMove.Y -= 1.0f;
            }
            if(keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down)) {
              playerMove.Y += 1.0f;
            }
          }

          if(miniGame.maxJumps > 0) {
            playerMove.Y = 0.0f;

            if((playerJumps < miniGame.maxJumps) && KeyPressed(lastKeyboard, keyboard, Keys.Space)) {
              playerDy = -300.0f;
              playerJumps += 1;
            }
            
            if(!playerOnGround) {
              const float Gravity = 800.0f;
              playerDy += Gravity * frameDt;
            }

            playerP.Y += playerDy * frameDt;
          }

          if(!soap) {
            playerMove = NormalizeOrDefault(playerMove, Vector2.Zero);
          }

          lastPlayerMove = playerMove;

          if((miniGame.gameplayFlags & MINIGAME_FLAG_CANNOT_MOVE) != 0) {
            if((playerMove.X != 0.0f) || (playerMove.Y != 0.0f)) {
              lose = true;
            }
          }

          playerP += playerMove * playerSpeed * frameDt;
          playerP.X = Math.Max(miniGameCenter.X - miniGameInnerHalfSize.X + playerHalfSize.X, Math.Min(miniGameCenter.X + miniGameInnerHalfSize.X - playerHalfSize.X, playerP.X));
          {
            playerOnGround = false;

            float floorY = miniGameCenter.Y + miniGameInnerHalfSize.Y - playerRadius;
            if(playerP.Y > floorY) {
              playerP.Y = floorY;
              playerJumps = 0;
              playerOnGround = true;
              playerDy = 0.0f;
            }
            float ceilingY = miniGameCenter.Y - miniGameInnerHalfSize.Y + playerRadius;
            if(playerP.Y < ceilingY) {
              playerP.Y = ceilingY;
              playerDy = 0.0f;
            }

            Vector2 playerLocalP = playerP - miniGameCenter;

            if(miniGame.tiles.Length > 0) {
              Vector2 playerTileP = (playerLocalP + miniGameInnerHalfSize) / (tileHalfSize * 2.0f);

              int playerTileX = (int)playerTileP.X;
              int playerTileY = (int)playerTileP.Y;

              bool nSolid = CollidePlayerTile(ref miniGame, playerTileX, playerTileY-1);
              bool sSolid = CollidePlayerTile(ref miniGame, playerTileX, playerTileY+1);
              bool wSolid = CollidePlayerTile(ref miniGame, playerTileX-1, playerTileY);
              bool eSolid = CollidePlayerTile(ref miniGame, playerTileX+1, playerTileY);

              if(!nSolid && !wSolid) {
                CollidePlayerTile(ref miniGame, playerTileX-1, playerTileY-1);
              }
              if(!nSolid && !eSolid) {
                CollidePlayerTile(ref miniGame, playerTileX+1, playerTileY-1);
              }
              if(!sSolid && !wSolid) {
                CollidePlayerTile(ref miniGame, playerTileX-1, playerTileY+1);
              }
              if(!sSolid && !eSolid) {
                CollidePlayerTile(ref miniGame, playerTileX+1, playerTileY+1);
              }
              
              playerLocalP = playerP - miniGameCenter;
            }
            
            for(int yokuIndex = 0; yokuIndex < miniGame.yoku.Length; ++yokuIndex) {
              Yoku yoku = miniGame.yoku[yokuIndex];

              if((miniGameTimer >= yoku.spawnTime) && (miniGameTimer < yoku.despawnTime)) {
                Vector2 p = TileToGame(yoku.x, yoku.y);
                p += tileHalfSize;

                CollideResult collide = CollideBoxBox(playerLocalP, playerHalfSize, p, tileHalfSize);
                if(collide.penetration > 0.0f) {
                  playerP += collide.correct * (collide.penetration + 1.0f);

                  if(collide.correct.Y != 0.0f) {
                    playerDy = 0.0f;
                  }

                  if(collide.correct.Y < 0.0f) {
                    playerJumps = 0;
                    playerOnGround = true;
                  }

                  break;
                }
              }

              playerLocalP = playerP - miniGameCenter;
            }

            for(int flagIndex = 0; flagIndex < miniGame.flags.Length; ++flagIndex) {
              Flag flag = miniGame.flags[flagIndex];
              Vector2 p = TileToGame(flag.tileX, flag.tileY);
              p += flagHalfSize;

              CollideResult collide = CollideBoxBox(playerLocalP, playerHalfSize, p, flagHalfSize);
              if(collide.penetration > 0.0f) {
                win = true;
                break;
              }
            }

            for(int guitarIndex = 0; guitarIndex < miniGame.guitars.Length; ++guitarIndex) {
              uint bit = (uint)1 << (byte)guitarIndex;
              if((guitarsHit & bit) == 0) {
                Guitar guitar = miniGame.guitars[guitarIndex];
                Vector2 p = TileToGame(guitar.tileX, guitar.tileY);
                p += guitarHalfSize;

                CollideResult collide = CollideBoxBox(playerLocalP, playerHalfSize, p, guitarHalfSize);
                if(collide.penetration > 0.0f) {
                  SoundEffectInstance instance = guitarSfx.CreateInstance();
                  instance.Volume = config.mainVolume;
                  instance.Play();

                  guitarsHit |= bit;
                  guitarsLeft -= 1;
                  if(guitarsLeft == 0) {
                    win = true;
                    break;
                  }
                }
              }
            }
          }


          { // Update enemies.
            Vector2 playerLocalP = playerP - miniGameCenter;

            for(int enemyIndex = 0; enemyIndex < MAX_ENEMIES; ++enemyIndex) {
              if((enemiesAlive[enemyIndex/64] & ((ulong)1 << (byte)(enemyIndex % 64))) != 0) {
                ref LiveEnemy live = ref liveEnemies[enemyIndex];
                ref Enemy enemy = ref miniGame.enemies[live.enemyIndex];

                if(live.pauseTimer > 0.0f) {
                  live.pauseTimer -= frameDt;
                }

                if(live.pauseTimer <= 0.0f) {
                  Vector2 nextPoint = enemy.points[live.pointIndex];
                  live.p = MoveToward(live.p, nextPoint, enemy.speed * frameDt);
                  if((live.p.X == nextPoint.X) && (live.p.Y == nextPoint.Y)) {
                    live.pointIndex += 1;

                    if(live.pointIndex == enemy.points.Length) {
                      enemiesAlive[enemyIndex/64] &= ~((ulong)1 << (byte)(enemyIndex % 64));
                    }
                  }

                  live.pauseIntervalTimer += frameDt;
                  if(live.pauseIntervalTimer >= enemy.pauseInterval) {
                    live.pauseIntervalTimer = 0.0f;
                    live.pauseTimer = enemy.pauseLength;
                  }
                }

                Vector2 toPlayer = playerLocalP - live.p;
                float collisionRadius = playerRadius + enemy.radius;
                if(currentMiniGameIndex == MINIGAME_TOBY2) {
                  collisionRadius = playerRadius_Toby2 + enemy.radius;
                }
                if(toPlayer.LengthSquared() < collisionRadius*collisionRadius) {
                  lose = true;
                }
              }
            }
          }


          { // Knob levels.
            // Update the knob.
            if((currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB1) || (currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB2)) {
              Vector2 playerLocalP = playerP - miniGameCenter;
              Vector2 pRotated = ComplexProd(knobP, knobRot);
              
              // @Duplication with the revolver.
              if(CircleIntersect(playerLocalP, playerRadius, pRotated, KNOB_RADIUS)) {
                Vector2 pNormalized = NormalizeOrDefault(pRotated, new Vector2(1, 0));
                Vector2 perp = new Vector2(-pNormalized.Y, pNormalized.X);
                float penetration = Vector2.Dot(playerLocalP, perp);

                Vector2 newP = (playerP - perp * penetration*2) - miniGameCenter;
                Vector2 newPNormalized = NormalizeOrDefault(newP, new Vector2(1, 0));
                float dCos = Vector2.Dot(pNormalized, newPNormalized);
                Vector2 dRot = Vector2.Zero;
                dRot.X = dCos;
                dRot.Y = Outer(pNormalized, newPNormalized);

                playerP += perp * penetration * 0.5f;

                knobRot = ComplexProd(knobRot, dRot);
                knobRot.Normalize();
                knobAngle = (float)Math.Atan2(knobRot.Y, knobRot.X);

                if(knobAngle >= MAX_KNOB_ANGLE) {
                  knobRot = new Vector2((float)Math.Cos(MAX_KNOB_ANGLE), (float)Math.Sin(MAX_KNOB_ANGLE));
                  knobAngle = MAX_KNOB_ANGLE;

                  if(currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB1) {
                    win = true;
                  }
                }
              }
            }

            // Update the I/O button.
            if(currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB2) {
              Vector2 playerLocalP = playerP - miniGameCenter;

              if(CircleIntersect(playerLocalP, playerRadius, ioButtonP, IO_BUTTON_RADIUS)) {
                if(playerMove.X > 0.0f) {
                  ioButton = true;
                } else if(playerMove.X < 0.0f) {
                  ioButton = false;
                }
              }

              if((knobAngle == MAX_KNOB_ANGLE) && ioButton) {
                win = true;
              }
            }
          }


          // Revolver.
          if(currentMiniGameIndex == MINIGAME_MOEBIUS_TRUMPET1) {
            Vector2 playerLocalP = playerP - miniGameCenter;

            for(int pIndex = 0; pIndex < revolverPs.Length; ++pIndex) {
              Vector2 p = revolverPs[pIndex];
              Vector2 pRotated = ComplexProd(p, revolverRot);

              if(CircleIntersect(playerLocalP, playerRadius, pRotated, handleRadius)) {
                Vector2 pNormalized = NormalizeOrDefault(pRotated, new Vector2(1, 0));
                Vector2 perp = new Vector2(-pNormalized.Y, pNormalized.X);
                float penetration = Vector2.Dot(playerLocalP, perp);

                Vector2 newP = (playerP - perp * penetration*2) - miniGameCenter;
                Vector2 newPNormalized = NormalizeOrDefault(newP, new Vector2(1, 0));
                float dCos = Vector2.Dot(pNormalized, newPNormalized);
                Vector2 dRot = Vector2.Zero;
                dRot.X = dCos;
                dRot.Y = Outer(pNormalized, newPNormalized);

                playerP += perp * penetration * 0.5f;

                revolverRot = ComplexProd(revolverRot, dRot);
                revolverRot.Normalize();
                revolverAngle = (float)Math.Atan2(revolverRot.Y, revolverRot.X);

                break;
              }
            }

            {
              Vector2 pRotated = ComplexProd(hammerP, revolverRot);

              if(CircleIntersect(playerLocalP, playerRadius, pRotated, hammerRadius)) {
                Vector2 Southwest = ComplexProd(new Vector2(-1, 1), revolverRot);
                float dir = Vector2.Dot(playerMove, Southwest);
                if(dir >= 0.01f) {
                  hammerAngle = -(float)Math.PI * 0.125f;
                } else if(dir <= -0.01f) {
                  hammerAngle = 0.0f;
                }
              }
            }

            // Only update the trigger when safety is off.
            if(hammerAngle != 0.0f) {
              Vector2 pRotated = ComplexProd(triggerP, revolverRot);

              if(CircleIntersect(playerLocalP, playerRadius, pRotated, hammerRadius)) {
                Vector2 west = ComplexProd(new Vector2(-1, 0), revolverRot);
                float dir = Vector2.Dot(playerMove, west);
                if(dir >= 0.01f) {
                  triggerAngle = (float)Math.PI * 0.125f;

                  SoundEffectInstance instance = shootSfx.CreateInstance();
                  instance.Play();

                  if(miniGameState == 2) {
                    win = true;
                  } else {
                    hammerAngle = 0.0f;
                  }
                }
              }
            } else if(triggerAngle != 0.0f) {
              triggerAngle = 0.0f;
            }

            if((revolverAngle >= 0.83f) && (revolverAngle <= 1.30f)) {
              if(hammerAngle != 0.0f) {
                if(triggerAngle != 0.0f) {
                  miniGameState = 3;

                  win = true;
                } else {
                  miniGameState = 2;
                }
              } else {
                miniGameState = 1;
              }
            } else {
              miniGameState = 0;
            }
          }


          if(KeyPressed(lastKeyboard, keyboard, Keys.Escape)) {
            lose = true;
          }


          // End of frame.

          miniGameTimer += frameDt;

          if(miniGameTimer >= miniGame.timeOut) {
            if((miniGame.gameplayFlags & MINIGAME_FLAG_WIN_ON_TIMEOUT) != 0) {
              win = true;
            } else {
              lose = true;
            }
          }

          if(win) {
            miniGameWon = true;

            CompleteCurrentLevel();
          } else if(lose) {
            miniGameWon = false;
          }

          if(win || lose) {
            soap = false;
            miniGameOutroTimer = 0.0f;

            if(miniGame.postMessage.Length > 0) {
              textBox.SetText(miniGame.postMessage);
              setTextThisFrame = true;
            }
          }
        } else {
          // Outro.
          miniGameOutroTimer += frameDt;
          miniGameOutroTimer = Math.Min(1.0f, miniGameOutroTimer);

          if(miniGameOutroTimer >= 1.0f) {
            inputKind = InputKind.Menu;

            if(miniGameWon) {
              ChangeMiniGame(1);
            }
          }
        }
      } else if(inputKind == InputKind.Text) {
        bool newMessage = false;

        if(textMessageCursor < 0) {
          textMessageCursor = 0;
          newMessage = true;
        } 
        if((textBox.charactersVisible >= textBox.str.Length) && (KeyPressed(lastKeyboard, keyboard, Keys.Space))) {
          textMessageCursor += 1;
          newMessage = true;
        }

        if(newMessage) {
          if(inIntroCutscene && (textMessageCursor == (textMessage.Length - 2))) {
            introFadeoutTimer = 0.0f;
          }
          if(inOutroCutscene && (textMessageCursor == (textMessage.Length - 2))) {
            outroFadeoutTimer = 0.0f;
          }

          if(textMessageCursor >= textMessage.Length) {
            if(inIntroCutscene) {
              introMusicInstance.Stop();

              int firstArrangerIndex = miniGames[0].arranger;
              arrangers[firstArrangerIndex].musicInstance.Volume = 1.0f * config.mainVolume;
              crossFadePrevArrangerIndex = firstArrangerIndex;

              for(int arrangerIndex = 0; arrangerIndex < ARRANGER_COUNT; ++arrangerIndex) {
                arrangers[arrangerIndex].musicInstance.Play();
              }

              textBox.SetText("Let's jam! Use SPACE to start playing!");
              setTextThisFrame = true;
              inIntroCutscene = false;
              crossFadeCreditsTimer = 0.0f;
              crossFadeAlpha = 0.0f;
              miniGameChangeTimer = 0.0f;
              miniGameChangeAlpha = 0.0f;
            } else if(inOutroCutscene) {
              outroMusicInstance.Stop();
              inOutroCutscene = false;

              // Fade the minigame music back in.
              crossFadePrevArrangerIndex = miniGame.arranger; // This is kind of a hack, but maybe it works?
              crossFadeLerpFactor = 0.0f;
              crossFadeCreditsTimer = 0.0f;
              crossFadeAlpha = 0.0f;
            }

            inputKind = InputKind.Menu;
          } else {
            textBox.SetText(textMessage[textMessageCursor]);
            setTextThisFrame = true;
          }
        }
      } else if(inputKind == InputKind.Item) {
        StartTextMessage(itemMessage_Soap);
        soap = true;
      }

      { // Update textbox
        float secondsPerCharacter = SECONDS_PER_CHARACTER_MEDIUM;
        if(!setTextThisFrame && KeyPressed(lastKeyboard, keyboard, Keys.Space)) {
          textBox.preventFastScrolling = false;
        }
        if(!textBox.preventFastScrolling && keyboard.IsKeyDown(Keys.Space)) {
          secondsPerCharacter = SECONDS_PER_CHARACTER_FAST;
        }
        if(textBox.timer >= secondsPerCharacter) {
          textBox.timer -= secondsPerCharacter * (float)(int)(textBox.timer / secondsPerCharacter);
          textBox.charactersVisible += 1;
        }
        textBox.timer += frameDt;
      }

      for(int arrangerIndex = 0; arrangerIndex < ARRANGER_COUNT; ++arrangerIndex) {
        ref Arranger arranger = ref arrangers[arrangerIndex];

        if(arranger.jumpTimer <= 0.0f) {
          arranger.jumpTimer += rng.Next(0, 5);
          arranger.dy = -rng.Next(50, 120);
          arranger.yOffset = -0.01f;
        }

        if(arranger.yOffset != 0.0f) {
          arranger.yOffset += arranger.dy * frameDt;
          arranger.dy += 250.0f * frameDt;
          
          arranger.yOffset = Math.Min(0.0f, arranger.yOffset);
        } else {
          arranger.jumpTimer -= frameDt;
        }
      }
    }

    lastKeyboard = keyboard;

    base.Update(gameTime);
  }

  const double TEXT_CURSOR_NUDGE_SPEED_125BPM = Math.PI * 2.0833333333333333;
  const double TEXT_CURSOR_NUDGE_SPEED_150BPM = Math.PI * 2.5;

  protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(screenColor);

    batch.Begin();

    if(inVolumeConfig) {
      Vector2 size = new Vector2(GraphicsDevice.Viewport.Width * 4 / 5, 50);
      Vector2 p = new Vector2(GraphicsDevice.Viewport.Width / 10, (GraphicsDevice.Viewport.Height - size.Y) * 0.5f);
      Vector2 cursorP = new Vector2(p.X + size.X * config.mainVolume, p.Y + size.Y * 0.5f);
      Vector2 cursorSize = new Vector2(50, 100);
      Vector2 textP = new Vector2(GraphicsDevice.Viewport.Width * 0.5f, p.Y - cursorSize.Y * 0.5f - 8 - textFont.pixelHeight - textFont.lineGap);

      DrawString(batch, textFont, "Arranged by: Keeba", new Vector2(2, 4), FontAlign.Top, FontHAlign.Left, Color.White);
      DrawString(batch, textFont, "Change volume with A and D, or the left and right arrow keys.", textP, FontAlign.Bottom, FontHAlign.Center, Color.White);
      textP.Y += textFont.pixelHeight + textFont.lineGap;
      DrawString(batch, textFont, "When you are ready, hit SPACE.", textP, FontAlign.Bottom, FontHAlign.Center, Color.White);
      batch.Draw(blankTexture, p, null, Color.White, 0.0f, Vector2.Zero, size, SpriteEffects.None, 0.0f);
      batch.Draw(blankTexture, cursorP, null, Color.White, 0.0f, new Vector2(0.5f, 0.5f), cursorSize, SpriteEffects.None, 0.0f);
    } else {
      ref MiniGame miniGame = ref miniGames[currentMiniGameIndex];
      ref Arranger arranger = ref arrangers[miniGame.arranger];

      if(!inIntroCutscene && !inOutroCutscene) {
        // Background.
        Color boatColor = new Color(80, 80, 80, 255);
        Color spotlightColor = Color.White * 0.4f;
        batch.Draw(boatTexture, Vector2.Zero, null, boatColor, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);

        if(inputKind != InputKind.Action) {
          Vector2 arrangerP = miniGameCenter - miniGameHalfSize;
          arrangerP.Y += arranger.yOffset;

          Texture2D texture = arranger.texture;
          if((miniGame.arranger == ARRANGER_TIME) && (currentMiniGameIndex == MINIGAME_TIME_YOKU4)) {
            texture = fourTexture;
          }

          batch.Draw(texture, arrangerP, null, Color.White, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
          batch.Draw(spotlightTexture, Vector2.Zero, null, spotlightColor, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
        }

        for(int arrangerIndex = 0; arrangerIndex < ARRANGER_COUNT; ++arrangerIndex) {
          ref Arranger arr = ref arrangers[arrangerIndex];

          if(allMiniGamesCompleted || AllBitsSet(miniGamesCompleted, arr.miniGameStartIndex, arr.miniGameOnePastLastIndex)) {
            if((arr.displayP.X != 0.0f) || (arr.displayP.Y != 0.0f)) {
              Vector2 arrP = arr.displayP;
              arrP.Y += arr.yOffset * 0.7f;

              batch.Draw(arr.texture, arrP, null, boatColor, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            }
          }
        }
      }

      if(inputKind == InputKind.Action) {
        bool Outro = (miniGameOutroTimer >= 0.0f);
        float rectTimer = miniGameIntroTimer*miniGameIntroTimer;
        float rectWidthTimer  = Math.Min(0.5f, rectTimer) * 2.0f;
        float rectHeightTimer = Math.Max(0.0f, rectTimer - 0.5f) * 2.0f;
        Vector2 halfSize = new Vector2(rectWidthTimer * miniGameHalfSize.X, rectHeightTimer * miniGameHalfSize.Y);
        Vector2 innerHalfSize = new Vector2(halfSize.X - (float)MINIGAME_RECT_THICKNESS, halfSize.Y - (float)MINIGAME_RECT_THICKNESS);
        Rectangle miniGameRect = new Rectangle((int)(miniGameCenter.X - halfSize.X), (int)(miniGameCenter.Y - halfSize.Y), (int)(halfSize.X * 2), (int)(halfSize.Y * 2));
        Rectangle miniGameInnerRect = new Rectangle((int)(miniGameCenter.X - innerHalfSize.X), (int)(miniGameCenter.Y - innerHalfSize.Y), (int)(innerHalfSize.X * 2), (int)(innerHalfSize.Y * 2));

        batch.Draw(blankTexture, miniGameRect, Color.Black);

        if((miniGameIntroTimer >= 1.0f) && !Outro) {
          if(miniGame.background != null) {
            batch.Draw(miniGame.background, miniGameInnerRect, Color.White);
          }

          if(currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB2) {
            if(!ioButton) {
              batch.Draw(moebius2_1Texture, miniGameInnerRect, Color.White);
            } else {
              batch.Draw(moebius2_2Texture, miniGameInnerRect, Color.White);
            }
          }

          // Tiles
          if(miniGame.tiles.Length > 0) {
            for(byte tileY = 0; tileY < TILE_MAP_SIZE; ++tileY) {
              for(byte tileX = 0; tileX < TILE_MAP_SIZE; ++tileX) {
                if(miniGame.tiles[tileY * TILE_MAP_SIZE + tileX] != 0) {
                  Vector2 p = TileToScreen(tileX, tileY);
                  batch.Draw(blankTexture, p, null, Color.White, 0.0f, Vector2.Zero, tileHalfSize * 2.0f, SpriteEffects.None, 0.0f);
                }
              }
            }
          }

          // Yoku
          for(int yokuIndex = 0; yokuIndex < miniGame.yoku.Length; ++yokuIndex) {
            Yoku yoku = miniGame.yoku[yokuIndex];

            if((miniGameTimer >= yoku.spawnTime) && (miniGameTimer < yoku.despawnTime)) {
              Vector2 p = TileToScreen(yoku.x, yoku.y);
              batch.Draw(blankTexture, p, null, Color.White, 0, Vector2.Zero, tileHalfSize * 2.0f, SpriteEffects.None, 0);
            }
          }

          // Flags
          for(int flagIndex = 0; flagIndex < miniGame.flags.Length; ++flagIndex) {
            Flag flag = miniGame.flags[flagIndex];
            Vector2 p = TileToScreen(flag.tileX, flag.tileY);
            batch.Draw(flagTexture, p, null, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
          }

          // Enemies
          int enemyCount = 0;
          Vector2 circleTextureOrigin = new Vector2(circleTexture.Width/2, circleTexture.Height/2);
          batch.Draw(circleTexture, playerP, null, Color.Cyan, 0, circleTextureOrigin, Vector2.One, SpriteEffects.None, 0);
          for(int enemyIndex = 0; enemyIndex < MAX_ENEMIES; ++enemyIndex) {
            if((enemiesAlive[enemyIndex/64] & (ulong)1 << (byte)(enemyIndex % 64)) != 0) {
              ref LiveEnemy live = ref liveEnemies[enemyIndex];
              float radius = miniGame.enemies[live.enemyIndex].radius;
              Vector2 p = GameToScreen(live.p);

              batch.Draw(circleTexture, p, null, Color.Red, 0, circleTextureOrigin, new Vector2(radius*2 / circleTexture.Width, radius*2 / circleTexture.Height), SpriteEffects.None, 0);
              enemyCount += 1;
            }
          }

          // Guitars
          for(int guitarIndex = 0; guitarIndex < miniGame.guitars.Length; ++guitarIndex) {
            uint bit = (uint)1 << (byte)guitarIndex;
            Guitar guitar = miniGame.guitars[guitarIndex];

            Vector2 p = TileToScreen(guitar.tileX, guitar.tileY);
            Color c = Color.White;
            if((guitarsHit & bit) != 0) {
              c = Color.Lime;
            }
            batch.Draw(guitarTexture, p, null, c, 0.0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
          }

          // Level-specific setpieces
          if((currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB1) || (currentMiniGameIndex == MINIGAME_MOEBIUS_KNOB2)) {
            batch.Draw(knobTexture, miniGameCenter, null, Color.White, knobAngle, halfSize, 1.0f, SpriteEffects.None, 0.0f);
          }
          if(currentMiniGameIndex == MINIGAME_MOEBIUS_TRUMPET1) {
            Color color2 = Color.White;
            Color color3 = Color.White;
            Color color4 = Color.White;

            if((helpTextRead & ((ulong)1 << (byte)HELP_TEXT_TIME)) != 0) {
              if(miniGameState == 0) {
                color2 = Color.Red;
              } else if(miniGameState == 1) {
                color3 = Color.Red;
              } else if(miniGameState == 2) {
                color4 = Color.Red;
              }
            }

            batch.Draw(moebius3_1Texture, miniGameCenter, null, Color.White, revolverAngle, halfSize, 1.0f, SpriteEffects.None, 0.0f);
            batch.Draw(moebius3_2Texture, miniGameCenter, null, color2, revolverAngle, halfSize, 1.0f, SpriteEffects.None, 0.0f);
            batch.Draw(moebius3_3Texture, miniGameCenter, null, color3, revolverAngle + hammerAngle, halfSize, 1.0f, SpriteEffects.None, 0.0f);
            batch.Draw(moebius3_4Texture, miniGameCenter, null, color4, revolverAngle + triggerAngle, halfSize, 1.0f, SpriteEffects.None, 0.0f);
          }

          { // Timer bar.
            Rectangle timeBarRect = new Rectangle(miniGameRect.X, miniGameRect.Y + (int)(halfSize.Y*2) + 12, miniGameRect.Width, 26);
            DrawRectOutline(batch, timeBarRect, 5, Color.White);
            
            float timeOutT = 1.0f - (miniGameTimer / miniGame.timeOut);
            float timeOutMaxWidth = timeBarRect.Width - 14;
            Rectangle timeLeftRect = new Rectangle(timeBarRect.X + 7, timeBarRect.Y + 7, (int)(timeOutMaxWidth * timeOutT), timeBarRect.Height - 14);
            batch.Draw(blankTexture, timeLeftRect, Color.White);
          }
        }

        if(!Outro) {
          float hintAlpha = Math.Min(miniGameIntroTimer * 4.0f, 1.0f);
          DrawString(batch, textFont, miniGame.hint, miniGameHintP, FontAlign.Bottom, FontHAlign.Center, Color.White * hintAlpha);
        }
        
        Color color = Color.White;
        if(Outro) {
          if(miniGameWon) {
            color = Color.Lime;
          } else {
            color = Color.Red;
          }

          color = color * (float)Math.Sin(miniGameOutroTimer * 30.0f);
        }
        DrawRectOutline(batch, miniGameRect, 5, color);
      }

      // UI
      if(inIntroCutscene) {
        float alpha = 1.0f;
        if(introFadeoutTimer != 2.0f) {
          alpha = 1.0f - introFadeoutTimer;
        }

        DrawString(batch, textFont, "Arranged by: Mel", new Vector2(2, 4), FontAlign.Top, FontHAlign.Left, Color.White * alpha);
      } else if(inOutroCutscene) {
        float alpha = 1.0f;
        if(outroFadeoutTimer != 2.0f) {
          alpha = 1.0f - outroFadeoutTimer;
        }

        DrawString(batch, textFont, "Arranged by: Lampie", new Vector2(2, 4), FontAlign.Top, FontHAlign.Left, Color.White * alpha);
      } else {
        if(crossFadeAlpha > 0.0f) {
          DrawString(batch, textFont, "Arranged by: " + arranger.displayName, new Vector2(2, 4), FontAlign.Top, FontHAlign.Left, Color.White * crossFadeAlpha);
        }
        if(miniGameChangeAlpha > 0.0f) {
          DrawString(batch, textFont, "Level " + (currentMiniGameIndex - arranger.miniGameStartIndex + 1), new Vector2(2, 4 + textFont.pixelHeight + textFont.lineGap), FontAlign.Top, FontHAlign.Left, Color.White * miniGameChangeAlpha);
        }
      }

      int panelH = GraphicsDevice.Viewport.Height / 4;
      int thickness = 12;
      int marginX = thickness + 20;
      int marginY = thickness + 18;
      int mainRectWidth = (int)(GraphicsDevice.Viewport.Width*0.6);
      if(inIntroCutscene || inOutroCutscene) {
        // Make the textbox take up the entire width of the screen.
        mainRectWidth = (int)GraphicsDevice.Viewport.Width - thickness;
      }
      { // Main rect
        Rectangle rect = new Rectangle(0, GraphicsDevice.Viewport.Height - 1 - panelH, mainRectWidth + thickness, panelH);
        batch.Draw(blankTexture, rect, Color.Black);
        DrawRectOutline(batch, rect, thickness, Color.White);

        if(inputKind != InputKind.Item) {
          Vector2 lineP = new Vector2(rect.X + marginX, rect.Y + marginY);
          Vector2 p = DrawString(batch, textFont, textBox.str, lineP, FontAlign.Top, FontHAlign.Left, Color.White, textBox.charactersVisible);
          // lineP.Y += textFont.GetLineYAdvance();
          // Vector2 p = DrawString(batch, textFont, textBox.str, lineP, FontAlign.Top, Color.White, textBox.charactersVisible);

          if((inputKind == InputKind.Text) && (textBox.charactersVisible >= textBox.str.Length)) {
            p.Y += (float)(Math.Sin(gameTime.TotalGameTime.TotalSeconds * TEXT_CURSOR_NUDGE_SPEED_125BPM) * 5.0);
            batch.Draw(textCursorTexture, p, Color.White);
          }
        }
      }
      if(!inIntroCutscene && !inOutroCutscene) { // Command rect
        Rectangle rect = new Rectangle(mainRectWidth, GraphicsDevice.Viewport.Height - 1 - panelH, GraphicsDevice.Viewport.Width - mainRectWidth, panelH);
        batch.Draw(blankTexture, rect, Color.Black);
        DrawRectOutline(batch, rect, thickness, Color.White);
        DrawString(batch, commandFont, "ACT", new Vector2(rect.X + marginX, rect.Y + marginY), FontAlign.Top, FontHAlign.Left, Color.White);
        DrawString(batch, commandFont, "SOAP", new Vector2(rect.X + rect.Width/2 + marginX, rect.Y + marginY), FontAlign.Top, FontHAlign.Left, Color.White);
        DrawString(batch, commandFont, "HELP", new Vector2(rect.X + marginX, rect.Y + rect.Height - marginY), FontAlign.Bottom, FontHAlign.Left, Color.White);
        // @Incomplete
        DrawString(batch, commandFont, "SKIP", new Vector2(rect.X + rect.Width/2 + marginX, rect.Y + rect.Height - marginY), FontAlign.Bottom, FontHAlign.Left, Color.White);

        if(inputKind == InputKind.Menu) {
          int cursorX = commandCursor & 1;
          int cursorY = (commandCursor & 2) >> 1;
          Rectangle cursorRect = new Rectangle(rect.X + thickness, rect.Y + thickness, rect.Width/2 - thickness, rect.Height/2 - thickness);
          cursorRect.X += cursorX * (rect.Width/2 - thickness);
          cursorRect.Y += cursorY * (rect.Height/2 - thickness);
          DrawRectOutline(batch, cursorRect, 5, Color.Orange);
        }
      }
    }

    batch.End();

    base.Draw(gameTime);
  }

  enum FontAlign {
    Bottom = 0,
    Top = 1,
  }

  enum FontHAlign {
    Left = 0,
    Center = 1,
  }

  void DrawRectOutline(SpriteBatch batch, Rectangle rect, int thickness, Color color) {
    batch.Draw(blankTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
    batch.Draw(blankTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
    batch.Draw(blankTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    batch.Draw(blankTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
  }

  const int MAX_STRING_LENGTH = 256;
  float[] glyphXs = new float[MAX_STRING_LENGTH];

  Vector2 DrawString(SpriteBatch batch, Font font, string str, Vector2 p, FontAlign align, FontHAlign halign, Color color, int length = -1) {
    if(length < 0) {
      length = str.Length;
    }
    length = Math.Min(str.Length, length);

    if(align == FontAlign.Top) {
      FontGlyph m = font.glyphs['M' - ' '];
      p.Y += m.y1 - m.y0;
    }

    float startX = p.X;

    char last = '\0';
    for(int charIndex = 0; charIndex < length; ++charIndex) {
      char c = str[charIndex];
      if(last != '\0') {
        for(int kernIndex = 0; kernIndex < font.kerning.Length; ++kernIndex) {
          KerningPair pair = font.kerning[kernIndex];

          if((pair.c0 == last) && (pair.c1 == c)) {
            p.X += pair.kern;
            break;
          } else if(pair.c0 > last) {
            break;
          }
        }
      }


      FontGlyph glyph = font.glyphs[c - ' '];
      glyphXs[charIndex] = p.X + glyph.offsetX;
      p.X += glyph.advanceX;
      
      last = c;
    }

    float totalLength = p.X - startX;
    float hAlignOffset = 0.0f;
    if(halign == FontHAlign.Center) {
      hAlignOffset = -totalLength * 0.5f;
    }

    for(int charIndex = 0; charIndex < length; ++charIndex) {
      char c = str[charIndex];
      float x = glyphXs[charIndex] + hAlignOffset;
      FontGlyph glyph = font.glyphs[c - ' '];

      batch.Draw(font.texture, new Vector2(x, p.Y + glyph.offsetY), new Rectangle(glyph.x0, glyph.y0, glyph.x1 - glyph.x0, glyph.y1 - glyph.y0), color);
    }

    return p;
  }
}
