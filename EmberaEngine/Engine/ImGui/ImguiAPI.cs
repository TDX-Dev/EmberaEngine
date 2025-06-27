using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System.Runtime.InteropServices;
using MaterialIconFont;
using SkiaSharp;

namespace EmberaEngine.Engine.Imgui
{
    /// <summary>
    /// A modified version of Veldrid.ImGui's ImGuiRenderer.
    /// Manages input for ImGui and handles rendering ImGui's DrawLists with Veldrid.
    /// </summary>
    public class ImguiAPI : IDisposable
    {
        private bool _frameBegun;

        // Veldrid objects
        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        public int windowWidth, windowHeight;

        public ImFontPtr _CurrentFont;
        public static ImFontPtr ICONFONT;
        private Texture _fontTexture;
        private Shader _guiShader;
        private GameWindow _GameWindow;
        private int FontCount;
        private static float dpi_scaling = 1f;
        Matrix4 mvp;
        uint DockspaceID;
        

        private System.Numerics.Vector2 Previous_Size = new System.Numerics.Vector2();

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImguiAPI(GameWindow gameWindow, int width, int height)
        {

            windowWidth = width;
            windowHeight = height;

            _GameWindow = gameWindow;
            var io = ImGui.GetIO();

            dpi_scaling = Monitors.GetMonitorFromWindow(gameWindow).HorizontalDpi;

            FontCount = io.Fonts.Fonts.Size;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            
            RecreateFontDeviceTexture();

            ImGui.StyleColorsDark();

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

        }

        public static unsafe ImFontPtr LoadIconFont(string path, int size, (ushort, ushort) range)
        {
            ImFontConfigPtr configuration = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());

            configuration.GlyphOffset = new System.Numerics.Vector2(0, 6);
            configuration.GlyphMinAdvanceX = size;
            configuration.MergeMode = true;
            configuration.PixelSnapH = true;

            GCHandle rangeHandle = GCHandle.Alloc(new ushort[]
            {
                range.Item1,
                range.Item2,
        0
            }, GCHandleType.Pinned);

            try
            {
                return ImGui.GetIO().Fonts.AddFontFromFileTTF(path, (float)size, configuration, rangeHandle.AddrOfPinnedObject()); ;
            }
            finally
            {
                configuration.Destroy();

                if (rangeHandle.IsAllocated)
                {
                    rangeHandle.Free();
                }
            }

        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            ImguiUtils.CreateVertexArray("ImGui", out _vertexArray);

            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            ImguiUtils.CreateVertexBuffer("ImGui", out _vertexBuffer);
            ImguiUtils.CreateElementBuffer("ImGui", out _indexBuffer);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _indexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // If using opengl 4.5 this could be a better way of doing it so that we are not modifying the bound buffers
            // GL.NamedBufferData(_vertexBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            // GL.NamedBufferData(_indexBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();
            _guiShader = new Shader("Engine/Imgui/shaders/GUI");

            GL.BindVertexArray(_vertexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);

            int stride = Unsafe.SizeOf<ImDrawVert>();

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // We don't need to unbind the element buffer as that is connected to the vertex array
            // And you should not touch the element buffer when there is no vertex array bound.

            //ImguiUtils.CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();


            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            _fontTexture = new Texture("ImGui Text Atlas", width, height, pixels);
            _fontTexture.SetMagFilter(OpenTK.Graphics.OpenGL.TextureMagFilter.Linear);
            _fontTexture.SetMinFilter(OpenTK.Graphics.OpenGL.TextureMinFilter.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture.GLTexture);

            //io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// This method requires a <see cref="GraphicsDevice"/> because it may create new DeviceBuffers if the size of vertex
        /// or index data has increased beyond the capacity of the existing buffers.
        /// A <see cref="CommandList"/> is needed to submit drawing and resource update commands.
        /// </summary>
        public void Render()
        {
            ImGui.PopFont();
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();

                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        float elapsedTime = 0f;

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput();
            _frameBegun = true;
            
            ImGui.NewFrame();
            ImGui.PushFont(_CurrentFont);

            HandleCursorWrap(_GameWindow);


            elapsedTime += deltaSeconds;

            if (elapsedTime > 10f)
            {
                //ImGui.SaveIniSettingsToDisk(AppContext.BaseDirectory + "Engine/Imgui/misc/imgui.ini");
                elapsedTime = 0;
            }

        }


