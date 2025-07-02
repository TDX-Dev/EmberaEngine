using ElementalEditor.Editor.Panels;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;

namespace ElementalEditor.Editor.GizmoAddons
{
    internal class ColliderGizmo : GizmoObject
    {

        public override Type ComponentType => typeof(ColliderComponent3D);

        private Texture lightTexture;

        public override void Initialize()
        {
            
        }

        public override void OnRender(Component component)
        {
            ColliderComponent3D lComponent = (ColliderComponent3D)component;
            if (lComponent.ColliderShape == ColliderShapeType.Box)
            {
                Guizmo3D.RenderCube(lComponent.gameObject.transform.Position, lComponent.Size, lComponent.gameObject.transform.Rotation);

            } else if (lComponent.ColliderShape == ColliderShapeType.Capsule)
            {
                Guizmo3D.DrawCapsule(lComponent.gameObject.transform.GlobalPosition, lComponent.Height, lComponent.Radius, lComponent.gameObject.transform.Rotation + new Vector3(), Color4.Yellow);
            }
        }
    }
}
