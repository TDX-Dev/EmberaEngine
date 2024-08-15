using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using ImGuiNET;
using EmberaEngine.Engine.Rendering;
using ElementalEditor.Editor.Panels;
using MaterialIconFont;
using OIconFont;
using EmberaEngine.Engine.Components;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Utilities;

namespace ElementalEditor.Editor
{
    public class EditorLayer : Layer
    {
        public Application app;
        public Scene EditorCurrentScene;

        public List<Panel> Panels = new List<Panel>();

        // Services

        public static DragDropService DragDropService = new DragDropService();

        public EditorLayer()
        {
            AddPanel<ViewportPanel>();
            AddPanel<GameObjectPanel>();
            AddPanel<ProjectAssetPanel>();
            AddPanel<DebugLogPanel>();
            AddPanel<ExperimentalPanel>();
        }

        public override void OnAttach()
        {
            SetEditorStyling();

            app.ImGuiLayer.SetFont(ImGui.GetIO().Fonts.AddFontFromFileTTF("Editor/Assets/Fonts/JetBrainsMono-Bold.ttf", 20));
            app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/forkawesome-webfont.ttf", 25, (FontAwesome.ForkAwesome.IconMin, FontAwesome.ForkAwesome.IconMax16));
            app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/MaterialIcons-Regular.ttf", 25, (MaterialDesign.IconMin, MaterialDesign.IconMax16));
            app.ImGuiLayer.RecreateFontDevice();

            // Setup Scene
            EditorCurrentScene = new Scene();
            LoadTestSandbox();


            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnAttach();
            }


            EditorCurrentScene.Initialize();
            EditorCurrentScene.Play();
        }

        public override void OnUpdate(float deltaTime)
        {
            EditorCurrentScene.OnUpdate(deltaTime);

            //t.Content = "FPS: " + Math.Round((1 / deltaTime));
        }

        public override void OnGUIRender()
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnGUI();
            }

            
        }

        public override void OnMouseButton(MouseButtonEvent buttonEvent)
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnMouseButton(buttonEvent);
            }
        }

        public override void OnMouseMove(MouseMoveEvent moveEvent)
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnMouseMove(moveEvent);
            }
        }

        public override void OnRender()
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnRender();
            }
        }

        public void AddPanel<T>() where T : Panel, new()
        {
            Panel panel = new T();
            panel.editor = this;
            Panels.Add(panel);

        }

        TextComponent t;

        void LoadTestSandbox()
        {
            CanvasComponent t1;

            GameObject mainScreenCanvas = EditorCurrentScene.addGameObject("MainCanvas");
            t1 = mainScreenCanvas.AddComponent<CanvasComponent>();
            t1.ScaleMode = CanvasScaleMode.ScaleWithScreen;

            GameObject playText = EditorCurrentScene.addGameObject("PlayText");
            playText.transform.position.X = 400;
            playText.transform.position.Y = 300;
            playText.transform.scale.X = 1;
            playText.transform.scale.Y = 1;

            t = playText.AddComponent<TextComponent>();
            t.Content = "TEXT OBJECT";

            GameObject playButton = EditorCurrentScene.addGameObject("PlayButton");
            playButton.transform.position.X = 100;
            playButton.transform.position.Y = 100;
            playButton.transform.scale.X = 50;
            playButton.transform.scale.Y = 50;
            SpriteRenderer sr = playButton.AddComponent<SpriteRenderer>();
            sr.SolidColor = new OpenTK.Mathematics.Vector4(1, 1, 1, 1);
            ButtonComponent btn = playButton.AddComponent<ButtonComponent>();

        }

        public void SetEditorStyling()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.11f, 0.10f, 0.08f, 1f);
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

            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1);
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1);
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.07f, 0.07f, 0.07f, 1);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.14f, 0.14f, 0.14f, 1);

            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.11f, 0.10f, 0.08f, 1f);
            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.9f);
            style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.7f);

            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(0.74f, 0.71f, 0.71f, 1f);

            style.WindowMenuButtonPosition = ImGuiDir.None;

            style.FramePadding = new System.Numerics.Vector2(14, 14);

            style.FrameRounding = 3f;
            style.TabRounding = 3f;
            style.PopupRounding = 5f;
        }


    }
}
