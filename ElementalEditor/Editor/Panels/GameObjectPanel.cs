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

            ImGui.Selectable("##DropZone", false, ImGuiSelectableFlags.AllowDoubleClick, new Vector2(ImGui.GetContentRegionAvail().X, 4));

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

            // --- DRAW ALTERNATING BACKGROUND ---
            Vector2 min = ImGui.GetItemRectMin(); // Only valid *after* a widget is rendered
            Vector2 max = ImGui.GetItemRectMax();

            float lineHeight = ImGui.GetTextLineHeightWithSpacing();
            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            var bgColor = (rowIndex % 2 == 0)
                ? new Vector4(0.16f, 0.16f, 0.16f, 1f)
                : new Vector4(0.18f, 0.18f, 0.18f, 1f);

            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(cursorPos.X, cursorPos.Y),
                new Vector2(cursorPos.X + ImGui.GetContentRegionAvail().X, cursorPos.Y + lineHeight),
                ImGui.ColorConvertFloat4ToU32(bgColor)
            );

            // --- CONTINUE WITH TREE NODE ---
            bool isSelected = (SelectedObject == gameObject);
            bool hasChildren = gameObject.children != null && gameObject.children.Count > 0;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanFullWidth;

            if (!hasChildren)
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

            if (isSelected)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
                ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.26f, 0.45f, 0.78f, 0.8f));         // Selected
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.3f, 0.5f, 0.85f, 1.0f));   // Selected + hovered
            }

            bool open = ImGui.TreeNodeEx(MaterialDesign.Crop_square + " " + gameObject.Name, flags);

            if (isSelected)
                ImGui.PopStyleColor(2);


            if (ImGui.IsItemClicked())
            {
                SelectedObject = gameObject;
            }

            if (ImGui.BeginDragDropSource())
            {
                ImGui.SetDragDropPayload("GAMEOBJECT", IntPtr.Zero, 0);
                ImGui.Text(MaterialDesign.Crop_square + " " + gameObject.Name);
                DraggedObject = gameObject;
                ImGui.EndDragDropSource();
            }

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

                // Setup padding and background for search bar section
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 5));
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.18f, 0.20f, 0.23f, 0f));

                if (ImGui.BeginChild("gameobject_child_window", new Vector2(ImGui.GetContentRegionAvail().X, 50), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    // Unified style
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.12f, 0.13f, 0.15f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 1f);

                    // Start a group so the icon and input share layout
                    ImGui.BeginGroup();

                    // Icon with background matching input
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.12f, 0.13f, 0.15f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.15f, 0.16f, 0.18f, 1f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.12f, 0.13f, 0.15f, 1f));
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(6, 5));

                    ImGui.Button(MaterialDesign.Search); // Icon as a button (for spacing and consistency)
                    ImGui.PopStyleVar();
                    ImGui.PopStyleColor(3);

                    ImGui.SameLine();

                    // Input field with hint and full remaining width
                    ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
                    ImGui.InputTextWithHint("##goSearch", "Search...", ref searchBuffer, 100);

                    ImGui.EndGroup();

                    ImGui.PopStyleVar(2);     // FrameRounding + FrameBorderSize
                    ImGui.PopStyleColor(2);   // FrameBg + Border
                    ImGui.EndChild();
                }

                ImGui.PopStyleColor();  // ChildBg
                ImGui.PopStyleVar(2);   // FramePadding + WindowPadding


                ContextMenu.InspectorPanelPopup(this);

                // Push styles for the object list rendering
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.11f, 0.10f, 0.08f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0));

                // Scrollable area for GameObject list
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10, 5));
                if (ImGui.BeginChild("gameobject_hierarchy_scroll", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    // Push styles for the object list rendering
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                    ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f));
                    ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.11f, 0.10f, 0.08f, 1f));

                    // Game object hierarchy rendering
                    var roots = editor.EditorCurrentScene.GameObjects;
                    int rowIndex = 0;
                    for (int i = 0; i < roots.Count; i++)
                    {
                        DrawDropZone(roots[i], before: true);
                        DrawGameObjectRecursive(roots[i], 0, ref rowIndex);
                    }

                    // Drop zone after the last item
                    if (roots.Count > 0)
                        DrawDropZone(roots[^1], before: false);

                    // Pop the final style sets
                    ImGui.PopStyleColor();     // Button color
                    ImGui.PopStyleVar(4);      // FramePadding, ButtonTextAlign, FrameBorderSize, ItemSpacing

                    ImGui.EndChild();          // End scrollable child
                }
                ImGui.PopStyleVar();           // WindowPadding for scrollable area
                ImGui.PopStyleColor();
            }

            ImGui.PopStyleVar();           // WindowPadding (from the very beginning)


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
                            if (searchBuffer.Trim() == "")
                            {
                                if (ImGui.Button(MaterialDesign.Settings + components[i].Name.ToString(), new Vector2(-1, 50)))
                                {
                                    SelectedObject.AddComponent((Component)Activator.CreateInstance(components[i]));
                                    ImGui.CloseCurrentPopup();
                                }
                            } else if (searchBuffer.Length < components[i].GetType().Name.Length)
                            {
                                if (searchBuffer.ToLower() == components[i].Name.Substring(0, searchBuffer.Length).ToLower())
                                {
                                    if (ImGui.Button(MaterialDesign.Settings + components[i].GetType().Name.ToString(), new Vector2(-1, 50)))
                                    {
                                        SelectedObject.AddComponent((Component)Activator.CreateInstance(components[i]));
                                        ImGui.CloseCurrentPopup();
                                    }
                                }
                            }
                        }

                        ImGui.EndPopup();
                    }
                    ImGui.PopStyleVar(2);
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);
                    List<Component> componentsToRemove = new();

                    for (int i = 0; i < SelectedObject.Components.Count; i++)
                    {
                        Component component = SelectedObject.Components[i];
                        Type componentType = component.GetType();

                        bool isTransform = componentType == typeof(Transform);
                        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
                        ImGui.PushID(componentType.Name + i);

                        string headerLabel = isTransform
                            ? MaterialDesign.Flip_to_front + " " + component.Type
                            : component.Type;

                        // Collapsing header
                        bool open = ImGui.CollapsingHeader(headerLabel, ImGuiTreeNodeFlags.DefaultOpen);

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
                            UI.BeginPropertyGrid("##" + i);
                            ImGui.TreePush();

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

                            ImGui.TreePop();
                            UI.EndPropertyGrid();
                        }

                        ImGui.PopID();
                        ImGui.PopStyleVar();
                    }

                    // Apply deletions
                    foreach (var comp in componentsToRemove)
                    {
                        SelectedObject.RemoveComponent(comp);
                    }




                    ImGui.PopStyleVar(1);

                }
                ImGui.End();
            }
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
