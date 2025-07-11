﻿using ImGuiNET;
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
using ElementalEditor.Editor.CustomEditors;
using ElementalEditor.Editor.EditorAttributes;

namespace ElementalEditor.Editor.Panels
{
    public class GameObjectPanel : Panel
    {
        public static GameObject SelectedObject = null;
        private string searchBuffer = "";
        static int? activeDragComponentIndex = null;
        private static GameObject DraggedObject = null;

        public override void OnAttach()
        {
            CacheCustomEditors();
            GetComponents();

            SelectedObject = editor.EditorCurrentScene?.GameObjects[0];
        }

        private bool IsChildOf(GameObject child, GameObject potentialParent)
        {
            var current = child.parentObject;
            while (current != null)
            {
                if (current == potentialParent)
                    return true;
                current = current.parentObject;
            }
            return false;
        }


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

        private void DrawDropZone(GameObject dropTarget, bool before)
        {
            ImGui.PushID($"dropzone_{dropTarget.GetHashCode()}_{before}");

            ImGui.PushStyleColor(ImGuiCol.HeaderHovered, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.HeaderActive, Vector4.Zero);
            ImGui.Selectable("##DropZone", false, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(ImGui.GetContentRegionAvail().X, 4));
            ImGui.PopStyleColor(2);

            // --- Allow clicking the drop zone to select the GameObject ---
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                SelectedObject = dropTarget;
            }


            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("GAMEOBJECT");
                    if (payload.NativePtr != null)
                    {
                        if (DraggedObject != null && DraggedObject != dropTarget && !IsChildOf(dropTarget, DraggedObject))
                        {
                            // Get the target sibling list (same parent as dropTarget)
                            var newParent = dropTarget.parentObject;
                            var targetList = newParent?.children ?? editor.EditorCurrentScene.GameObjects;

                            // Remove from old parent
                            var oldList = DraggedObject.parentObject?.children ?? editor.EditorCurrentScene.GameObjects;
                            oldList.Remove(DraggedObject);

                            // Set new parent
                            DraggedObject.parentObject = newParent;

                            // Insert at appropriate position
                            int index = targetList.IndexOf(dropTarget);
                            if (!before) index++;
                            index = Math.Clamp(index, 0, targetList.Count);
                            targetList.Insert(index, DraggedObject);
                        }

                        DraggedObject = null;
                    }
                }
                ImGui.EndDragDropTarget();
            }

            ImGui.PopID();
        }



        private void DrawGameObjectRecursive(GameObject gameObject, int depth, ref int rowIndex)
        {
            ImGui.PushID(gameObject.GetHashCode());

            float rowHeight = 30f;
            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Vector2 rowSize = new Vector2(ImGui.GetContentRegionAvail().X, rowHeight);

            // Alternating background color
            Vector4 bgColor = (rowIndex % 2 == 0)
                ? new Vector4(0.16f, 0.16f, 0.16f, 1f)
                : new Vector4(0.18f, 0.18f, 0.18f, 1f);

            ImGui.GetWindowDrawList().AddRectFilled(
                cursorPos,
                new Vector2(cursorPos.X + rowSize.X, cursorPos.Y + rowHeight),
                ImGui.ColorConvertFloat4ToU32(bgColor)
            );

            // Reserve space and reset cursor to top of row
            ImGui.Dummy(new Vector2(0, rowHeight)); // Reserve vertical space
            ImGui.SetCursorScreenPos(cursorPos);    // Go back to top of row

            bool isSelected = (SelectedObject == gameObject);
            bool hasChildren = gameObject.children != null && gameObject.children.Count > 0;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding;
            if (!hasChildren)
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
            if (isSelected)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
                ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.26f, 0.45f, 0.78f, 0.8f));      // Selected
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.3f, 0.5f, 0.85f, 1.0f)); // Hovered + selected
            }

            // Adjust vertical padding to match row height
            float textHeight = ImGui.GetTextLineHeight();
            float verticalPadding = MathF.Max(0f, (rowHeight - textHeight) / 2f);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4f, verticalPadding));

            // Draw the tree node
            bool open = ImGui.TreeNodeEx(IconRegistry.GetFontIcon(typeof(GameObject), gameObject.children.Count > 0 ? 1 : 0).icon + " " + gameObject.Name, flags);

            ImGui.PopStyleVar(); // FramePadding
            if (isSelected) ImGui.PopStyleColor(2); // Header + Hovered color

            // Click to select
            if (ImGui.IsItemClicked())
                SelectedObject = gameObject;

            // Begin drag
            if (ImGui.BeginDragDropSource())
            {
                ImGui.SetDragDropPayload("GAMEOBJECT", IntPtr.Zero, 0);
                ImGui.Text(IconRegistry.GetFontIcon(typeof(GameObject)).icon + " " + gameObject.Name);
                DraggedObject = gameObject;
                ImGui.EndDragDropSource();
            }

            // Accept drop
            if (ImGui.BeginDragDropTarget())
            {
                ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("GAMEOBJECT");
                unsafe
                {
                    if (payload.NativePtr != null)
                    {
                        if (DraggedObject != null && DraggedObject != gameObject && !IsChildOf(DraggedObject, gameObject))
                        {
                            DraggedObject.parentObject?.children?.Remove(DraggedObject);
                            if (DraggedObject.parentObject == null)
                                editor.EditorCurrentScene.GameObjects.Remove(DraggedObject);

                            DraggedObject.parentObject = gameObject;
                            gameObject.children ??= new List<GameObject>();
                            gameObject.children.Add(DraggedObject);
                        }
                        DraggedObject = null;
                    }
                }
                ImGui.EndDragDropTarget();
            }

            rowIndex++;

            // Recursively draw children if node is open
            if (open && hasChildren)
            {
                var children = gameObject.children;
                for (int i = 0; i < children.Count; i++)
                {
                    DrawDropZone(children[i], before: true);
                    DrawGameObjectRecursive(children[i], depth + 1, ref rowIndex);
                }
                DrawDropZone(children[^1], before: false);
                ImGui.TreePop();
            }

            ImGui.PopID();
        }






        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

            if (ImGui.Begin(MaterialDesign.List + " GameObjects"))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5));
                if (ImGui.BeginChild("gameobject_child_window", new Vector2(ImGui.GetContentRegionAvail().X, 50), ImGuiChildFlags.AlwaysUseWindowPadding))
                {
                    ImGui.BeginGroup();

                    ImGui.Button(MaterialDesign.Search); // Icon as a button
                    ImGui.SameLine();

                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.InputTextWithHint("##goSearch", "Search...", ref searchBuffer, 100);

                    ImGui.EndGroup();
                }
                ImGui.EndChild();
                ImGui.PopStyleVar();

                // Push styles for the object list rendering
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(new Vector3(0.12f), 1f));

                if (ImGui.BeginChild("gameobject_hierarchy_scroll", ImGui.GetContentRegionAvail(), ImGuiChildFlags.AlwaysUseWindowPadding, ImGuiWindowFlags.ChildWindow))
                {
                    var roots = editor.EditorCurrentScene.GameObjects;
                    int rowIndex = 0;
                    for (int i = 0; i < roots.Count; i++)
                    {
                        DrawDropZone(roots[i], before: true);
                        DrawGameObjectRecursive(roots[i], 0, ref rowIndex);
                    }

                    if (roots.Count > 0)
                        DrawDropZone(roots[^1], before: false);

                    if (ImGui.IsWindowHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                    {
                        Console.WriteLine("Context Menu!");
                        ImGui.OpenPopup("GameObjectMenu");
                    }

                    ContextMenu.InspectorPanelPopup(this); // This is fine if it's showing a different popup
                }
                ImGui.EndChild();

                ImGui.PopStyleColor();
                ImGui.PopStyleVar(4);
            }

            ImGui.End();
            ImGui.PopStyleVar();


            // Drawing Inspector

            if (ImGui.Begin(MaterialDesign.Edit_square + " Inspector"))
            {
                if (SelectedObject != null)
                {
                    bool openPopupTemp = false;
                    bool a = true;

                    UI.BeginPropertyGrid("OBJECT PROPS");

                    UI.BeginProperty("Add Component");
                    if (UI.DrawButton("Add " + MaterialDesign.Add))
                    {
                        openPopupTemp = true;
                    }
                    UI.EndProperty();

                    UI.BeginProperty("Name");
                    UI.PropertyString(ref SelectedObject.Name, false);
                    UI.EndProperty();

                    UI.EndPropertyGrid();

                    if (openPopupTemp)
                    {
                        ImGui.OpenPopup("Component Menu");
                        ImGui.SetNextWindowPos(new Vector2(ImGui.GetWindowPos().X - ImGui.GetWindowSize().X / 2, ImGui.GetWindowPos().Y - ImGui.GetWindowSize().Y / 2));
                        openPopupTemp = false;

                    }



                    ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 30);
                    ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
                    if (ImGui.BeginPopupModal("Component Menu", ref a, ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.InputText("##component_search", ref searchBuffer, 20);

                        List<Type> components = componentCache;

                        for (int i = 0; i < components.Count; i++)
                        {
                            if (searchBuffer.Trim() == "" ||
    (searchBuffer.Length < components[i].Name.Length &&
     searchBuffer.ToLower() == components[i].Name.Substring(0, searchBuffer.Length).ToLower()))
                            {
                                FontIconEntry icon = IconRegistry.GetFontIcon(components[i]);
                                string iconStr = icon.icon;
                                string labelStr = components[i].Name;

                                Vector2 iconSize = ImGui.CalcTextSize(iconStr);
                                Vector2 labelSize = ImGui.CalcTextSize(labelStr);
                                float height = 50.0f;
                                float padding = 8.0f;

                                Vector2 fullSize = new Vector2(iconSize.X + labelSize.X + padding * 2, height);

                                // Get starting position
                                Vector2 cursorPos = ImGui.GetCursorScreenPos();

                                // Handle input
                                if (ImGui.InvisibleButton($"##component_btn_{i}", fullSize))
                                {
                                    SelectedObject.AddComponent((Component)Activator.CreateInstance(components[i]));
                                    ImGui.CloseCurrentPopup();
                                }

                                // Align text vertically centered
                                float iconYOffset = (height - iconSize.Y) * 0.5f;
                                float labelYOffset = (height - labelSize.Y) * 0.5f;

                                // Draw icon
                                ImGui.SetCursorScreenPos(new Vector2(cursorPos.X + padding, cursorPos.Y + iconYOffset));
                                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(icon.iconColor.R, icon.iconColor.G, icon.iconColor.B, icon.iconColor.A));
                                ImGui.TextUnformatted(iconStr);
                                ImGui.PopStyleColor();

                                // Draw label
                                ImGui.SameLine();
                                ImGui.SetCursorScreenPos(new Vector2(ImGui.GetCursorScreenPos().X, cursorPos.Y + labelYOffset));
                                ImGui.TextUnformatted(labelStr);


                            }

                        }

                        ImGui.EndPopup();
                    }
                    ImGui.PopStyleVar(2);



                    List<Component> componentsToRemove = new();

                    for (int i = 0; i < SelectedObject.Components.Count; i++)
                    {
                        Component component = SelectedObject.Components[i];
                        Type componentType = component.GetType();

                        bool isTransform = componentType == typeof(Transform);
                        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                        ImGui.PushID(componentType.Name + i);

                        string headerLabel = IconRegistry.GetFontIcon(componentType) + " " + component.Type;

                        // Collapsing header
                        bool open = EditorUI.DrawCollapsingHeaderStart(headerLabel, ImGuiTreeNodeFlags.DefaultOpen);

                        // === Context Menu ===
                        if (!isTransform && ImGui.BeginPopupContextItem("ComponentContext"))
                        {
                            if (ImGui.MenuItem("Remove Component"))
                            {
                                componentsToRemove.Add(component);
                            }
                            ImGui.EndPopup();
                        }

                        // === Drag Source ===
                        if (!isTransform && ImGui.BeginDragDropSource())
                        {
                            ImGui.SetDragDropPayload("COMPONENT_DRAG", IntPtr.Zero, 0); // dummy payload
                            activeDragComponentIndex = i;
                            ImGui.Text($"Move {component.Type}");
                            ImGui.EndDragDropSource();
                        }

                        // === Drop Target ===
                        if (!isTransform && ImGui.BeginDragDropTarget())
                        {
                            unsafe
                            {
                                ImGuiPayloadPtr payload = ImGui.AcceptDragDropPayload("COMPONENT_DRAG");
                                if (payload.NativePtr != null && activeDragComponentIndex.HasValue && activeDragComponentIndex.Value != i)
                                {
                                    var dragged = SelectedObject.Components[activeDragComponentIndex.Value];

                                    int insertIndex = i;
                                    if (activeDragComponentIndex.Value < insertIndex)
                                        insertIndex--;

                                    SelectedObject.Components.RemoveAt(activeDragComponentIndex.Value);
                                    SelectedObject.Components.Insert(insertIndex, dragged);

                                    activeDragComponentIndex = null; // clear it
                                }
                            }
                            ImGui.EndDragDropTarget();
                        }


                        // === Component UI ===
                        if (open)
                        {
                            EditorUI.DrawCollapsingHeaderContent(headerLabel, () =>
                            {
                                UI.BeginPropertyGrid("##" + i);

                                if (customEditorMap.TryGetValue(componentType, out var editorType))
                                {
                                    editorType.component = component;
                                    editorType.OnGUI();
                                }
                                else
                                {
                                    UI.DrawComponentProperty(componentType.GetProperties(), component);
                                    UI.DrawComponentField(componentType.GetFields(), component);
                                }

                                UI.EndPropertyGrid();
                            });
                        }
                        else
                        {
                            ImGui.EndChild(); // Close outer header window if content wasn't opened
                            ImGui.PopStyleColor();
                        }

                        ImGui.PopID();
                        ImGui.PopStyleVar();
                    }

                    // Apply deletions
                    foreach (var comp in componentsToRemove)
                    {
                        SelectedObject.RemoveComponent(comp);
                    }





                }
            }
            ImGui.End();
        }

        Dictionary<Type, CustomEditorScript> customEditorMap = new();

        void CacheCustomEditors()
        {
            var editorTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(CustomEditorScript).IsAssignableFrom(t));

            foreach (var editorType in editorTypes)
            {
                var attr = editorType.GetCustomAttribute<CustomEditor>();
                if (attr != null && attr.target != null)
                {
                    customEditorMap[attr.target] = (CustomEditorScript)Activator.CreateInstance(editorType);
                    customEditorMap[attr.target].OnEnable();
                }
            }
        }

        List<Type> componentCache = new List<Type>();
        List<Type> GetComponents()
        {

            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Component)))
                .ToList();

            componentCache = allTypes;
            return allTypes;
        }
    }

}