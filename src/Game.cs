using Newtonsoft.Json;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakout
{
    class Game : IDisposable
    {
        SolidColorBrush DebugBrush;
        Random random = new Random();
        DirectInput DirectInput;
        Keyboard Keyboard;
        Bitmap Tileset;
        Color4 BackgroundColor;

        Ball Ball;
        Bat Bat;
        Vector2 Size;
        Vector2 BoundaryCoordinates;

        private readonly string[] Levels;

        int CurrentLevel = 0;
        int Score = 0;

        float RandomBounceMarginPercentage = 2f;

        private List<PowerUp> PowerUps = new List<PowerUp>();
        private List<Brick> Bricks = new List<Brick>();
        private List<Tuple<LineSegment, Brick>> Boundaries
        {
            get
            {
                List<Tuple<LineSegment, Brick>> result = new List<Tuple<LineSegment, Brick>>();
                foreach (var segment in Bat.Boundaries(Ball))
                    result.Add(segment);

                foreach (Brick brick in Bricks)
                    foreach (var segment in brick.Boundaries(Ball))
                        result.Add(segment);

                foreach (var segment in BoundaryFactory(0, 0, BoundaryCoordinates.Y, BoundaryCoordinates.X))
                    result.Add(new Tuple<LineSegment, Brick>(segment, null));

                return result;
            }
        }

        public Game(int width, int height)
        {
            using (Stream stream = File.OpenRead(Path.GetFullPath("maps/level-definition.json")))
            using (StreamReader sr = new StreamReader(stream))
                Levels = JsonConvert.DeserializeObject<string[]>(sr.ReadToEnd());
            BoundaryCoordinates = new Vector2(width, height);
            NextLevel();

            Bat = new Bat(width / 2f, height - 20, 400, Bat.Kinds.Regular);
        }

        private List<Brick> LoadLevel(string levelName)
        {
            List<Brick> result = new List<Brick>();
            Brick.Kinds[,] kinds;
            using (Stream stream = File.OpenRead(Path.GetFullPath($"maps/{levelName}.json")))
            using (StreamReader sr = new StreamReader(stream))
                kinds = JsonConvert.DeserializeObject<Brick.Kinds[,]>(sr.ReadToEnd());
            for (int x = 0; x < kinds.GetLength(1); x++)
                for (int y = 0; y < kinds.GetLength(0); y++)
                    if (kinds[y, x] != Brick.Kinds.None)
                        result.Add(new Brick(50 * x, 40 + 20 * y, result, kinds[y, x]));
            return result;
        }

        public IEnumerable<LineSegment> BoundaryFactory(float top, float left, float bottom, float right)
        {
            bottom -= Ball.Size.Y;
            right -= Ball.Size.X;
            yield return new LineSegment()
            {
                StartPosition = new Vector2(left, top),
                EndPosition = new Vector2(left, bottom),
            };
            yield return new LineSegment()
            {
                EndPosition = new Vector2(left, bottom),
                StartPosition = new Vector2(right, bottom),
            };
            yield return new LineSegment()
            {
                EndPosition = new Vector2(right, bottom),
                StartPosition = new Vector2(right, top),
            };
            yield return new LineSegment()
            {
                EndPosition = new Vector2(right, top),
                StartPosition = new Vector2(left, top),
            };
        }

        public void LoadResources(RenderTarget render)
        {
            BackgroundColor = Color.CornflowerBlue;
            Tileset = Resources.LoadImageFromFile(render, "breakout_pieces.png");
            DebugBrush = new SolidColorBrush(render, new RawColor4(123f, 123f, 123f, 123f));

            DirectInput = new DirectInput();
            Keyboard = new Keyboard(DirectInput);
            Keyboard.Acquire();
        }

        public void DrawScene(RenderTarget render)
        {
            render.BeginDraw();
            render.Clear(BackgroundColor);
            Ball.Render(render, Tileset);
            Bat.Render(render, Tileset);
            foreach (Brick brick in Bricks)
                brick.Render(render, Tileset);
            foreach (PowerUp powerUp in PowerUps)
                powerUp.Render(render, Tileset);
            Text.Render(10, 10, 20, String.Format("Level: {0:00} Score: {1}",CurrentLevel ,Score), render, Tileset);
            render.EndDraw();
        }

        public void Update(float elapsed)
        {
            MoveBat(Keyboard.GetCurrentState(), elapsed);
            MoveBall(elapsed);
            MovePowerUps(elapsed);
        }

        private void NextLevel()
        {
            if (CurrentLevel < Levels.Length)
                Bricks = LoadLevel(Levels[CurrentLevel]);
            else
                for (int i = 0; i < 780; i += 50)
                    for (int k = 40; k < 360; k += 20)
                        Bricks.Add(new Brick(i, k, Bricks));

            Ball = new Ball(BoundaryCoordinates.X / 2, BoundaryCoordinates.Y - 100, 23, -10, 200, 0, Ball.Kinds.Regular);
            CurrentLevel += 1;
        }

        private void GameOver()
        {
            throw new NotImplementedException();
        }

        private void MoveBat(KeyboardState keyboardState, float elapsed)
        {
            Bat.Direction = Vector2.Zero;
            if (keyboardState.IsPressed(Key.Left))
                Bat.Direction.X += -1;
            if (keyboardState.IsPressed(Key.Right))
                Bat.Direction.X += +1;

            if (Bat.Kind == Bat.Kinds.Inverted)
                Bat.Position = Bat.Position - Bat.Direction * Bat.Speed * elapsed;
            else
                Bat.Position = Bat.Position + Bat.Direction * Bat.Speed * elapsed;

            //CHEATMODE
            //Bat.Position.X = Ball.Position.X - random.NextFloat(0, Bat.Size.X);

            if (Bat.Position.X < 0)
                Bat.Position.X = 0;
            else if (Bat.Position.X > BoundaryCoordinates.X - Bat.Size.X)
                Bat.Position.X = BoundaryCoordinates.X - Bat.Size.X;
        }

        private void MoveBall(float elapsed)
        {
            Ball.Direction.Normalize();
            Ball.Speed += Ball.Acceleration * elapsed;
            Ball.Direction *= Ball.Speed * elapsed;
            Bounce(Boundaries);

            if (Ball.Position.Y > BoundaryCoordinates.Y + Ball.Size.Y)
                GameOver();

            if (Bricks.Where(b => b.Kind != Brick.Kinds.Indestructible).Count() == 0)
                NextLevel();
        }

        private void MovePowerUps(float elapsed)
        {
            if (PowerUps.Count > 0)
            {
                PowerUp[] copy = new PowerUp[PowerUps.Count];
                PowerUps.CopyTo(copy);
                foreach (PowerUp powerUp in copy)
                {
                    PowerUp.Kinds? kind = powerUp.Move(Bat, elapsed);
                    if (kind.HasValue == false)
                        continue;
                    switch(kind.GetValueOrDefault())
                    {
                        case PowerUp.Kinds.LavaBall:
                            Ball.Kind = Ball.Kinds.Lava;
                            break;
                        case PowerUp.Kinds.SlowerBat:
                            Bat.Kind = Bat.Kinds.Slow;
                            break;
                        case PowerUp.Kinds.FasterBat:
                            Bat.Kind = Bat.Kinds.Fast;
                            break;
                        case PowerUp.Kinds.InvertedBat:
                            Bat.Kind = Bat.Kinds.Inverted;
                            break;
                        case PowerUp.Kinds.StickyBat:
                            Bat.Kind = Bat.Kinds.Sticky;
                            break;
                        case PowerUp.Kinds.SmallerBat:
                            Bat.Size.X = Math.Max(Bat.Size.X / 2, 20);
                            break;
                        case PowerUp.Kinds.BiggerBat:
                            Bat.Size.X = Math.Min(Bat.Size.X * 2, 320);
                            break;
                        case PowerUp.Kinds.SlowerBall:
                            Ball.SpeedMultiplier /= 2;
                            break;
                        case PowerUp.Kinds.FasterBall:
                            Ball.SpeedMultiplier *= 2;
                            break;
                        case PowerUp.Kinds.SmallerBall:
                            float size1 = Ball.Size.X;
                            size1 = (float)Math.Max(size1 / 2, 2.5);
                            Ball.Size = new Vector2(size1, size1);
                            break;
                        case PowerUp.Kinds.BiggerBall:
                            float size2 = Ball.Size.X;
                            size2 = (float)Math.Min(size2 * 2, 40);
                            Ball.Size = new Vector2(size2, size2);
                            break;
                        case PowerUp.Kinds.ExtraLive:
                            ;
                            break;
                    }
                }
            }
        }

        private void Bounce(List<Tuple<LineSegment, Brick>> boundaries, Tuple<LineSegment, Brick> removedElement = null)
        {
            IntersectionPoint intersectionPoint = CalculateIntersection(boundaries);
            if (intersectionPoint == null)
                return;

            Tuple<LineSegment, Brick> boundary = boundaries[intersectionPoint.Index1];
            //remove the element the ball bounced on
            boundaries.RemoveAt(intersectionPoint.Index1);

            if (Ball.Kind != Ball.Kinds.Lava || intersectionPoint.Brick1 == null)
                Reflect(intersectionPoint, boundary);

            //delete the brick in case ball bounced off of one
            if (intersectionPoint.Brick1 != null)
            {
                if (RemoveBrickAndCheckPowerUp(intersectionPoint.Brick1, boundaries))
                    boundary = null;
                if (intersectionPoint.Brick2 != null)
                {
                    RemoveBrickAndCheckPowerUp(intersectionPoint.Brick2, boundaries);
                }
            }

            //ball landed directly on border, no further checks needed
            if (intersectionPoint.Distance == 0)
                return;

            //add removed element from last iteration in case ball would bounce twice from one wall during one movement
            if (removedElement != null)
                boundaries.Add(removedElement);

            Ball.Direction *= intersectionPoint.Distance;
            Bounce(boundaries, boundary);
        }

        //TODO: removes brickborders even though the brick is still there
        private bool RemoveBrickAndCheckPowerUp(Brick brick, List<Tuple<LineSegment, Brick>> boundaries)
        {
            bool dropPowerUp;
            bool isDestroyed = brick.Hit(out dropPowerUp, Ball.Kind == Ball.Kinds.Lava);
            if (dropPowerUp)
                PowerUps.Add(new PowerUp(brick.Position.X, brick.Position.Y, PowerUps));
            if (isDestroyed)
            {
                boundaries.RemoveAll(t => ReferenceEquals(t.Item2, brick));
                Score += (int)Math.Pow(((int)brick.Kind + 1) * 10, 2);
                return true;
            }
            return false;
        }

        private void Reflect(IntersectionPoint intersectionPoint, Tuple<LineSegment, Brick> boundary)
        {
            //only reflect when ball wasnt previously exactly on a wall
            if (intersectionPoint.Distance != Ball.Direction.Length())
            {
                if (intersectionPoint.Index1 == 0)//TODO: proper check for bounce on bat
                {
                    Ball.Direction = Utils.ReflectCircular(intersectionPoint.Point.Value, Bat, Ball);
                    if (Bat.Kind == Bat.Kinds.Sticky)
                        Ball.SpeedMultiplier = 0;
                }
                else
                    Ball.Direction = Utils.Reflect(Ball.Direction, boundary.Item1.Direction);

                Ball.Position = intersectionPoint.Point.Value;
                Ball.Direction = Utils.RandomizeVector(Ball.Direction, RandomBounceMarginPercentage);
            }
        }

        private IntersectionPoint CalculateIntersection(List<Tuple<LineSegment, Brick>> boundaries)
        {
            Vector2 newPosition = Ball.Position + Ball.Direction;

            //perform collision checks and give out closest border
            var intersections = boundaries
                .Select((l, i) =>
                {
                    float distance;
                    Vector2? point = Utils.GetIntersectionPoint(new LineSegment() { StartPosition = newPosition, EndPosition = Ball.Position }, l.Item1, out distance);
                    return new IntersectionPoint(distance, point, i, l.Item2);
                })
                .Where(i => i.Point.HasValue == true)
                .OrderByDescending(i => i.Distance)
                .GroupBy(i => i.Brick1)
                .Select(g => g.First())
                .ToArray();

            //no intersection
            if (intersections.Length < 1)
            {
                Ball.Position = newPosition;
                return null;
            }

            IntersectionPoint firstIntersection = intersections[0];

            if (intersections.Length == 1)
            {
                return firstIntersection;
            }

            IntersectionPoint secondIntersection = intersections[1];

            if (firstIntersection.Distance == secondIntersection.Distance)
            {
                firstIntersection.Index2 = secondIntersection.Index1;
                firstIntersection.Brick2 = secondIntersection.Brick1;
            }
            return firstIntersection;
        }

        public void Dispose()
        {
            Tileset.Dispose();
            Keyboard.Dispose();
            DirectInput.Dispose();
            DebugBrush.Dispose();
        }

        private class IntersectionPoint
        {
            public float Distance;
            public Vector2? Point;
            public int Index1;
            public int Index2;
            public Brick Brick1;
            public Brick Brick2;

            public IntersectionPoint(float distance, Vector2? point, int index1, Brick brick1, int index2 = -1, Brick brick2 = null)
            {
                Distance = distance;
                Point = point;
                Index1 = index1;
                Index2 = index2;
                Brick1 = brick1;
                Brick2 = brick2;
            }
        }
    }
}