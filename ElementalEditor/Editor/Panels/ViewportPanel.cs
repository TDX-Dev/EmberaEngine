using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using MaterialIconFont;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Windowing.Desktop;
using ElementalEditor.Editor.Utils;

namespace ElementalEditor.Editor.Panels
{

    class ViewportPanel : Panel
    {
        struct SupportedResolution
        {
            public int height, width;
            public int refreshRate;
        }

        List<SupportedResolution> SupportedResolutions = new List<SupportedResolution>()
        {
            new SupportedResolution()
            {
                height = 16,
                width = 16,
                refreshRate = 60
            },
            new SupportedResolution()
            {
                height = 128,
                width = 128,
                refreshRate = 60
            },
            new SupportedResolution()
            {
                height = 512,
                width = 512,
                refreshRate = 60
            },
            new SupportedResolution()
            {
                height = 600,
                width = 800,
                refreshRate = 60
            }

        };
        SupportedResolution selectedResolution;
        bool freeAspectRatio = false;
        (int, int, int, int) resizeCoords;

        Texture viewportBufferTexture;
        Framebuffer viewportBuffer;
        Framebuffer compositeBuffer;

        int prevViewportHeight, prevViewportWidth;
        int viewportHeight, viewportWidth;
        Vector2 viewportPos;

        public override void OnAttach()
        {
            compositeBuffer = Renderer2D.GetComposite2D();

            viewportBufferTexture = new Texture(TextureTarget2d.Texture2D);
            viewportBufferTexture.TexImage2D(viewportWidth, viewportHeight, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            viewportBufferTexture.GenerateMipmap();

            viewportBuffer = new Framebuffer();
            viewportBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, viewportBufferTexture);

            // FIX THIS MF, THIS ONLY ACCOUNTS FOR THE FIRST MONITOR!!!
            MonitorInfo monitorInfoList = Monitors.GetMonitors()[0];

            for (int i = 0; i < monitorInfoList.SupportedVideoModes.Count; i++)
            {
                SupportedResolutions.Add(new SupportedResolution()
                {
                    width = monitorInfoList.SupportedVideoModes[i].Width,
                    height = monitorInfoList.SupportedVideoModes[i].Height,
                    refreshRate = monitorInfoList.SupportedVideoModes[i].RefreshRate
                });
            }

            selectedResolution = SupportedResolutions[0];
        }

        public override void OnGUI()
        {

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0, 0));
            ImGui.Begin(MaterialDesign.Landscape + " Viewport", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            viewportHeight = (int)ImGui.GetContentRegionMax().Y;
            viewportWidth = (int)ImGui.GetContentRegionMax().X;

            if (prevViewportHeight != viewportHeight || prevViewportWidth != viewportWidth)
            {
                prevViewportHeight = viewportHeight;
                prevViewportWidth = viewportWidth;

                viewportBufferTexture.TexImage2D(viewportWidth, viewportHeight, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
                viewportBufferTexture.GenerateMipmap();

                if (!freeAspectRatio)
                    Renderer.Resize(selectedResolution.width, selectedResolution.height);
            }

            resizeCoords = CalculateScaleWithScreen(viewportWidth,viewportHeight,selectedResolution.width,selectedResolution.height);
            if (!freeAspectRatio)
            {
                Framebuffer.BlitFrameBuffer(compositeBuffer, viewportBuffer, (0, 0, selectedResolution.width, selectedResolution.height), (resizeCoords.Item1, resizeCoords.Item2, resizeCoords.Item3, resizeCoords.Item4), OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);
            }

            viewportPos = ImGui.GetWindowPos();

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new System.Numerics.Vector2(0, 0));
            ImGui.Image(freeAspectRatio ? (IntPtr)compositeBuffer.GetFramebufferTexture(0).GetRendererID() : (IntPtr)viewportBufferTexture.GetRendererID(), new System.Numerics.Vector2(ImGui.GetContentRegionMax().X, ImGui.GetContentRegionMax().Y - 32), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(1, -1));


            ImGui.PopStyleVar(2);

            ImGui.SetItemAllowOverlap();

            DrawViewportTools();

            DrawGuizmo2D();



            ImGui.End();

            ImGui.PopStyleVar();
        }




        public (int, int, int, int) CalculateScaleWithScreen(int viewportWidth, int viewportHeight, int resolutionWidth, int resolutionHeight)
        {

            // Calculate the scaling factors for width and height
            float scaleWidth = (float)viewportWidth / resolutionWidth;
            float scaleHeight = (float)viewportHeight / resolutionHeight;

            // Choose the smaller scaling factor to maintain aspect ratio
            float scaleFactor = Math.Min(scaleWidth, scaleHeight);

            // Apply the scaling factor
            float scaledWidth = resolutionWidth * scaleFactor;
            float scaledHeight = resolutionHeight * scaleFactor;

            // Calculate centering offsets
            int left = (int)((viewportWidth - scaledWidth) / 2);
            int top = (int)((viewportHeight - scaledHeight) / 2);
            int right = left + (int)scaledWidth;
            int bottom = top + (int)scaledHeight;

            // Return the scaled resolution and offsets
            return (left, top, right, bottom);
        }





