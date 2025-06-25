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

        public static Vector4 HeaderColor = new Vector4(new Vector3(0.327f), 0.5f);

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
            style.Colors[((int)ImGuiCol.Header)] = new Vector4(new Vector3(0.327f), 0.5f);
            //style.Colors[((int)ImGuiCol.HeaderHovered)] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.5f);
            //style.Colors[((int)ImGuiCol.HeaderActive)] = new System.Numerics.Vector4(0.2f, 0.2f, 0.2f, 0.5f);

            style.Colors[(int)ImGuiCol.Tab] = new Vector4(new Vector3(0.117f) * 1.3f, 1f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(new Vector3(0.207f), 1f); // Hovered
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(new Vector3(0.207f), 1f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(new Vector3(0.117f) * 1.3f, 1f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(new Vector3(0.207f), 1);

            style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.4f, 0.4f, 0.4f, 1f);

            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.325f, 0.325f, 0.325f, 1f);
            //style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 0.9f);
            style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(new Vector3(0.247f), 1f);

            //style.Colors[(int)ImGuiCol.PopupBg] = new System.Numerics.Vector4(0.1f, 0.1f, 0.1f, 1f);

            style.Colors[(int)ImGuiCol.Text] = new Vector4(0.71f, 0.71f, 0.71f, 1f);


            style.WindowMenuButtonPosition = ImGuiDir.None;

            style.FramePadding = new Vector2(10, 10);
            style.WindowPadding = new Vector2(20);

            style.FrameRounding = 2f;
            style.ChildRounding = 2f;
            style.TabRounding = 2f;
            style.PopupRounding = 3f;
        }


        public static bool BeginWindow(string value, ImGuiWindowFlags flags)
        {
            bool isOpen =  ImGui.Begin(value, flags);
            return isOpen;
        }

        public static void EndWindow()
        {
            ImGui.End();
        }

        public static void DrawCollapsingHeader(string value, Action action)
        {
            if (ImGui.CollapsingHeader(value, ImGuiTreeNodeFlags.FramePadding))
            {
                ImGui.PushStyleColor(ImGuiCol.ChildBg, HeaderColor);

                // Remove any active indentation before drawing the child
                float indent = 4;
                ImGui.Unindent(indent);

                // Match width and position
                float childWidth = ImGui.GetContentRegionMax().X;
                if (ImGui.BeginChild(value + "#", new Vector2(childWidth, 0), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    ImGui.TreePush();
                    action();
                    ImGui.TreePop();
                    ImGui.EndChild();
                }

                ImGui.Indent(indent);   

                ImGui.PopStyleColor();
            }
        }


    }
}
