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
    class Ball : RectangularObject
    {
        public float Speed;
        public float SpeedMultiplier = 1;
        public float Acceleration;
        private RawRectangleF TileLocation;
        private Kinds _Kind;
        public Kinds Kind
        {
            get
            {
                return _Kind;
            }
            set
            {
                _Kind = value;
                TileLocation = new RawRectangleF(48 + 9 * (int)_Kind, 136, 56 + 9 * (int)_Kind, 144);
            }
        }

        public Ball(float positionX, float positionY, float directionX, float directionY, float speed, float acceleration, Kinds kind)
            : base(new Vector2(10, 10), new Vector2(directionX, directionY), new Vector2(positionX, positionY))
        {
            Speed = speed;
            Acceleration = acceleration;
            Kind = kind;
        }

        public override RawRectangleF GetTileLocation()
        {
            return TileLocation;
        }

        public Line ToLine()
        {
            return new Line()
            {
                Position = Position,
                Direction = Direction,
            };
        }

        public LineSegment ToLineSegment()
        {
            return new LineSegment()
            {
                StartPosition = Position,
                EndPosition = Position + Direction,
            };
        }

        public enum Kinds
        {
            Regular,
            Lava,
        }
    }
}
