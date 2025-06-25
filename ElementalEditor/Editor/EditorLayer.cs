using ElementalEditor.Editor.AssetHandling;
using ElementalEditor.Editor.Panels;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Core;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Serializing;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using MaterialIconFont;
using OIconFont;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using static EmberaEngine.Engine.Utilities.ModelImporter;
using static EmberaEngine.Engine.Utilities.NewModelImporter;

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

            interBoldFont = ImGui.GetIO().Fonts.AddFontFromFileTTF("Editor/Assets/Fonts/InterExtraBold.ttf", 20);
            EditorUI.DefaultFontLarge = ImGui.GetIO().Fonts.AddFontFromFileTTF("Editor/Assets/Fonts/JetBrainsMono-Bold.ttf", 32);
            app.ImGuiLayer.SetFont(ImGui.GetIO().Fonts.AddFontFromFileTTF("Editor/Assets/Fonts/JetBrainsMono-Bold.ttf", 20));
            
            app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/forkawesome-webfont.ttf", 25, (FontAwesome.ForkAwesome.IconMin, FontAwesome.ForkAwesome.IconMax16));
            app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/MaterialIcons-Regular.ttf", 25, (MaterialDesign.IconMin, MaterialDesign.IconMax16));
            materialIcon24 = app.ImGuiLayer.SetIconFont("Editor/Assets/Fonts/MaterialIcons-Regular.ttf", 128, (MaterialDesign.IconMin, MaterialDesign.IconMax16));
            app.ImGuiLayer.RecreateFontDevice();

            EditorUI.SetEditorStyling();

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
            //EditorUI.SetEditorStyling();
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

            EditorCurrentScene = SceneSerializer.DeSerialize(File.ReadAllBytes(Path.Combine(projectPath, Project.PROJECT_GAME_FILES_DIRECTORY, "scene2.dscn")));




            //GameObject cameraObject = EditorCurrentScene.addGameObject("Camera Boiii");
            //CameraComponent3D camComp = cameraObject.AddComponent<CameraComponent3D>();
            //camComp.ClearColor = new OpenTK.Mathematics.Color4(0, 0, 0, 255);
            //cameraObject.transform.Position = new OpenTK.Mathematics.Vector3(-5, 5f, 0);
            //cameraObject.transform.Rotation = new(0, -25, 0);

            //GameObject lightObject = EditorCurrentScene.addGameObject("LightObject");
            //lightObject.transform.Position = new Vector3(0, 6, 0);
            //lightObject.AddComponent<LightComponent>();




            //app.window.Close();

            //barrelObject = EditorCurrentScene.addGameObject("Barrel");
            //MeshRenderer barrelMeshRenderer = barrelObject.AddComponent<MeshRenderer>();
            //barrelObject.transform.Scale = Vector3.One * 1;

            //Mesh[] meshLoaderOutput = ModelImporter.LoadModel("Engine/Content/Models/SSAO Test/sphere.obj");

            //ModelData meshLoaderOutput = ModelImporter.LoadModel("Sponza/sponza.obj");
            //ModelData meshLoaderOutput = ModelImporter.LoadModel("JivinModel/barrel.fbx");

            //ModelData meshLoaderOutput = ModelImporter.LoadModel("AmbientShadowTesting/ambient_shadow_testing.fbx");
            //ModelData meshLoaderOutput = ModelImporter.LoadModel("ShooterTest/file.fbx");
            //if (meshLoaderOutput.rootObject == null)
            //{
            //    return;
            //}

            //Mesh[] meshLoaderOutput = ModelImporter.LoadModel("Engine/Content/Models/Portal2-Elevator/scene.gltf");

            //EditorCurrentScene.addGameObject(meshLoaderOutput.rootObject);



        }

        GameObject barrelObject;


    }
}
