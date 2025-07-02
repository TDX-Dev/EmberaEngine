using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace EmberaEngine.Engine.Core
{
    public struct WindowSpecification
    {
        public string Name;
        public bool VSync;
        public int Width, Height;
        public bool useFullscreen;

        public bool customTitlebar;
        public bool darkTitlebar;
    }

    public enum WindowState_
    {
        Normal,
        Minimized,
        Maximized,
        Fullscreen
    }

    public class Window : GameWindow
    {
        WindowSpecification specification;

        public Window(WindowSpecification specification) : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            API = ContextAPI.OpenGL,
            APIVersion = new Version(4, 6),
            Size = new OpenTK.Mathematics.Vector2i(specification.Width, specification.Height),
            Title = specification.Name,
            StartVisible = false,
            Flags = ContextFlags.Debug
            
        })
        {
            this.specification = specification;

            base.VSync = specification.VSync ? VSyncMode.On : VSyncMode.Off;
            this.WindowBorder = specification.customTitlebar ? WindowBorder.Hidden : this.WindowBorder;
            this.CenterWindow();

            unsafe
            {
                if (specification.darkTitlebar) WindowUtil.EnableDarkMode(WindowUtil.GetWin32Window(this));
            }
        }

        public void setWindowState(WindowState_ windowState)
        {
            WindowState = (OpenTK.Windowing.Common.WindowState)windowState;
        }

        public void Create()
        {

        }

        protected override void OnLoad()
        {
            this.IsVisible = true;

            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            SwapBuffers();
            base.OnRenderFrame(args);
        }
    }
}
