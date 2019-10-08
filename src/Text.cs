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
        private RawRectangleF _TileLocation;

        public Character(float x, float y, float width, float height, char c)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(width, height);

            int offset = 0;
            if (c >= '0' && c <= '9')
                offset = -22;
            else if (c >= 'A' && c <= 'Z')
                offset = -65;
            else if (c >= 'a' && c <= 'z')
                offset = -97;
            else if (c == ':')
                offset = -22;
            else if (c == '!')
                offset = 4;
            else if (c == '\'')
                offset = -1;
            else
                offset = 100;
            _TileLocation = new RawRectangleF(38 + 6 * (c + offset), 1, 43 + 6 * (c + offset), 9);
        }

        public override RawRectangleF GetTileLocation()
        {
            return _TileLocation;
        }
    }
}
