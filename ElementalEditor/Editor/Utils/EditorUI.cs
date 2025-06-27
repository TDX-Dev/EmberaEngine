using EmberaEngine.Engine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{




    public static class EditorUI
    {
        public static ImFontPtr DefaultFontLarge;
        public static ImFontPtr DefaultFontMedium;
        public static ImFontPtr DefaultFontSmall;

        public static Vector4 HeaderColor = new Vector4(new Vector3(0.247f), 0.5f);

        public static Vector2 FramePadding = new Vector2(7.5f);

        public static void SetEditorStyling()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            style.Colors[(int)ImGuiCol.Button] = new Vector4(0.325f, 0.325f, 0.325f, 1f);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.4f, 0.4f, 0.4f, 1f);
            //style.Colors[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.6f, 0.6f, 0.6f, 1f);

            //style.Colors[(int)ImGuiCol.Separator] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 1f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.1f, 0.1f, 0.1f, 1f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(new Vector3(0.117f), 1f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(new Vector3(0.117f), 1f);
            style.Colors[((int)ImGuiCol.WindowBg)] = new Vector4(new Vector3(0.165f), 1);
            style.Colors[((int)ImGuiCol.Header)] = HeaderColor;
            //style.Colors[((int)ImGuiCol.HeaderHovered)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            style.Colors[((int)ImGuiCol.HeaderActive)] = HeaderColor;

            style.Colors[(int)ImGuiCol.Tab] = new Vector4(new Vector3(0.117f) * 1.3f, 1f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(new Vector3(0.207f), 1f); // Hovered
            style.Colors[(int)ImGuiCol.TabSelected] = new Vector4(new Vector3(0.207f), 1f);
            style.Colors[(int)ImGuiCol.TabDimmed] = new Vector4(new Vector3(0.117f) * 1.3f, 1f);
            style.Colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(new Vector3(0.207f), 1);

            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.4f, 0.4f, 0.4f, 1f);

            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.325f, 0.325f, 0.325f, 1f);
            //style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.9f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(new Vector3(0.247f), 1f);

            //style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(0.71f, 0.71f, 0.71f, 1f);


            style.WindowMenuButtonPosition = ImGuiDir.None;

            style.FramePadding = FramePadding;
            style.WindowPadding = new Vector2(20);

            style.FrameRounding = 2f;
            style.ChildRounding = 2f;
            style.TabRounding = 2f;
            style.PopupRounding = 3f;
        }


        public static bool BeginWindow(string value, ImGuiWindowFlags flags)
        {
            bool isOpen = ImGui.Begin(value, flags);
            return isOpen;
        }

        public static void EndWindow()
        {
            ImGui.End();
        }



        public static void DrawCollapsingHeader(string value, Action action, float width = 0)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, HeaderColor);

            float childWidth = width == 0 ? ImGui.GetContentRegionAvail().X : width;

            if (ImGui.BeginChild(value + " childwindow", new Vector2(childWidth, 0), ImGuiChildFlags.AutoResizeY))
            {
                if (ImGui.CollapsingHeader(value))
                {
                    // ✅ Ensure inner window gets the full width
                    float innerWidth = ImGui.GetContentRegionAvail().X;
                    if (ImGui.BeginChild("innerwindow", new Vector2(innerWidth, 0), ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.AutoResizeY))
                    {
                        action();
                    }
                    ImGui.EndChild();
                }
            }
            ImGui.EndChild();

            ImGui.PopStyleColor();
        }

        public static bool DrawCollapsingHeaderStart(string value, ImGuiTreeNodeFlags flags)
        {
            ImGui.PushStyleColor(ImGuiCol.ChildBg, HeaderColor);

            float childWidth = ImGui.GetContentRegionAvail().X;
            if (ImGui.BeginChild(value + "_child", new Vector2(childWidth, 0), ImGuiChildFlags.AutoResizeY))
            {
                bool open = ImGui.CollapsingHeader(value, flags);
                return open;
            }

            return false;
        }

        public static void DrawCollapsingHeaderContent(string value, Action content)
        {
            float innerWidth = ImGui.GetContentRegionAvail().X;
            if (ImGui.BeginChild(value + "_inner", new Vector2(innerWidth, 0), ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.AutoResizeY))
            {
                content?.Invoke();
            }
            ImGui.EndChild();
            ImGui.EndChild();
            ImGui.PopStyleColor();
        }

        public static void DrawTextInput(string value, ref string text, bool multiline = false)
        {
            if (multiline)
            {

            } else
            {
                ImGui.InputText(value, ref text, 100);
            }
        }


    }
}
