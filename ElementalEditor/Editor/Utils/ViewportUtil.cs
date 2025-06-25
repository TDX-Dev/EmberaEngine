using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using System.Numerics;
using MaterialIconFont;

namespace ElementalEditor.Editor.Utils
{
    public enum ViewportAlignment
    {
        Left,
        Center,
        Right
    }

    public class ViewportControl
    {
        public ViewportAlignment Alignment;
        public Vector2 Size;
        public Action Draw;

        public ViewportControl(ViewportAlignment alignment, Action draw, Vector2? size = null)
        {
            Alignment = alignment;
            Draw = draw;
            Size = size ?? new Vector2(40f); // Approximate space reservation
        }
    }



    static class ViewportUtil
    {
        public static void DrawViewportTools(float toolbarSize, List<ViewportControl> controls)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.5f, 0.5f));
            ImGui.BeginChild("##VIEWPORT_TOOLS", new Vector2(-1, toolbarSize), false,
                ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.AlwaysUseWindowPadding);

            float yCenter = toolbarSize / 2f - 20f;
            float windowWidth = ImGui.GetWindowSize().X;

            var left = controls.Where(c => c.Alignment == ViewportAlignment.Left).ToList();
            var center = controls.Where(c => c.Alignment == ViewportAlignment.Center).ToList();
            var right = controls.Where(c => c.Alignment == ViewportAlignment.Right).ToList();

            // LEFT
            float leftX = 10;
            foreach (var control in left)
            {
                ImGui.SetCursorPos(new Vector2(leftX, yCenter));
                ImGui.SetNextItemWidth(control.Size.X);
                control.Draw?.Invoke();
                leftX += control.Size.X + 5;
            }

            // CENTER
            float totalCenterWidth = center.Sum(c => c.Size.X + 5) - 5;
            float centerX = (windowWidth / 2f) - (totalCenterWidth / 2f);
            foreach (var control in center)
            {
                ImGui.SetCursorPos(new Vector2(centerX, yCenter));
                ImGui.SetNextItemWidth(control.Size.X);
                control.Draw?.Invoke();
                centerX += control.Size.X + 5;
            }

            // RIGHT
            float rightX = windowWidth;
            for (int i = right.Count - 1; i >= 0; i--)
            {
                var control = right[i];
                rightX -= control.Size.X + 10;
                ImGui.SetCursorPos(new Vector2(rightX, yCenter));
                ImGui.SetNextItemWidth(control.Size.X);
                control.Draw?.Invoke();
            }

            ImGui.EndChild();
            ImGui.PopStyleVar();
        }


        public static void ToggleGizmo(string label, GizmoType type)
        {
            bool isEnabled = Guizmo3D.EnabledGizmos.HasFlag(type);
            if (ImGui.Checkbox(label, ref isEnabled))
            {
                if (isEnabled)
                    Guizmo3D.EnabledGizmos |= type;
                else
                    Guizmo3D.EnabledGizmos &= ~type;
            }
        }




    }
}
