using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{

    public class Renderer
    {
        public static int Height { get; private set; }
        public static int Width { get; private set; }

        public static void Initialize(int width, int height)
        {
            Height = height;
            Width = width;

            Renderer2D.Initialize(width, height);
        }

        public static void BeginFrame()
        {
            GraphicsState.Clear(true, true);
            //GraphicsState.ErrorCheck();
            Renderer2D.BeginRender();
        }

        public static void RenderFrame()
        {
            Renderer2D.Render();
        }

        public static void EndFrame()
        {
            Renderer2D.EndRender();
        }

        public static void Resize(int width, int height)
        {
            Height = height;
            Width = width;
            Renderer2D.Resize(width, height);
        }

        public static Framebuffer GetCompositeBuffer()
        {
            return Renderer2D.GetComposite2D();
        }


    }
}
