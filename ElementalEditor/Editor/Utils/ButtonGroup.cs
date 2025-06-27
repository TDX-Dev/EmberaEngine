using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    public class ButtonGroup
    {
        public enum RenderMode
        {
            ImGuiButtons,
            CustomDraw
        }

        private readonly List<(string label, Action onClick)> buttons = new();
        public float rounding = 6f;
        public Vector2 padding;
        public float spacing = 0f;
        public RenderMode mode = RenderMode.ImGuiButtons;
        public string groupId;

        public ButtonGroup(string id, RenderMode mode = RenderMode.ImGuiButtons)
        {
            this.groupId = id;
            this.rounding = ImGui.GetStyle().FrameRounding;
            this.padding = ImGui.GetStyle().FramePadding;
            this.spacing = spacing;
            this.mode = mode;
        }

        public void Add(string label, Action onClick)
        {
            buttons.Add((label, onClick));
        }

        public void Render()
        {
            if (buttons.Count == 0) return;

            if (mode == RenderMode.ImGuiButtons)
                RenderWithImGuiButtons();
            else
                RenderWithCustomDraw();
        }

        private void RenderWithImGuiButtons()
        {
            var oldSpacing = ImGui.GetStyle().ItemSpacing;

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, oldSpacing.Y));

            for (int i = 0; i < buttons.Count; i++)
            {
                bool isFirst = i == 0;
                bool isLast = i == buttons.Count - 1;

                float currentRounding = isFirst || isLast ? rounding : 0f;
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, currentRounding);

                if (i > 0)
                    ImGui.SameLine(0, 0);

                if (ImGui.Button(buttons[i].label, new Vector2(0, 0)))
                    buttons[i].onClick?.Invoke();

                ImGui.PopStyleVar();
            }

            ImGui.PopStyleVar(); // ItemSpacing
        }

        private void RenderWithCustomDraw()
        {
            var drawList = ImGui.GetWindowDrawList();
            var cursorPos = ImGui.GetCursorScreenPos();
            Vector2 pos = cursorPos;

            for (int i = 0; i < buttons.Count; i++)
            {
                var label = buttons[i].label;
                var textSize = ImGui.CalcTextSize(label);
                Vector2 buttonSize = new Vector2(textSize.X + padding.X * 2, textSize.Y + padding.Y * 2);

                Vector2 p0 = pos;
                Vector2 p1 = new Vector2(pos.X + buttonSize.X, pos.Y + buttonSize.Y);

                ImDrawFlags flags = ImDrawFlags.None;
                if (i == 0)
                    flags |= ImDrawFlags.RoundCornersLeft;
                if (i == buttons.Count - 1)
                    flags |= ImDrawFlags.RoundCornersRight;
                if (i > 0 && i < buttons.Count - 1)
                    flags = ImDrawFlags.RoundCornersNone;

                // Create ImGui item for interaction tracking
                ImGui.SetCursorScreenPos(pos);
                ImGui.InvisibleButton($"##btn{i}{groupId}", buttonSize);

                // ✅ FIXED: Only show active if mouse press started on this item
                bool hovered = ImGui.IsItemHovered();
                bool active = ImGui.IsItemActive() && hovered;

                uint color = active
                    ? ImGui.GetColorU32(ImGuiCol.ButtonActive)
                    : hovered
                        ? ImGui.GetColorU32(ImGuiCol.ButtonHovered)
                        : ImGui.GetColorU32(ImGuiCol.Button);

                // Draw background
                drawList.AddRectFilled(p0, p1, color, rounding, flags);

                // Draw text centered
                Vector2 textPos = new Vector2(
                    p0.X + padding.X,
                    p0.Y + padding.Y
                );
                drawList.AddText(textPos, ImGui.GetColorU32(ImGuiCol.Text), label);

                // Click logic: only fire if released while still hovering after a press started here
                if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    ButtonGroupInputState.RegisterPress(groupId, i);

                if (ImGui.IsItemHovered() && ButtonGroupInputState.ShouldTrigger(groupId, i))
                    buttons[i].onClick?.Invoke();

                ButtonGroupInputState.ClearIfReleased(groupId);

                // Advance to next button
                pos.X += buttonSize.X + spacing;
                if (i < buttons.Count - 1)
                    ImGui.SameLine(0, spacing);
            }
        }

    }

    static class ButtonGroupInputState
    {
        private static Dictionary<string, int?> groupPressMap = new();

        public static void RegisterPress(string groupId, int index)
        {
            groupPressMap[groupId] = index;
        }

        public static bool ShouldTrigger(string groupId, int index)
        {
            return groupPressMap.TryGetValue(groupId, out int? pressed) &&
                   pressed == index &&
                   ImGui.IsMouseReleased(ImGuiMouseButton.Left);
        }

        public static void ClearIfReleased(string groupId)
        {
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                groupPressMap[groupId] = null;
        }
    }


}
