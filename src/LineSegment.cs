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
    class LineSegment
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;

        public Vector2 Direction
        {
            get
            {
                return EndPosition - StartPosition;
            }
        }

        public float Length
        {
            get
            {
                return (EndPosition - StartPosition).Length();
            }
        }

        public Line ToLine()
        {
            return new Line()
            {
                Position = StartPosition,
                Direction = Direction,
            };
        }
    }

    class Line
    {
        public Vector2 Position;
        public Vector2 Direction;
    }

    class Circle
    {
        public Vector2 Position;
        public float Radius;
    }
}
