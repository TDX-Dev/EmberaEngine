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
using OpenTK.Mathematics;
using static EmberaEngine.Engine.Utilities.ModelImporter;

namespace ElementalEditor.Editor
{
    public class EditorLayer : Layer
    {
        public Application app;
        public Scene EditorCurrentScene;
        public EditorCamera EditorCamera;

        public ImFontPtr interBoldFont;

        public ImFontPtr materialIcon24;

        public string projectPath;

        public List<Panel> Panels = new List<Panel>();

        // Services

        public static DragDropService DragDropService = new DragDropService();

        public EditorLayer()
        {
            AddPanel<MenuBar>();
            AddPanel<GuizmoPanel>();
            AddPanel<ViewportPanel>();
            AddPanel<ProjectAssetPanel>();
            AddPanel<DebugLogPanel>();
            AddPanel<ExperimentalPanel>();
            AddPanel<PerformancePanel>();
            AddPanel<MaterialPanel>();
            AddPanel<GameObjectPanel>();
            AddPanel<TimelinePanel>();
        }

        void LoadProject()
        {
            Project.SetupProject(projectPath);

            //app.window.Close();
        }

        public override void OnAttach()
        {


            SetEditorStyling();

            interBoldFont = ImGui.GetIO().Fonts.AddFontFromFileTTF("Editor/Assets/Fonts/InterExtraBold.ttf", 20);
            app.ImGuiLayer.SetFont(ImGui.GetIO().Fonts.AddFontFromFileTTF("Editor/Assets/Fonts/JetBrainsMono-Bold.ttf", 20));
            
            app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/forkawesome-webfont.ttf", 25, (FontAwesome.ForkAwesome.IconMin, FontAwesome.ForkAwesome.IconMax16));
            app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/MaterialIcons-Regular.ttf", 25, (MaterialDesign.IconMin, MaterialDesign.IconMax16));
            materialIcon24 = app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/MaterialIcons-Regular.ttf", 128, (MaterialDesign.IconMin, MaterialDesign.IconMax16));
            app.ImGuiLayer.RecreateFontDevice();


            LoadProject();

            // Setup Scene
            EditorCurrentScene = new Scene();
            EditorCamera = new EditorCamera(65.0f, Screen.Size.X, Screen.Size.Y, 1000f, 0.1f);
            LoadTestSandbox();


            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnAttach();
            }


            EditorCurrentScene.Initialize();
            EditorCurrentScene.Play();
            Renderer3D.SetRenderCamera(EditorCamera.Camera);
        }

        public override void OnKeyDown(KeyboardEvent keyboardEvent)
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnKeyDown(keyboardEvent);
            }
        }

        public override void OnKeyUp(KeyboardEvent keyboardEvent)
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnKeyUp(keyboardEvent);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            EditorCurrentScene.OnUpdate(deltaTime);
            EditorCamera.Update(deltaTime);

            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnUpdate(deltaTime);
            }

            //t.Content = "FPS: " + Math.Round((1 / deltaTime));
            //barrelObject.transform.Rotation.Y += 100 * deltaTime;
        }

        public override void OnResize(int width, int height)
        {
            EditorCamera.SetViewportSize(width, height);
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

        public override void OnMouseWheel(MouseWheelEvent mouseWheel)
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnMouseWheel(mouseWheel);
            }
        }

        public override void OnRender()
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnRender();
            }
        }

        public override void OnLateRender()
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                Panels[i].OnLateRender();
            }
        }

        public void AddPanel<T>() where T : Panel, new()
        {
            Panel panel = new T();
            panel.editor = this;
            Panels.Add(panel);

        }

        public T GetPanel<T>() where T : Panel, new()
        {
            for (int i = 0; i < Panels.Count; i++)
            {
                if (typeof(T) == Panels[i].GetType())
                {
                    return (T)Panels[i];
                }
            }

            return null;
        }

        void LoadTestSandbox()
        {
            SkyboxManager.LoadHDRI(Helper.loadHDRIAsTex("Engine/Content/Textures/Skyboxes/autumn.hdr"));

            GameObject cameraObject = EditorCurrentScene.addGameObject("Camera Boiii");
            CameraComponent3D camComp = cameraObject.AddComponent<CameraComponent3D>();
            camComp.ClearColor = new OpenTK.Mathematics.Color4(0, 0, 0, 255);
            cameraObject.transform.Position = new OpenTK.Mathematics.Vector3(-5, 5f, 0);
            cameraObject.transform.Rotation = new(0, -25, 0);

            GameObject lightObject = EditorCurrentScene.addGameObject("LightObject");
            lightObject.transform.Position = new Vector3(0, 6, 0);
            lightObject.AddComponent<LightComponent>();


            NewModelImporter.Load("AmbientShadowTesting/ambient_shadow_testing.fbx");

            //barrelObject = EditorCurrentScene.addGameObject("Barrel");
            //MeshRenderer barrelMeshRenderer = barrelObject.AddComponent<MeshRenderer>();
            //barrelObject.transform.Scale = Vector3.One * 1;

            //Mesh[] meshLoaderOutput = ModelImporter.LoadModel("Engine/Content/Models/SSAO Test/sphere.obj");

            ModelData meshLoaderOutput = ModelImporter.LoadModel("Sponza/sponza.obj");
            //ModelData meshLoaderOutput = ModelImporter.LoadModel("JivinModel/barrel.fbx");

            //ModelData meshLoaderOutput = ModelImporter.LoadModel("AmbientShadowTesting/ambient_shadow_testing.fbx");
            //ModelData meshLoaderOutput = ModelImporter.LoadModel("ShooterTest/file.fbx");
            if (meshLoaderOutput.rootObject == null)
            {
                return;
            }

            //Mesh[] meshLoaderOutput = ModelImporter.LoadModel("Engine/Content/Models/Portal2-Elevator/scene.gltf");

            //EditorCurrentScene.addGameObject(meshLoaderOutput.rootObject);
        }

        GameObject barrelObject;

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

            style.Colors[(int)ImGuiCol.Tab] = new System.Numerics.Vector4(0.18f, 0.20f, 0.23f, 1f); // Default tab
            style.Colors[(int)ImGuiCol.TabHovered] = new System.Numerics.Vector4(0.28f, 0.30f, 0.35f, 1f); // Hovered
            style.Colors[(int)ImGuiCol.TabActive] = new System.Numerics.Vector4(0.22f, 0.24f, 0.27f, 1f); // Active tab
            style.Colors[(int)ImGuiCol.TabUnfocused] = new System.Numerics.Vector4(0.12f, 0.12f, 0.13f, 1f); // Background/inactive
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new System.Numerics.Vector4(0.18f, 0.20f, 0.23f, 1f); // Active but unfocused



            style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(0.11f, 0.10f, 0.08f, 1f);
            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.9f);
            style.Colors[(int)ImGuiCol.ChildBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.7f);

            style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.9f, 0.9f, 0.9f, 1f);

            style.Colors[(int)ImGuiCol.Text] = new System.Numerics.Vector4(0.74f, 0.71f, 0.71f, 1f);


            style.WindowMenuButtonPosition = ImGuiDir.None;

            style.FramePadding = new System.Numerics.Vector2(12, 12);

            style.FrameRounding = 2f;
            style.TabRounding = 2f;
            style.PopupRounding = 3f;
        }


    }
}
