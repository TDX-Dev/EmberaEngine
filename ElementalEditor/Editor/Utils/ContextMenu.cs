using ElementalEditor.Editor.Panels;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace ElementalEditor.Editor.Utils
{
    class ContextMenu
    {

        public static void InspectorPanelPopup(GameObjectPanel gameObjectPanel)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 10));

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0, 0, 0, 1));

            if (ImGui.BeginPopupContextWindow("MyPopup", ImGuiPopupFlags.MouseButtonRight))
            {
                if (ImGui.MenuItem("Action 1"))
                {
                    // Handle Action 1
                }

                if (ImGui.BeginMenu("Add Object"))
                {
                    if (ImGui.MenuItem("Basic GameObject"))
                    {
                        GameObjectPanel.SelectedObject = gameObjectPanel.editor.EditorCurrentScene.addGameObject("Basic Object");
                    }
                    if (ImGui.MenuItem("Collider Object"))
                    {
                        GameObjectPanel.SelectedObject = gameObjectPanel.editor.EditorCurrentScene.addGameObject("Collider Object");
                        GameObjectPanel.SelectedObject.AddComponent<ColliderComponent3D>();
                    }
                    if (ImGui.MenuItem("Camera"))
                    {
                        GameObjectPanel.SelectedObject = gameObjectPanel.editor.EditorCurrentScene.addGameObject("Camera Object");
                        GameObjectPanel.SelectedObject.AddComponent<CameraComponent3D>();
                    }



                    if (ImGui.BeginMenu("Primitives"))
                    {
                        if (ImGui.MenuItem("Cube"))
                        {
                            GameObjectPanel.SelectedObject = gameObjectPanel.editor.EditorCurrentScene.addGameObject("Cube Primitive");
                            MeshRenderer meshRenderer = GameObjectPanel.SelectedObject.AddComponent<MeshRenderer>();

                            Material material = Renderer3D.ActiveRenderingPipeline.GetDefaultMaterial();
                            material.shader = ShaderRegistry.GetShader("CLUSTERED_PBR");
                            Mesh mesh = Graphics.GetCube();
                            //mesh.MaterialIndex = MaterialManager.AddMaterial(material);


                            meshRenderer.SetMesh(mesh);
                        }


                        ImGui.EndMenu();
                    }




                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Another Submenu"))
                {
                    if (ImGui.MenuItem("Sub-option A"))
                    {
                        // Handle Sub-option A
                    }
                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }

            ImGui.PopStyleVar();

            ImGui.PopStyleColor();
        }







    }
}