        // Store these between frames:
        Vector2 _lastMouseScreen;
        bool _justWarped = false;

        void HandleCursorWrap(NativeWindow window)
        {
            return;
            var io = ImGui.GetIO();
            Vector2 screenSize = new(window.Size.X, window.Size.Y);
            Vector2 screenPoint = window.MousePosition;
            Vector2 mouseDelta = screenPoint - _lastMouseScreen;

            Vector2 newScreenPoint = screenPoint;
            bool wrapped = false;
            const float margin = 2.0f;

            if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            {
                // Check if we need to wrap
                if (screenPoint.X <= margin)
                {
                    newScreenPoint.X = screenSize.X - margin - 1;
                    wrapped = true;
                }
                else if (screenPoint.X >= screenSize.X - margin)
                {
                    newScreenPoint.X = margin + 1;
                    wrapped = true;
                }

                if (screenPoint.Y <= margin)
                {
                    newScreenPoint.Y = screenSize.Y - margin - 1;
                    wrapped = true;
                }
                else if (screenPoint.Y >= screenSize.Y - margin)
                {
                    newScreenPoint.Y = margin + 1;
                    wrapped = true;
                }

                if (wrapped)
                {
                    // Warp cursor
                    window.MousePosition = new OpenTK.Mathematics.Vector2i((int)newScreenPoint.X, (int)newScreenPoint.Y);

                    // Update ImGui mouse position using scaled value
                    io.MousePos = new System.Numerics.Vector2(
                        newScreenPoint.X / _scaleFactor.X,
                        newScreenPoint.Y / _scaleFactor.Y
                    );

                    // Manually apply the delta from this frame BEFORE warp
                    io.MouseDelta = new System.Numerics.Vector2(mouseDelta.X / _scaleFactor.X, mouseDelta.Y / _scaleFactor.Y);

                    // Prevent delta from spiking on next frame
                    _justWarped = true;
                    _lastMouseScreen = newScreenPoint;
                    return;
                }
            }

            // Handle normal input or post-warp reset
            io.MousePos = new System.Numerics.Vector2(screenPoint.X / _scaleFactor.X, screenPoint.Y / _scaleFactor.Y);

            if (_justWarped)
            {
                io.MouseDelta = System.Numerics.Vector2.Zero;
                _justWarped = false;
            }
            else
            {
                io.MouseDelta = new System.Numerics.Vector2(mouseDelta.X / _scaleFactor.X, mouseDelta.Y / _scaleFactor.Y);
            }

            _lastMouseScreen = screenPoint;
        }




        //int[] param = new int[];

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
            if (io.Fonts.Fonts.Size != FontCount)
            {
                //Console.WriteLine("Remaking Atlas, Font Count " + io.Fonts.Fonts.Size);
                RecreateFontDeviceTexture();
                FontCount = io.Fonts.Fonts.Size;
            }
        }

