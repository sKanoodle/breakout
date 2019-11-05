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
    class Brick : RectangularObject
    {
        private static readonly Random Random = new Random();
        private const int FullHP = 60;
        public readonly Kinds Kind;
        public int Hitpoints = FullHP;
        public readonly int DamageOnHit;
        private readonly List<Brick> ListReference;
        private readonly RawRectangleF TileLocation;
        private static readonly int[] Weights = new int[] 
        {
            100, //1hit
            20,  //2hit
            15,  //3hit
            10,  //4hit
            5,   //5hit
            2,   //6hit
            1,   //indestructible
            20,  //powerup
            0,   //none
        };

        public Brick(float x, float y, List<Brick> listReference, Kinds? kind = null)
            : base(new Vector2(50, 20), default, new Vector2(x, y))
        {
            if (kind.HasValue)
                Kind = kind.Value;
            else
                Kind = GetRandomKind();

            ListReference = listReference;

            DamageOnHit = FullHP / (1 + (int)Kind);
            if (Kind == Kinds.Indestructible)
                DamageOnHit = 0;
            else if (Kind == Kinds.PowerUP)
                DamageOnHit = FullHP;

            TileLocation = new RawRectangleF(1, 1 + 18 * (int)Kind, 33, 17 + 18 * (int)Kind);
        }

        private Kinds GetRandomKind()
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

        public bool Hit(out bool dropPowerUp, bool isLavaBall = false)
        {
            dropPowerUp = false;
            Hitpoints -= DamageOnHit;
            if (Hitpoints <= 0 || isLavaBall)
            {
                ListReference.Remove(this);
                if (Kind == Kinds.PowerUP)
                    dropPowerUp = true;
                return true;
            }
            return false;
        }

        public new void Render(RenderTarget render, Bitmap Tileset)
        {
            render.DrawBitmap(Tileset, GetDrawDestination(), 1, BitmapInterpolationMode.Linear, TileLocation);
            int decayState = Hitpoints switch
            {
                FullHP => 0,
                _ when Hitpoints > FullHP * 0.8 => 1,
                _ when Hitpoints > FullHP * 0.4 => 2,
                _ => 3,
            };

            render.DrawBitmap(Tileset, GetDrawDestination(), 1, BitmapInterpolationMode.Linear, new RawRectangleF(186 + 34 * decayState, 92, 218 + 34 * decayState, 108));
        }

        public override RawRectangleF GetTileLocation()
        {
            return TileLocation;
        }

        //TODO: find a different way to pass the brick to delete
        public IEnumerable<Tuple<LineSegment, Brick>> Boundaries(Ball Ball)
        {
            float top = Position.Y - Ball.Size.Y;
            float left = Position.X - Ball.Size.X;
            float bottom = Position.Y + Size.Y;
            float right = Position.X + Size.X;
            yield return new Tuple<LineSegment, Brick>(new LineSegment()
            {
                StartPosition = new Vector2(left, top),
                EndPosition = new Vector2(left, bottom),
            }, this);
            yield return new Tuple<LineSegment, Brick>(new LineSegment()
            {
                EndPosition = new Vector2(left, bottom),
                StartPosition = new Vector2(right, bottom),
            }, this);
            yield return new Tuple<LineSegment, Brick>(new LineSegment()
            {
                EndPosition = new Vector2(right, bottom),
                StartPosition = new Vector2(right, top),
            }, this);
            yield return new Tuple<LineSegment, Brick>(new LineSegment()
            {
                EndPosition = new Vector2(right, top),
                StartPosition = new Vector2(left, top),
            }, this);
        }

        public enum Kinds
        {
            OneHit,
            TwoHit,
            ThreeHit,
            FourHit,
            FiveHit,
            SixHit,
            Indestructible,
            PowerUP,
            None,
        }
    }
}
