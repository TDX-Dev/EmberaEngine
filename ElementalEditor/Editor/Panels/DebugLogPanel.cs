using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    public class DebugLogPanel : Panel
    {
        public enum DebugMessageSeverity
        {
            Warning,
            Information,
            Error
        }

        public struct DebugMessage
        {
            public DebugMessageSeverity severity;
            public string message;
            public string additionalInfo;
        }

        static List<DebugMessage> messages = new List<DebugMessage>();

        public override void OnAttach()
        {
            base.OnAttach();
        }

        public override void OnGUI()
        {
            if (ImGui.Begin("Debug Log"))
            {
                if (ImGui.BeginTable("##debuglog", 4, ImGuiTableFlags.BordersV))
                {
                    ImGui.TableSetupColumn("No.", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Severity", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Message");
                    ImGui.TableSetupColumn("Additional", ImGuiTableColumnFlags.WidthFixed);

                    ImGui.TableHeadersRow();
                    ImGui.TableNextColumn();

                    for (int i = 0; i < messages.Count; i++)
                    {
                        ImGui.Text((i+1).ToString());
                        ImGui.TableNextColumn();

                        ImGui.Text(messages[i].severity.ToString());
                        ImGui.TableNextColumn();

                        ImGui.Text(messages[i].message);
                        ImGui.TableNextColumn();
                        ImGui.Text(messages[i].additionalInfo);
                        ImGui.TableNextColumn();
                    }

                    ImGui.EndTable();
                }

                ImGui.End();
            }
        }

        public static void Log(string message, DebugMessageSeverity severity = DebugMessageSeverity.Information, string causingFactor = "")
        {
            if (messages.Count > 499)
            {
                messages.RemoveAt(0);
            }

            messages.Add(new DebugMessage()
            {
                message = message,
                severity = severity,
                additionalInfo = causingFactor
            });
        }
    }
}
