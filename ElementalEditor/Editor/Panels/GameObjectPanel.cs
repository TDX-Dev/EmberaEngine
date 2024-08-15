using ImGuiNET;
using MaterialIconFont;
using OIconFont;
using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Components;
using ElementalEditor.Editor.Utils;
using System.Numerics;
using System.Text;
using System.Reflection;
using EmberaEngine.Engine.Rendering;

namespace ElementalEditor.Editor.Panels
{
    public class GameObjectPanel : Panel
    {
        public GameObject SelectedObject = null;
        private string searchBuffer = "";

        public void DrawObjectButton(GameObject gameObject, int id)
        {
            bool selected = gameObject == SelectedObject;
            if (selected) { ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.4f, 0.4f, 0.4f, 1)); }
            ImGui.PushID("##object" + id);
            if (ImGui.Button(MaterialDesign.Cable + " " + gameObject.Name, new Vector2(-1, ImGui.GetTextLineHeight() + 10)))
            {
                SelectedObject = gameObject;
            }
            ImGui.PopID();
            if (selected) { ImGui.PopStyleColor(); }
        }

        public override void OnGUI()
        {

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(0, 0));
            if (ImGui.Begin(MaterialDesign.List +  " GameObjects"))
            {

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(10, 10));
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(5, 5));
                if (ImGui.BeginPopupContextWindow())
                {
                    ImGui.Text("Context Menu");
                    if (ImGui.Selectable("Create GameObject"))
                    {
                        editor.EditorCurrentScene.addGameObject("GameObject");
                    }
                    ImGui.EndPopup();
                }
                ImGui.PopStyleVar(2);


                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(5, 5));
                ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, System.Numerics.Vector2.Zero);
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.11f, 0.10f, 0.08f, 1f));


                if (ImGui.BeginTable("##gameObjects", 2)) {

                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Status");
                    ImGui.TableHeadersRow();
                    ImGui.TableNextColumn();
                    float columnSizeY = ImGui.GetTextLineHeight();

                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                    for (int i = 0; i < editor.EditorCurrentScene.GameObjects.Count; i++)
                    {
                        DrawObjectButton(editor.EditorCurrentScene.GameObjects[i], i);
                        
                        ImGui.TableNextColumn();

                        ImGui.Text("DISABLED");

                        ImGui.TableNextColumn();
                    }
                    ImGui.PopStyleVar(1);
                    ImGui.EndTable();
                }


                ImGui.PopStyleColor();

                ImGui.PopStyleVar(5);

                ImGui.End();
            }

            // Drawing Inspector

            if (ImGui.Begin(MaterialDesign.Edit_square + " Inspector"))
            {
                if (SelectedObject != null)
                {
                    bool openPopupTemp = false;
                    bool a = true;

                    UI.BeginPropertyGrid("OBJECT PROPS");
                    UI.BeginProperty("Object Properties");
                    if (UI.DrawButton("Add " + MaterialDesign.Add))
                    {
                        openPopupTemp = true;
                    }
                    UI.EndProperty();
                    UI.EndPropertyGrid();

                    if (openPopupTemp)
                    {
                        ImGui.OpenPopup("Component Menu");
                        ImGui.SetNextWindowPos(new Vector2(Screen.Size.X / 2 - ImGui.GetWindowSize().X / 2, Screen.Size.Y / 2 - ImGui.GetWindowSize().Y / 2));
                        openPopupTemp = false;
                        
                    }
                    ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 30);
                    ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
                    if (ImGui.BeginPopupModal("Component Menu", ref a, ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.InputText("##component_search", ref searchBuffer, 20);

                        List<Component> components = GetComponents();

                        for (int i = 0; i < components.Count; i++)
                        {
                            if (searchBuffer.Trim() == "")
                            {
                                if (ImGui.Button(MaterialDesign.Settings + components[i].GetType().Name.ToString(), new Vector2(-1, 50)))
                                {
                                    SelectedObject.AddComponent(components[i]);
                                    ImGui.CloseCurrentPopup();
                                }
                            } else if (searchBuffer.Length < components[i].GetType().Name.Length)
                            {
                                if (searchBuffer.ToLower() == components[i].GetType().Name.Substring(0, searchBuffer.Length).ToLower())
                                {
                                    if (ImGui.Button(MaterialDesign.Settings + components[i].GetType().Name.ToString(), new Vector2(-1, 50)))
                                    {
                                        SelectedObject.AddComponent(components[i]);
                                        ImGui.CloseCurrentPopup();
                                    }
                                }
                            }
                        }

                        ImGui.EndPopup();
                    }
                    ImGui.PopStyleVar(2);
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);

                    for (int i = 0; i < SelectedObject.Components.Count; i++)
                    {
                        Component component = SelectedObject.Components[i];
                        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                        ImGui.PushID(component.GetType().Name + i);
                        if (ImGui.CollapsingHeader(component.GetType() == typeof(Transform) ? MaterialDesign.Flip_to_front + " " + component.Type : component.Type))
                        {
                            ImGui.TreePush();
                            UI.BeginPropertyGrid("##" + i);
                            UI.DrawComponentField(component.GetType().GetFields(), (object)component);
                            UI.EndPropertyGrid();
                            ImGui.TreePop();
                        }
                        ImGui.PopStyleVar();
                        ImGui.PopID();
                    }


                    ImGui.PopStyleVar(1);

                }
                ImGui.End();
            }
        }

        List<Component> componentCache = new List<Component>();
        List<Component> GetComponents()
        {
            if (componentCache.Count > 0) { return componentCache; }
            List<Component> objects = new List<Component>();
            foreach (Type type in Assembly.GetAssembly(typeof(Component)).GetTypes().Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Component))))
            {
                objects.Add((Component)Activator.CreateInstance(type));
            }

            componentCache = objects;

            return objects;
        }
    }

}
