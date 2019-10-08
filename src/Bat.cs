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
    class Bat : RectangularObject
    {
        private RawRectangleF TileLocation;

        private float _SpeedModifier = 1;
        private float _Speed;
        public float Speed
        {
            get
            {
                return _Speed * _SpeedModifier;
            }
            set
            {
                _Speed = value;
            }
        }

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
                TileLocation = new RawRectangleF(8 + 68 * (int)_Kind, 151, 72 + 68 * (int)_Kind, 171);
                if (_Kind == Kinds.Slow)
                    _SpeedModifier = 0.5f;
                else if (_Kind == Kinds.Fast)
                    _SpeedModifier = 2f;
                else
                    _SpeedModifier = 1f;
            }
        }

        public Bat(float x, float y, float speed, Kinds kind)
        {
            Position = new Vector2(x, y);
            Size = new Vector2(80, 15);
            Speed = speed;
            Direction = Vector2.Zero;
            Kind = kind;
        }

        public override RawRectangleF GetTileLocation()
        {
            return TileLocation;
        }

        public IEnumerable<Tuple<LineSegment, Brick>> Boundaries(Ball ball)
        {
            float top = Position.Y - ball.Size.Y;
            float left = Position.X - ball.Size.X;
            float bottom = Position.Y + Size.Y;
            float right = Position.X + Size.X;

            //yield return new Tuple<LineSegment, Brick>(new LineSegment()
            //{
            //    StartPosition = new Vector2(left, top),
            //    EndPosition = new Vector2(left, bottom),
            //}, null);
            //yield return new Tuple<LineSegment, Brick>(new LineSegment()
            //{
            //    EndPosition = new Vector2(left, bottom),
            //    StartPosition = new Vector2(right, bottom),
            //}, null);
            //yield return new Tuple<LineSegment, Brick>(new LineSegment()
            //{
            //    EndPosition = new Vector2(right, bottom),
            //    StartPosition = new Vector2(right, top),
            //}, null);
            yield return new Tuple<LineSegment, Brick>(new LineSegment()
            {
                EndPosition = new Vector2(right, top),
                StartPosition = new Vector2(left, top),
            }, null);
        }

        public enum Kinds
        {
            Regular,
            Sticky,
            Inverted,
            Slow,
            Fast,
        }
    }
}
