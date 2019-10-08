using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = SharpDX.Direct3D11.Device;
using Factory = SharpDX.DXGI.Factory;
using D2DFactory = SharpDX.Direct2D1.Factory;

namespace Breakout
{
    static class Program
    {
        private const string FormCaption = "Breakout";
        private const int Width = 800;
        private const int Height = 600;
        private const int Fps = 60;
        private const bool Fullscreen = false;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (RenderForm form = new RenderForm(FormCaption))
            using (D2DFactory d2dFactory = new D2DFactory())
            {
                form.ClientSize = new System.Drawing.Size(Width, Height);

                HwndRenderTargetProperties wtp = new HwndRenderTargetProperties();
                wtp.Hwnd = form.Handle;
                wtp.PixelSize = new Size2(Width, Height);
                wtp.PresentOptions = PresentOptions.Immediately;
                using (WindowRenderTarget renderTarget = new WindowRenderTarget(d2dFactory, new RenderTargetProperties(), wtp))
                {
                    RunGame(form, renderTarget);
                }
            }
        }

        static void RunGame(RenderForm form, RenderTarget renderTarget)
        {
            using (Game game = new Game(Width, Height))
            {
                game.LoadResources(renderTarget);
                Stopwatch gameTime = Stopwatch.StartNew();
                TimeSpan lastUpdate = TimeSpan.Zero;
                RenderLoop.Run(form, () =>
                {
                    TimeSpan totalTime = gameTime.Elapsed;
                    TimeSpan elapsed = totalTime - lastUpdate;
                    lastUpdate = totalTime;

                    game.Update((float)elapsed.TotalSeconds);
                    game.DrawScene(renderTarget);
                });
            }
        }
    }
}