        public void OnResize(int width, int height)
        {

            windowWidth = width;
            windowHeight = height;

            ImGuiIOPtr io = ImGui.GetIO();

            io.DisplaySize = new System.Numerics.Vector2(
                    (float)windowWidth / _scaleFactor.X,
                    (float)windowHeight / _scaleFactor.Y
                );

            mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f
            );
        }

        readonly List<char> pressedChars = new List<char>();

        private void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = _GameWindow.MouseState.GetSnapshot();
            KeyboardState KeyboardState = _GameWindow.KeyboardState.GetSnapshot();

            io.MouseDown[0] = MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left);
            io.MouseDown[1] = MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Right);
            io.MouseDown[2] = MouseState.IsButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Middle);

            var screenPoint = MouseState.Position;
            io.MousePos = new System.Numerics.Vector2(screenPoint.X / _scaleFactor.X, screenPoint.Y / _scaleFactor.Y);

            io.MouseWheel = MouseState.Scroll.Y - MouseState.PreviousScroll.Y;
            io.MouseWheelH = MouseState.Scroll.X - MouseState.PreviousScroll.X;


            foreach (var key in Enum.GetValues<OpenTK.Windowing.GraphicsLibraryFramework.Keys>())
            {
                var imguiKey = ImguiUtils.ConvertToImGuiKey(key);
                if (imguiKey != ImGuiKey.None)
                {
                    ImGui.GetIO().AddKeyEvent(imguiKey, KeyboardState.IsKeyDown(key));
                }
            }


            for (int i = 0; i < pressedChars.Count; i++)
            {
                io.AddInputCharacter(pressedChars[i]);
            }

            io.AddKeyEvent(ImGuiKey.ModCtrl, KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl));
            io.AddKeyEvent(ImGuiKey.ModShift, KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift));
            io.AddKeyEvent(ImGuiKey.ModAlt, KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt));
            io.AddKeyEvent(ImGuiKey.ModSuper, KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftSuper) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightSuper));


            pressedChars.Clear();

            //io.KeyCtrl = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftControl) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightControl);
            //io.KeyAlt = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftAlt) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightAlt);
            //io.KeyShift = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftShift) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightShift);
            //io.KeySuper = KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftSuper) || KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightSuper);
        }



        private char KeyToChar(OpenTK.Windowing.GraphicsLibraryFramework.Keys e, bool shift = false)
        {
            var str = e.ToString();

            if (str.Length == 0) { return ' '; }

            if (str.Length == 1)
            {
                return shift ? str[0] : str.ToLower()[0];
            }


            else if ((str.StartsWith("D") || str.StartsWith( "KeyPad")) && (str.Length == 7 || str.Length == 2))
            {
                return str[str.Length - 1];
            }


            switch (e)
            {
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Backslash:
                    return '\\';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Slash:
                    return '/';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.LeftBracket:
                    return '(';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.RightBracket:
                    return ')';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Comma:
                    return ',';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Space:
                    return ' ';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.GraveAccent:
                    return '`';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.Equal:
                    return '=';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadEqual:
                    return '=';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadAdd:
                    return '+';
                case OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadDecimal:
                    return '.';
            }

            return '\0';
        }


        public void PressChar(char keyChar)
        {
            pressedChars.Add(keyChar);
        }

        private static void SetKeyMappings()
        {
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            //ImGui.GetStyle().ScaleAllSizes(dpi_scaling);

            GL.Viewport(0, 0, _GameWindow.Size.X, _GameWindow.Size.Y);
            // Get intial state.
            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
            Span<int> prevScissorBox = stackalloc int[4];
            unsafe
            {
                fixed (int* iptr = &prevScissorBox[0])
                {
                    GL.GetInteger(GetPName.ScissorBox, iptr);
                }
            }
            Span<int> prevPolygonMode = stackalloc int[2];
            unsafe
            {
                fixed (int* iptr = &prevPolygonMode[0])
                {
                    GL.GetInteger(GetPName.PolygonMode, iptr);
                }
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(_vertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    //Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    //Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
            1.0f);

            _guiShader.Use();
            _guiShader.SetMatrix4("projection_matrix", mvp, false);
            _guiShader.SetFloat("dpi_scaling", dpi_scaling);
            _guiShader.SetInt("in_fontTexture", 0);

            GL.BindVertexArray(_vertexArray);

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[n];

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _GameWindow.Size.Y - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            // Reset state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((OpenTK.Graphics.OpenGL.TextureUnit)prevActiveTexture);
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVAO);
            GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
            GL.BlendFuncSeparate(
                (BlendingFactorSrc)prevBlendFuncSrcRgb,
                (BlendingFactorDest)prevBlendFuncDstRgb,
                (BlendingFactorSrc)prevBlendFuncSrcAlpha,
                (BlendingFactorDest)prevBlendFuncDstAlpha);
            if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);
        }

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus /*| ImGuiWindowFlags.NoBackground*/ | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.DockNodeHost;
        bool firstFrame = true;

        public void SetUpDockspace(bool customTitlebar = false, Core.Texture titlebarLogo = null)
        {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.Pos);
            ImGui.SetNextWindowSize(viewport.Size);
            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);

            ImGuiWindowFlags windowFlags =
                ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoNavFocus;

            ImGui.Begin("Dockspace", windowFlags);

            float titlebarHeight = 90.0f;

            if (customTitlebar)
            {
                ImGui.SetCursorScreenPos(viewport.Pos);
                ImGui.BeginChild("Titlebar", new System.Numerics.Vector2(viewport.Size.X, titlebarHeight),
                    ImGuiChildFlags.AlwaysAutoResize,
                    ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar |
                    ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDecoration);

                var min = viewport.Pos;
                var max = new System.Numerics.Vector2(viewport.Size.X, titlebarHeight);
                var mouse = _GameWindow.MouseState.Position;

                bool titlebarHovered = ImGui.IsMouseHoveringRect(min, max);

                if ((titlebarHovered && ImGui.IsMouseDown(ImGuiMouseButton.Left)) || _dragging)
                {
                    if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                    {
                        _dragging = false;
                    }
                    else if (!_dragging)
                    {
                        _dragging = true;
                        _lastMouseScreen1 = mouse;
                    }
                    else
                    {
                        var deltaX = mouse.X - _lastMouseScreen1.X;
                        var deltaY = mouse.Y - _lastMouseScreen1.Y;

                        var currentPos = _GameWindow.Location;
                        _GameWindow.Location = new OpenTK.Mathematics.Vector2i(
                            currentPos.X + (int)deltaX,
                            currentPos.Y + (int)deltaY
                        );

                        _lastMouseScreen = mouse;
                    }
                }
                else
                {
                    _dragging = false;
                }

                // Logo
                ImGui.SetCursorPos(new System.Numerics.Vector2(10, 10));
                ImGui.Image(titlebarLogo.GetRendererID(), new System.Numerics.Vector2(160, 61));

                // Window control buttons
                var region = ImGui.GetContentRegionAvail();
                ImGui.SetCursorPos(new System.Numerics.Vector2(region.X - 130, 10));
                if (ImGui.Button(MaterialDesign.Minimize))
                {
                    _GameWindow.WindowState = OpenTK.Windowing.Common.WindowState.Minimized;
                }

                ImGui.SameLine();
                if (ImGui.Button(_GameWindow.WindowState == OpenTK.Windowing.Common.WindowState.Maximized
                    ? MaterialDesign.Fullscreen_exit : MaterialDesign.Fullscreen))
                {
                    _GameWindow.WindowState = _GameWindow.WindowState == OpenTK.Windowing.Common.WindowState.Maximized
                        ? OpenTK.Windowing.Common.WindowState.Normal
                        : OpenTK.Windowing.Common.WindowState.Maximized;
                }

                ImGui.SameLine();
                if (ImGui.Button(MaterialDesign.Close))
                {
                    _GameWindow.Close();
                }

                ImGui.EndChild();

                // Push cursor below the titlebar
                ImGui.SetCursorScreenPos(new System.Numerics.Vector2(viewport.Pos.X, viewport.Pos.Y + titlebarHeight));
            }

            // Initialize dockspace ID only once
            if (firstFrame)
            {
                DockspaceID = ImGui.GetID("MainDockspace");
                firstFrame = false;
            }

            // The actual dockspace
            ImGui.DockSpace(DockspaceID, System.Numerics.Vector2.Zero);

            ImGui.End();
            ImGui.PopStyleVar(2); // Padding + Rounding
        }

        // internal state
        bool _dragging = false;
        Vector2 _lastMouseScreen1;


        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _fontTexture.Dispose();
            _guiShader.Dispose();
        }
    }
}

