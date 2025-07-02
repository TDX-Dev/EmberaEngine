using ElementalEditor.Editor.Panels;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;

namespace ElementalEditor.Editor.GizmoAddons
{
    internal class CameraGizmo : GizmoObject
    {

        public override Type ComponentType => typeof(CameraComponent3D);


        public override void Initialize()
        {
        }

        public override void OnRender(Component component)
        {
            CameraComponent3D camera = (CameraComponent3D)component;
            Transform transform = camera.gameObject.transform;

            Vector3 origin = transform.GlobalPosition;
            Vector3 forward = camera.Front.Normalized();
            Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
            Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));

            float length = 3f;
            float spread = 0.125f;

            Vector3 tip = origin + forward * length;

            // Aspect ratio adjustment: 16:9
            float aspectX = 16f;
            float aspectY = 9f;
            Vector3 rightSpread = right * spread * aspectX;
            Vector3 upSpread = up * spread * aspectY;

            // Draw main forward line (camera direction)
            //Guizmo3D.RenderLine(origin, tip, Color4.White);

            // Frustum corner offsets
            Vector3 fovOffset1 = tip + (rightSpread + upSpread);
            Vector3 fovOffset2 = tip + (-rightSpread + upSpread);
            Vector3 fovOffset3 = tip + (-rightSpread - upSpread);
            Vector3 fovOffset4 = tip + (rightSpread - upSpread);

            // Frustum lines from origin
            Guizmo3D.RenderLine(origin, fovOffset1, Color4.Orange);
            Guizmo3D.RenderLine(origin, fovOffset2, Color4.Orange);
            Guizmo3D.RenderLine(origin, fovOffset3, Color4.Orange);
            Guizmo3D.RenderLine(origin, fovOffset4, Color4.Orange);

            // Frustum rectangle at the far end
            Guizmo3D.RenderLine(fovOffset1, fovOffset2, Color4.Orange);
            Guizmo3D.RenderLine(fovOffset2, fovOffset3, Color4.Orange);
            Guizmo3D.RenderLine(fovOffset3, fovOffset4, Color4.Orange);
            Guizmo3D.RenderLine(fovOffset4, fovOffset1, Color4.Orange);
        }


    }
}
