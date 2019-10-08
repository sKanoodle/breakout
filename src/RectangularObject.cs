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
    abstract class RectangularObject
    {
        public Vector2 Size;
        public Vector2 Direction;
        public Vector2 Position;

        public abstract RawRectangleF GetTileLocation();

        public RectangleF GetDrawDestination()
        {
            return new RectangleF(Position.X, Position.Y, Size.X, Size.Y);
        }

        public void Render(RenderTarget render, Bitmap tileset)
        {
            render.DrawBitmap(tileset, GetDrawDestination(), 1, BitmapInterpolationMode.Linear, GetTileLocation());
        }
    }
}
