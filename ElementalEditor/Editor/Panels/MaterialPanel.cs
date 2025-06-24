using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using MaterialIconFont;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<Material> materials = MaterialManager.GetMaterials();
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new System.Numerics.Vector2(5, 5));
            if (ImGui.Begin("Material Editor"))
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    PBRMaterial material = (PBRMaterial)materials.ElementAt(i);
                    ImGui.PushID("material" + i);
                    if (ImGui.CollapsingHeader("Material " + i))
                    {
                        ImGui.TreePush();

                        UI.BeginPropertyGrid("##material" + i);
                        UI.DrawComponentProperty(material.GetType().GetProperties(), material);
                        UI.EndPropertyGrid();


                        ImGui.TreePop();
                    }

                    ImGui.PopID();
                }


                ImGui.End();
            }
        }

    }
}
