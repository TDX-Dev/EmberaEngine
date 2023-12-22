﻿using EmberaEngine.Engine.Imgui;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.Core
{

    public struct ApplicationSpecification
    {
        public string Name;
        public int Width, Height;
        public bool forceVsync;
        public bool useImGui;
    }

    public class Application
    {
        public Window window;
        public LayerHandler LayerHandler;
        public ImguiLayer ImGuiLayer;
        public ApplicationSpecification appSpec;

        public void Create(ApplicationSpecification appSpec)
        {
            WindowSpecification windowSpec = new WindowSpecification()
            {
                Name = appSpec.Name,
                Width = appSpec.Width,
                Height = appSpec.Height,
                VSync = appSpec.forceVsync
            };

            LayerHandler = new LayerHandler();

            if (appSpec.useImGui)
            {
                ImGuiLayer = new ImguiLayer();
                LayerHandler.AddLayer(ImGuiLayer);
            }

            window = new Window(windowSpec);

            window.Load += OnLoad;
            window.UpdateFrame += OnUpdateFrame;
            window.RenderFrame += OnRenderFrame;
            window.KeyDown += OnKeyDown;
            window.Resize += OnResize;
            window.TextInput += OnTextInput;
            window.MouseMove += OnMouseMove;

            Renderer.Initialize(appSpec.Width, appSpec.Height);

            this.appSpec = appSpec;
        }

        private void OnMouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs obj)
        {
            Input.SetMousePos(obj.Position);
        }

        public ImguiAPI? GetImGuiAPI()
        {
            if (appSpec.useImGui)
            {
                return ImGuiLayer.imguiAPI;
            }
            return null;
        }

        private void OnTextInput(OpenTK.Windowing.Common.TextInputEventArgs obj)
        {
            LayerHandler.TextInput(obj.Unicode);
        }

        private void OnResize(OpenTK.Windowing.Common.ResizeEventArgs obj)
        {
            Renderer.Resize(obj.Width, obj.Height);
            LayerHandler.ResizeLayers(obj.Width, obj.Height);
            Screen.Size = new OpenTK.Mathematics.Vector2i(obj.Width, obj.Height);
        }

        private void OnLoad()
        {
            if (appSpec.useImGui)
            {
                ImGuiLayer.InitIMGUI(window, appSpec.Width, appSpec.Height);
            }

            LayerHandler.AttachLayers();
        }

        private void OnKeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs obj)
        {
            bool caps = obj.Modifiers.ToString().Split(",").Contains("CapsLock");
            LayerHandler.KeyDownInput((Keys)(int)obj.Key, obj.ScanCode, obj.Modifiers.ToString(), caps);
        }

        public void Run()
        {
            window.Run();
        }

        public void AddLayer(Layer layer)
        {
            LayerHandler.AddLayer(layer);
        }

        private void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            LayerHandler.RenderLayers();

            Renderer.BeginFrame();
            Renderer.RenderFrame();
            Renderer.EndFrame();
        }

        private void OnUpdateFrame(OpenTK.Windowing.Common.FrameEventArgs obj)
        {
            if (appSpec.useImGui)
            {
                ImGuiLayer.Begin((float)obj.Time);
                for (int i = 0; i < LayerHandler.layers.Count; i++)
                {
                    LayerHandler.layers[i].OnGUIRender();
                }

                ImGuiLayer.End();
            }

            bool left = window.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            bool right = window.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right);
            bool middle = window.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle);

            Input.mouseButton = left ? MouseButtonEvent.Left : right ? MouseButtonEvent.Right : middle ? MouseButtonEvent.Middle : MouseButtonEvent.None;

            // Updating

            LayerHandler.UpdateLayers((float)obj.Time);
        }
    }
}
