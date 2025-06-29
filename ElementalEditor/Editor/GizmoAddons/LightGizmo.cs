using ElementalEditor.Editor.Panels;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;

namespace ElementalEditor.Editor.GizmoAddons
{
    internal class LightGizmo : GizmoObject
    {
        public override Type ComponentType => typeof(LightComponent);

        private Texture pointLightTexture;
        private Texture spotLightTexture;
        private Texture directionalLightTexture;

        public override void Initialize()
        {
            pointLightTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/PointLightOverlay.png");
            spotLightTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/SpotLightOverlay.png");
            directionalLightTexture = Helper.loadImageAsTex("Editor/Assets/Textures/GizmoTextures/DirectionalLightOverlay.png");
        }

        public override void OnRender(Component component)
        {
            LightComponent lComponent = (LightComponent)component;
            if (!lComponent.Enabled) return;

            var pos = component.gameObject.transform.GlobalPosition;

            switch (lComponent.LightType)
            {
                case LightType.PointLight:
                    Guizmo3D.RenderTexture(pointLightTexture, pos, Vector3.One);
                    Guizmo3D.RenderLightCircle(pos, Vector3.One * lComponent.Radius, Vector3.Zero);
                    break;

                case LightType.SpotLight:
                    Guizmo3D.RenderTexture(spotLightTexture, pos, Vector3.One);
                    break;

                case LightType.DirectionalLight:
                    Guizmo3D.RenderTexture(directionalLightTexture, pos, Vector3.One);
                    break;
            }
        }
    }
}
