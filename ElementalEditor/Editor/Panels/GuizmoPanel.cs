using Assimp;
using ElementalEditor.Editor.GizmoAddons;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{



    class GuizmoPanel : Panel
    {
        public override void OnAttach()
        {
            // Register gizmo types centrally.
            GizmoRegistry.Register(new LightGizmo());
            GizmoRegistry.Register(new ColliderGizmo());
        }

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);
            // You can add interactive behavior here in future (like selection or hover)
        }

        public override void OnLateRender()
        {
            Renderer3D.GetOutputFrameBuffer().Bind();

            // Prepare for rendering gizmos (set camera, states, etc.)
            Guizmo3D.Render(editor.EditorCurrentScene);

            // Ask the registry to handle drawing all gizmos
            GizmoRegistry.RenderAll(editor.EditorCurrentScene);
        }

        public override void OnGUI()
        {
            if (ImGui.Begin("Gizmo Manager"))
            {
                ImGui.Text("Registered Gizmos:");
                foreach (var kvp in GizmoRegistry.GetRegisteredTypes())
                {
                    ImGui.Text($"- {kvp.Key.Name} ({kvp.Value.Count})");
                }

                ImGui.End();
            }
        }
    }

}
