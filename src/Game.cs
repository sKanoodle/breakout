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
    class Game: IDisposable
    {
        DirectInput DirectInput;
        Keyboard Keyboard;
        Bitmap Tileset;
        Color4 BackgroundColor;

        Vector2 BallPosition;
        Vector2 BallSize;
        float BallSpeed;

        RawRectangleF BallSprite = new RawRectangleF(48, 136, 56, 144);

        public Game(int width, int height)
        {
            BallPosition = new Vector2(width / 2f, height / 2f);
            BallSize = new Vector2(20, 20);
            BallSpeed = 100; // Pixel/Second
        }

        public void LoadResources(RenderTarget render)
        {
            BackgroundColor = Color.CornflowerBlue;
            Tileset = Resources.LoadImageFromFile(render, "breakout_pieces.png");

            DirectInput = new DirectInput();
            Keyboard = new Keyboard(DirectInput);
            Keyboard.Acquire();
        }

        public void DrawScene(RenderTarget render)
        {
            render.BeginDraw();
            render.Clear(BackgroundColor);
            RectangleF destination = new RectangleF(BallPosition.X, BallPosition.Y, BallSize.X, BallSize.Y);
            render.DrawBitmap(Tileset, destination, 1, BitmapInterpolationMode.Linear, BallSprite);
            render.EndDraw();
        }

        public void Update(float elapsed)
        {
            Vector2 ballDirection = Vector2.Zero;
            KeyboardState keyboard = Keyboard.GetCurrentState();

            if (keyboard.IsPressed(Key.Up))
                ballDirection.Y += -1;
            if (keyboard.IsPressed(Key.Down))
                ballDirection.Y += +1;
            if (keyboard.IsPressed(Key.Left))
                ballDirection.X += -1;
            if (keyboard.IsPressed(Key.Right))
                ballDirection.X += +1;
            ballDirection.Normalize();
            BallPosition += ballDirection * BallSpeed * elapsed;
        }

        public void Dispose()
        {
            Tileset.Dispose();
            Keyboard.Dispose();
            DirectInput.Dispose();
        }
    }
}
