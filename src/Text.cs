using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakout
{
    static class Text
    {
        public static void Render(float x, float y, float size, string text, RenderTarget render, Bitmap tileset)
        {
            float width = size * 5 / 8;
            var characters = text
                .Select((c, i) => new Character(x + (width + 1) * i, y, width, size, c));
            foreach (var c in characters)
                c.Render(render, tileset);
        }
    }

    class Character : RectangularObject
    {
        private readonly RawRectangleF TileLocation;

        public Character(float x, float y, float width, float height, char c)
            : base(new Vector2(width, height), default, new Vector2(x, y))
        {
            int offset = c switch
            {
                ':' => -22,
                '!' => 4,
                '\'' => -1,
                _ when c >= '0' && c <= '9' => -22,
                _ when c >= 'A' && c <= 'Z' => -65,
                _ when c >= 'a' && c <= 'z' => -97,
                _ => 100,
            };
            TileLocation = new RawRectangleF(38 + 6 * (c + offset), 1, 43 + 6 * (c + offset), 9);
        }

        public override RawRectangleF GetTileLocation()
        {
            return TileLocation;
        }
    }
}
