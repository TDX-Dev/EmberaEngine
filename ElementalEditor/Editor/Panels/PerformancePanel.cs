using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class PerformancePanel : Panel
    {
        Queue<float> frameTimes = new Queue<float>();
        const int sampleCount = 10;

        public override void OnGUI()
        {

            // Compute average dt
            float avgDt = 0;
            foreach (var t in frameTimes)
                avgDt += t;
            avgDt /= frameTimes.Count;

            float avgFps = 1f / avgDt;

            if (ImGui.Begin("Performance"))
            {
                if (ImGui.BeginTable("##debuglog", 2, ImGuiTableFlags.BordersV))
                {
                    ImGui.TableSetupColumn("Category", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Performance", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableHeadersRow();

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text("Avg Frame Rate");

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text($"{avgFps:F2} FPS");

                    ImGui.EndTable();
                }
            }
            ImGui.End();

        }

        public override void OnUpdate(float dt)
        {
            // Store the latest dt
            frameTimes.Enqueue(dt);
            if (frameTimes.Count > sampleCount)
                frameTimes.Dequeue();
        }
    }
}
