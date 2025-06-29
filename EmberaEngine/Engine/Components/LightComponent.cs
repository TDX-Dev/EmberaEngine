using EmberaEngine.Engine.Attributes;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Serializing;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;

namespace EmberaEngine.Engine.Components
{
    [ExecuteInPauseMode]
    public class LightComponent : Component
    {
        public override string Type => nameof(LightComponent);

        private PointLight pointLight;
        private SpotLight spotLight;
        private DirectionalLight directionalLight;

        private bool enabled = true;
        private Vector3 color = Vector3.One;
        private float intensity = 10;
        private float radius = 30;

        private float outerCutoff = 1f;
        private float innerCutoff = 1f;
        private LightType lightType = LightType.PointLight;
        private LightAttenuationType attenuationType = LightAttenuationType.Custom;
        private float linearFactor = 0.7f;
        private float quadraticFactor = 1.8f;

        public LightType LightType
        {
            get => lightType;
            set
            {
                if (lightType != value)
                {
                    OnChangeLightType(lightType, value);
                    lightType = value;
                }
            }
        }

        public bool Enabled
        {
            get => enabled;
            set { enabled = value; OnChangedValue(); }
        }

        public Color4 Color
        {
            get => new Color4(color.X, color.Y, color.Z, 1);
            set { color = Helper.ToVector4(value).Xyz; OnChangedValue(); }
        }

        public float Radius
        {
            get => radius;
            set { radius = value; OnChangedValue(); }
        }

        public float Intensity
        {
            get => intensity;
            set { intensity = value; OnChangedValue(); }
        }

        public LightAttenuationType AttenuationFunction
        {
            get => attenuationType;
            set { attenuationType = value; OnChangedValue(); }
        }

        public float LinearFactor
        {
            get => linearFactor;
            set { linearFactor = value; OnChangedValue(); }
        }

        public float QuadraticFactor
        {
            get => quadraticFactor;
            set { quadraticFactor = value; OnChangedValue(); }
        }

        public float InnerCutoff
        {
            get => MathHelper.RadiansToDegrees(innerCutoff);
            set { innerCutoff = MathHelper.DegreesToRadians(value); OnChangedValue(); }
        }

        public float OuterCutoff
        {
            get => MathHelper.RadiansToDegrees(outerCutoff);
            set { outerCutoff = MathHelper.DegreesToRadians(value); OnChangedValue(); }
        }

        public LightComponent()
        {
            // No light creation here; deferred to OnStart
        }

        private void DisposeLights()
        {
            if (pointLight != null)
            {
                LightManager.RemovePointLight(pointLight);
                pointLight = null;
            }

            if (spotLight != null)
            {
                LightManager.RemoveSpotLight(spotLight);
                spotLight = null;
            }

            if (directionalLight != null)
            {
                LightManager.RemoveDirectionalLight(directionalLight);
                directionalLight = null;
            }
        }

        public void OnChangeLightType(LightType previousValue, LightType newValue)
        {
            DisposeLights();
            lightType = newValue;
            OnStart(); // Recreate the new light
        }

        public void OnChangedValue()
        {
            switch (lightType)
            {
                case LightType.PointLight:
                    if (pointLight != null)
                    {
                        pointLight.position = gameObject.transform.GlobalPosition;
                        pointLight.Color = color;
                        pointLight.range = radius;
                        pointLight.intensity = intensity;
                        pointLight.enabled = enabled;
                        pointLight.attenuationType = (int)attenuationType;
                        pointLight.attenuationParameters = new Vector2(linearFactor, quadraticFactor);
                    }
                    break;

                case LightType.SpotLight:
                    if (spotLight != null)
                    {
                        spotLight.position = gameObject.transform.GlobalPosition;
                        spotLight.Color = color;
                        spotLight.range = radius;
                        spotLight.intensity = intensity;
                        spotLight.enabled = enabled;
                        spotLight.innerCutoff = innerCutoff;
                        spotLight.outerCutoff = outerCutoff;
                        spotLight.direction = Vector3.Normalize(gameObject.transform.Rotation);
                    }
                    break;

                case LightType.DirectionalLight:
                    if (directionalLight != null)
                    {
                        directionalLight.direction = gameObject.transform.Rotation;
                        directionalLight.color = color;
                        directionalLight.intensity = intensity;
                        directionalLight.enabled = enabled;
                    }
                    break;
            }
        }

        public override void OnStart()
        {
            DisposeLights(); // Just in case

            Vector3 position = gameObject.transform.GlobalPosition;

            switch (lightType)
            {
                case LightType.PointLight:
                    pointLight = LightManager.AddPointLight(position, color, intensity, radius);
                    break;

                case LightType.SpotLight:
                    spotLight = LightManager.AddSpotLight(
                        position, color,
                        Vector3.Normalize(gameObject.transform.Rotation),
                        intensity, radius, innerCutoff, outerCutoff);
                    break;

                case LightType.DirectionalLight:
                    directionalLight = LightManager.AddDirectionalLight(
                        gameObject.transform.Rotation, color, intensity);
                    break;
            }

            OnChangedValue(); // Sync properties after light is created
        }

        public override void OnUpdate(float dt)
        {
            if (gameObject.transform.hasMoved)
            {
                OnChangedValue();
            }
        }

        public override void OnDestroy()
        {
            DisposeLights();
        }
    }
}
