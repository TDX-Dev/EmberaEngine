using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    public static class GizmoRegistry
    {
        private static Dictionary<Type, List<GizmoObject>> gizmoRenderers = new();

        public static void Register(GizmoObject gizmo)
        {
            if (!gizmoRenderers.ContainsKey(gizmo.ComponentType))
            {
                gizmoRenderers[gizmo.ComponentType] = new List<GizmoObject>();
            }

            gizmoRenderers[gizmo.ComponentType].Add(gizmo);
            gizmo.Initialize();
        }

        public static void RenderAll(Scene scene)
        {
            foreach (var pair in gizmoRenderers)
            {
                var components = scene.GetAllComponentsOfType(pair.Key);

                foreach (var component in components)
                {
                    foreach (var gizmo in pair.Value)
                    {
                        gizmo.OnRender(component);
                    }
                }
            }
        }

        public static Dictionary<Type, List<GizmoObject>> GetRegisteredTypes()
        {
            return gizmoRenderers;
        }
    }


}
