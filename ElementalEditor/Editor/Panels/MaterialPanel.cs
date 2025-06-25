using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using MaterialIconFont;
using Microsoft.CodeAnalysis;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class MaterialPanel : Panel
    {

        public override void OnAttach()
        {

        }

        public override void OnGUI()
        {
            //if (ImGui.Begin("Test"))
            //{
            //    bool showTransform = true;
            //    if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
            //    {
            //        ImGui.TreePush();
            //        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(4, 3));
            //        ImGui.PushStyleColor(ImGuiCol.ChildBg, new System.Numerics.Vector4(0.15f, 0.15f, 0.15f, 1.0f)); // darker panel bg

            //        if (ImGui.BeginChild("TransformGroup", new System.Numerics.Vector2(0, 0), false))
            //        {
            //            ImGui.Text("Mode");
            //            ImGui.SameLine();
            //            ImGui.SetNextItemWidth(100);
            //            ImGui.Button("Hey!");
            //            ImGui.EndChild();
            //        }
            //        ImGui.PopStyleColor();
            //        ImGui.PopStyleVar();
            //        ImGui.TreePop();
            //    }



            //    ImGui.End();
            //}


            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new System.Numerics.Vector2(10, 10));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 2);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(10, 10));
            List<Material> materials = MaterialManager.GetMaterials();
            if (ImGui.Begin("Material Editor"))
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    PBRMaterial material = (PBRMaterial)materials.ElementAt(i);
                    //ImGui.PushID("material" + i);
                    EditorUI.DrawCollapsingHeader("material" + i, () =>
                    {
                        UI.BeginPropertyGrid("##material" + i);
                        UI.DrawComponentProperty(material.GetType().GetProperties(), material);
                        UI.EndPropertyGrid();
                    });

                    //ImGui.PopID();
                }


                ImGui.End();
            }

            ImGui.PopStyleVar(3);
        }

    }
}
