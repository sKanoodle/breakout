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
        private static Random _Random = new Random();
        public static int FullHP = 60;
        public readonly Kinds Kind;
        public int Hitpoints = FullHP;
        public readonly int DamageOnHit;
        private List<Brick> _ListReference;
        private RawRectangleF _TileLocation;
        private static int[] _Weights = new int[] 
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
        {
            if (kind.HasValue)
                Kind = kind.Value;
            else
                Kind = GetRandomKind();

            _ListReference = listReference;
            Position = new Vector2(x, y);
            Size = new Vector2(50, 20);

            DamageOnHit = FullHP / (1 + (int)Kind);
            if (Kind == Kinds.Indestructible)
                DamageOnHit = 0;
            else if (Kind == Kinds.PowerUP)
                DamageOnHit = FullHP;

            _TileLocation = new RawRectangleF(1, 1 + 18 * (int)Kind, 33, 17 + 18 * (int)Kind);
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

        public bool Hit(out bool dropPowerUp, bool isLavaBall = false)
        {
            dropPowerUp = false;
            Hitpoints -= DamageOnHit;
            if (Hitpoints <= 0 || isLavaBall)
            {
                _ListReference.Remove(this);
                if (Kind == Kinds.PowerUP)
                    dropPowerUp = true;
                return true;
            }
            return false;
        }

        public new void Render(RenderTarget render, Bitmap Tileset)
        {
            render.DrawBitmap(Tileset, GetDrawDestination(), 1, BitmapInterpolationMode.Linear, _TileLocation);
            int decayState = -1;
            if (Hitpoints == FullHP)
                decayState = 0;
            else if (Hitpoints > FullHP * 0.8)
                decayState = 1;
            else if (Hitpoints > FullHP * 0.4)
                decayState = 2;
            else if (Hitpoints > 0)
                decayState = 3;
            else
                throw new Exception();

            render.DrawBitmap(Tileset, GetDrawDestination(), 1, BitmapInterpolationMode.Linear, new RawRectangleF(186 + 34 * decayState, 92, 218 + 34 * decayState, 108));
        }

        public override RawRectangleF GetTileLocation()
        {
            return _TileLocation;
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
