using EmberaEngine.Engine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class MenuBar : Panel
    {
        Window window;

        public override void OnAttach()
        {

        }

        public override void OnGUI()
        {
            if (ImGui.Begin("Experimental"))
            {
                if (ImGui.Button("Open Code Editor"))
                {
                    Thread thread = new Thread(() =>
                    {
                        var spec = new WindowSpecification
                        {
                            Name = "Code Editor",
                            Width = 800,
                            Height = 600,
                            VSync = false,
                            customTitlebar = false,
                            useFullscreen = false,
                        };

                        // IMPORTANT: create & run the window inside the same thread
                        using var window = new Window(spec);
                        window.Run();
                    });

                    thread.IsBackground = true;
                    thread.SetApartmentState(ApartmentState.STA); // optional, sometimes helps with Win32/GLFW quirks
                    thread.Start();
                }
            }

            ImGui.End();
        }

    }
}
