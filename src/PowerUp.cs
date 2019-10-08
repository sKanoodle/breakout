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
    class PowerUp : RectangularObject
    {
        private static Random _Random = new Random();
        private float _FallSpeed = 50;
        public Kinds Kind;
        private List<PowerUp> _ListReference;
        private static int[] _Weights = new int[]
        {
            100,//BiggerBall,
            50, //SmallerBall,
            100,//FasterBall,
            50, //SlowerBall,
            10, //LavaBall,
            200,//BiggerBat,
            60, //SmallerBat,
            200,//FasterBat,
            100,//SlowerBat,
            5,  //InvertedBat,
            20, //ExtraLive,
            15, //StickyBat,
        };

        public PowerUp(float x, float y, List<PowerUp> listReference)
        {
            Initialize(x, y, GetRandomKind(), listReference);
        }

        public PowerUp(float x, float y, Kinds kind, List<PowerUp> listReference)
        {
            Initialize(x, y, kind, listReference);
        }

        private void Initialize(float x, float y, Kinds kind, List<PowerUp> listReference)
        {
            Size = new Vector2(20, 20);
            Direction = new Vector2(0, 1);
            Position = new Vector2(x, y);
            Kind = kind;
            _ListReference = listReference;
        }

        private Kinds GetRandomKind()
        {
            int sum = _Weights.Sum();
            int index = 0;
            double r = _Random.NextDouble();
            while (index < _Weights.Length)
            {
                if (r > _Weights.Where((_, i) => i > index).Sum() / (float)sum)
                    return (Kinds)index;
                index += 1;
            }
            throw new Exception();
        }

        public Kinds? Move(Bat bat, float elapsed)
        {
            Position += Direction * _FallSpeed * elapsed;
            if (Position.Y > bat.Position.Y - Size.Y)
            {
                if (Position.X > bat.Position.X - Size.X && Position.X < bat.Position.X + bat.Size.X)
                {
                    _ListReference.Remove(this);
                    return Kind;
                }
            }
            if (Position.Y > bat.Position.Y)
                _ListReference.Remove(this);
            return null;
        }

        public override RawRectangleF GetTileLocation()
        {
            if ((int)Kind < 7)
                return new RawRectangleF(197, 121, 213, 137);
            return new RawRectangleF(215, 121, 231, 137);
        }

        public enum Kinds
        {
            BiggerBall,
            FasterBall,
            LavaBall,
            BiggerBat,
            FasterBat,
            ExtraLive,
            StickyBat,
            SmallerBall,
            SlowerBall,
            SmallerBat,
            SlowerBat,
            InvertedBat,
        }
    }
}
