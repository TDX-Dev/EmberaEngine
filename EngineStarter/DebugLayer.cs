using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Utilities;
using OpenTK.Graphics.OpenGL;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;

namespace EngineStarter
{
    public class DebugLayer : Layer
    {
        public Application Application;
        public Scene scene;

        public int width, height;

        public override void OnAttach()
        {
            scene = new Scene();
            LoadTestSandbox();
            scene.Initialize();
            scene.Play();
        }

        TextComponent t;
        CanvasComponent t1;

        void LoadTestSandbox()
        {
            GameObject mainScreenCanvas = scene.addGameObject("MainCanvas");
            t1 = mainScreenCanvas.AddComponent<CanvasComponent>();
            t1.ScaleMode = CanvasScaleMode.ScaleWithScreen;

            GameObject playText = scene.addGameObject("PlayText");
            playText.AddComponent<ButtonComponent>();
            playText.transform.position.X = 400;
            playText.transform.position.Y = 300;
            playText.transform.scale.X = 800;
            playText.transform.scale.Y = 600;

            Image image = new EmberaEngine.Engine.Utilities.Image();
            image.LoadPNG("untitled.png");


            Texture debugTexture = new Texture(EmberaEngine.Engine.Core.TextureTarget2d.Texture2D);
            debugTexture.SetFilter(EmberaEngine.Engine.Core.TextureMinFilter.Linear, EmberaEngine.Engine.Core.TextureMagFilter.Linear);
            debugTexture.SetWrapMode(EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge, EmberaEngine.Engine.Core.TextureWrapMode.ClampToEdge);
            debugTexture.TexImage2D<byte>(image.Width, image.Height, EmberaEngine.Engine.Core.PixelInternalFormat.Rgba16f, EmberaEngine.Engine.Core.PixelFormat.Rgba, EmberaEngine.Engine.Core.PixelType.UnsignedByte, image.Pixels);
            debugTexture.GenerateMipmap();

            playText.AddComponent<SpriteRenderer>().Sprite = debugTexture;

            GameObject screenText = scene.addGameObject("new");
            SpriteRenderer sr = screenText.AddComponent<SpriteRenderer>();

            sr.Sprite = debugTexture;
            //sr.SolidColor = new OpenTK.Mathematics.Vector4(0, 1, 0, 0.2f);
            screenText.transform.position.X = 400 - 50;
            screenText.transform.position.Y = 300;
            screenText.transform.scale.X = 200;
            screenText.transform.scale.Y = 100;

        }

        float updateTime = 0f;

        public override void OnUpdate(float deltaTime)
        {

            scene.OnUpdate(deltaTime);

            


        }

        public override void OnMouseMove(MouseMoveEvent moveEvent)
        {

        }

        public override void OnMouseButton(MouseButtonEvent buttonEvent)
        {

        }

        public override void OnRender()
        {

        }

        public override void OnGUIRender()
        {
            if (ImGui.Begin("Hello World - Debug"))
            {



                ImGui.End();
            }
        }

        public void SetEditorStyling()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.4f, 0.4f, 0.4f, 1f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1f);
            style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[(int)ImGuiCol.TitleBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1);
            style.Colors[((int)ImGuiCol.WindowBg)] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1);
            style.Colors[((int)ImGuiCol.Header)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderHovered)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderActive)] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 0.5f);

            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.09f, 0.09f, 0.09f, 1);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1);
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.3f, 0.3f, 0.3f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.07f, 0.07f, 0.07f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);

            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1);
            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.9f);
            style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.7f);


            style.FramePadding = new System.Numerics.Vector2(10, 7);

            style.FrameRounding = 3f;
            style.TabRounding = 3f;
            style.PopupRounding = 5f;
        }

        float angle = 0;
        float jumpCount = 0;
        public override void OnKeyDown(KeyboardEvent keyboardEvent)
        {
            if (keyboardEvent.Key == Keys.Backspace)
            {
                if (t.Content.Length > 1)
                {
                    t.Content = t.Content.Substring(0, t.Content.Length - 1);
                }
                else
                {
                    t.Content = "";
                }
            }
            if (keyboardEvent.Key == Keys.Enter)
            {
                t.Content += "\n";
            }
        }

        public override void OnTextInput(TextInputEvent textInputEvent)
        {
        }

        public override void OnResize(int width, int height)
        {
            this.width = width;
            this.height = height;
        }


    }
}
