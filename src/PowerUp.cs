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
        private static readonly Random Random = new Random();
        private readonly float FallSpeed = 50;
        public Kinds Kind;
        private readonly List<PowerUp> ListReference;
        private static readonly int[] Weights = new int[]
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
            : this(x, y, GetRandomKind(), listReference) { }

        public PowerUp(float x, float y, Kinds kind, List<PowerUp> listReference)
            : base(new Vector2(20, 20), new Vector2(0, 1), new Vector2(x, y))
        {
            Kind = kind;
            ListReference = listReference;
        }

        private static Kinds GetRandomKind()
        {
            int sum = Weights.Sum();
            int index = 0;
            double r = Random.NextDouble();
            while (index < Weights.Length)
            {
                if (r > Weights.Where((_, i) => i > index).Sum() / (float)sum)
                    return (Kinds)index;
                index += 1;
            }
            throw new Exception();
        }

        public Kinds? Move(Bat bat, float elapsed)
        {
            Position += Direction * FallSpeed * elapsed;
            if (Position.Y > bat.Position.Y - Size.Y)
            {
                if (Position.X > bat.Position.X - Size.X && Position.X < bat.Position.X + bat.Size.X)
                {
                    ListReference.Remove(this);
                    return Kind;
                }
            }
            if (Position.Y > bat.Position.Y)
                ListReference.Remove(this);
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
