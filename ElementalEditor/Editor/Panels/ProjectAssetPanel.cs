using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    public class ProjectAssetPanel : Panel
    {

        public override void OnGUI()
        {
            if (ImGui.Begin("Project Assets"))
            {
                ImGui.End();
            }
        }
    }
}
