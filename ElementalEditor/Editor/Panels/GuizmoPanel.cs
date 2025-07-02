using ElementalEditor.Editor.GizmoAddons;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{



    class GuizmoPanel : Panel
    {

        Texture gameObjectTexture;

        public override void OnAttach()
        {
            gameObjectTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/ObjectIndicator.png");

            // Register gizmo types centrally.
            GizmoRegistry.Register(new LightGizmo());
            GizmoRegistry.Register(new ColliderGizmo());
            GizmoRegistry.Register(new CameraGizmo());
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            // You can add interactive behavior here in future (like selection or hover)
        }

        public override void OnLateRender()
        {
            Renderer3D.GetOutputFrameBuffer().Bind();

            // Prepare for rendering gizmos (set camera, states, etc.)
            Guizmo3D.Render(editor.EditorCurrentScene);

            Guizmo3D.DrawGrid();

            //if (GameObjectPanel.SelectedObject != null)
            //{
            //    Guizmo3D.RenderTexture(gameObjectTexture, GameObjectPanel.SelectedObject.transform.GlobalPosition, Vector3.One);
            //}

            void RenderGameObjectAndChildren(GameObject gameObject)
            {
                Vector3 position = gameObject.transform.GlobalPosition;

                // Calculate distance from camera to object
                var camera = editor.EditorCamera;
                float distance = Vector3.Distance(camera.GetPosition(), position);

                // Project a pixel size to world units at that distance
                float pixelSize = 32.0f; // Target size in pixels on screen
                float worldSize = GetWorldSizeFromPixels(pixelSize, distance, camera);

                Vector3 scale = new Vector3(worldSize);

                Guizmo3D.RenderTexture(gameObjectTexture, position, scale);

                foreach (var child in gameObject.children)
                {
                    RenderGameObjectAndChildren(child);
                }
            }


            foreach (GameObject gameObject in editor.EditorCurrentScene.GameObjects)
            {
                RenderGameObjectAndChildren(gameObject);
            }

            // Ask the registry to handle drawing all gizmos
            GizmoRegistry.RenderAll(editor.EditorCurrentScene);
        }

        float GetWorldSizeFromPixels(float pixelSize, float distanceToCamera, EditorCamera camera)
        {
            float fov = camera.Fov; // In degrees
            float screenHeight = Screen.Size.Y; // in pixels

            // Convert FOV to radians
            float fovRad = MathF.PI * fov / 180f;

            // Total height in world units at this distance
            float worldScreenHeight = 2.0f * distanceToCamera * MathF.Tan(fovRad / 2.0f);

            // Each pixel's size in world units
            float pixelsToWorld = worldScreenHeight / screenHeight;

            return pixelsToWorld * pixelSize;
        }


        public override void OnGUI()
        {
            if (ImGui.Begin("Gizmo Manager"))
            {
                ImGui.Text("Registered Gizmos:");
                foreach (var kvp in GizmoRegistry.GetRegisteredTypes())
                {
                    ImGui.Text($"- {kvp.Key.Name} ({kvp.Value.Count})");
                }
            }
            ImGui.End();
        }
    }

}