        static float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            // First, normalize the value to the range [0, 1] within the source range
            float normalizedValue = ((value - fromMin) * (toMax - toMin)) / (fromMax - fromMin) + toMin;

            return normalizedValue;

        }

        public override void OnMouseMove(MouseMoveEvent moveEvent)
        {
            if (editor.EditorCurrentScene.IsPlaying)
            {
                if (freeAspectRatio)
                {
                    moveEvent.position.X = (int)MapValue(moveEvent.position.X - viewportPos.X, 0, editor.app.window.Size.X, 0, viewportWidth);
                    moveEvent.position.Y = (int)MapValue(moveEvent.position.Y - (viewportPos.Y + 46), 0, editor.app.window.Size.Y, 0, viewportHeight);
                } else
                {
                    moveEvent.position.X = (int)MapValue(moveEvent.position.X/*  - viewportPos.X - resizeCoords.Item1 */, viewportPos.X + resizeCoords.Item1, viewportPos.X + resizeCoords.Item3, 0, selectedResolution.width);
                    moveEvent.position.Y = (int)MapValue(moveEvent.position.Y, (viewportPos.Y + 54) + resizeCoords.Item2, viewportPos.Y + 54 + resizeCoords.Item4, 0, selectedResolution.height);
                }

                if (moveEvent.position.X < 0)
                    moveEvent.position.X = 0;
                if (moveEvent.position.Y < 0)
                    moveEvent.position.Y = 0;

                //Console.WriteLine(moveEvent.position);

                editor.EditorCurrentScene.GameObjects[1].GetComponent<TextComponent>().Content = moveEvent.position.ToString();

                Input.OnMouseMove(moveEvent);
            }
        }

        public override void OnMouseButton(MouseButtonEvent buttonEvent)
        {
            if (editor.EditorCurrentScene.IsPlaying)
            {

            }
        }

        public override void OnRender()
        {
            if (freeAspectRatio)
            {
                Renderer.Resize(viewportWidth, viewportHeight);
                Screen.Size.X = viewportWidth;
                Screen.Size.Y = viewportHeight;
            }
                
            CanvasManager.ResizeAllCanvases(viewportWidth, viewportHeight);
        }

        public void DrawViewportTools()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
            float size = 45;

            ImGui.SetCursorPosX(10);
            ImGui.SetCursorPosY(size + 15);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(7, 7));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, new System.Numerics.Vector4(0, 0, 0, 0));

            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(7, 7));
            ImGui.BeginChild("##VIEW_TOOLS", new System.Numerics.Vector2(250, 48), true, ImGuiWindowFlags.AlwaysUseWindowPadding);


            if (ImGui.BeginCombo("##supportedResolutions", freeAspectRatio ? "Free Aspect" : selectedResolution.width + "x" + selectedResolution.height + " @" + selectedResolution.refreshRate + "Hz"))
            {

                for (int i = 0; i < SupportedResolutions.Count; i++)
                {
                    if (ImGui.Selectable(SupportedResolutions[i].width + "x" + SupportedResolutions[i].height + " @" + SupportedResolutions[i].refreshRate + "Hz"))
                    {
                        selectedResolution = SupportedResolutions[i];
                        freeAspectRatio = false;
                        Renderer.Resize(selectedResolution.width, selectedResolution.height);
                        DebugLogPanel.Log("RESIZED RENDERER", DebugLogPanel.DebugMessageSeverity.Information, "Viewport Change");
                        Screen.Size.X = selectedResolution.width;
                        Screen.Size.Y = selectedResolution.height;
                    }
                }

                if (ImGui.Selectable("Free Aspect"))
                {
                    freeAspectRatio = true;
                }

                ImGui.EndCombo();
            }

            ImGui.EndChild();

            ImGui.PopStyleVar();

            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor();

            ImGui.SetCursorPosX((ImGui.GetWindowContentRegionMax().X * 0.5f) - ((size+10) * 0.5f));
            ImGui.SetCursorPosY(size + 15);
            ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(.21f, .21f, .21f, 0.7f));
            if (ImGui.Button(editor.EditorCurrentScene.IsPlaying == true ? MaterialDesign.Pause : MaterialDesign.Play_arrow, new Vector2(size + 10, size)))
            {

            }



            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);


        }

        public void DrawGuizmo2D()
        {
            ImGui.SetItemAllowOverlap();
            // CHANGE THIS

            GameObjectPanel gop = (GameObjectPanel)editor.Panels[1];

            if (gop.SelectedObject != null)
            {
                CanvasComponent canvasComponent = gop.SelectedObject.GetComponent<CanvasComponent>();
                if (canvasComponent != null)
                {
                    ImDrawListPtr drawList = ImGui.GetWindowDrawList();

                    //DebugLogPanel.Log(canvasComponent.left + resizeCoords.Item1 + " " + canvasComponent.right);
                    //drawList.AddLine(new Vector2(resizeCoords.Item1 + viewportPos.X, 50 + resizeCoords.Item2), new Vector2(canvasComponent.right, canvasComponent.bottom), UI.ToUint(new OpenTK.Mathematics.Vector4i(255,255,255,255)));
                }
            }
        }

    }
}
