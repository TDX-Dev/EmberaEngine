using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class WorldEnvironment : Component
    {
        public override string Type => nameof(WorldEnvironment);

        private float exposure = 1f;
        private float bloomIntensity = 1f;
        private TonemapFunction tonemapFunction = TonemapFunction.ACES;
        private bool useBloom = true;
        private bool useSSAO = true;
        private bool useIBL = true;
        private bool useAntialiasing = true;
        private bool renderSkybox = true;
        private Color4 ambientColor = Color4.Black;
        private float ambientFactor = 0.1f;
        private MSAA_Samples msaa_samples = MSAA_Samples.Disabled;
        private AmbientOcclusionScale occlusion_scale = AmbientOcclusionScale.Low;


        private RenderSetting renderSetting;

        public float Exposure
        {
            get => exposure;
            set
            {
                exposure = value;
                OnChangeValue();
            }
        }

        public float BloomIntensity
        {
            get => bloomIntensity;
            set
            {
                bloomIntensity = value;
                OnChangeValue();
            }
        }

        public TonemapFunction Tonemapper
        {
            get => tonemapFunction;
            set
            {
                tonemapFunction = value;
                OnChangeValue();
            }
        }

        public bool UseBloom
        {
            get => useBloom;
            set
            {
                useBloom = value;
                OnChangeValue();
            }
        }

        public bool UseSSAO
        {
            get => useSSAO;
            set
            {
                useSSAO = value;
                OnChangeValue();
            }
        }

        public bool UseIBL
        {
            get => useIBL;
            set
            {
                useIBL = value;
                OnChangeValue();
            }
        }

        public bool UseAntiAliasing
        {
            get => useAntialiasing;
            set
            {
                useAntialiasing = value;
                OnChangeValue();
            }
        }

        public bool RenderSkybox
        {
            get => renderSkybox;
            set
            {
                renderSkybox = value;
                OnChangeValue();
            }
        }

        public Color4 AmbientColor
        {
            get => ambientColor;
            set
            {
                ambientColor = value;
                OnChangeValue();
            }
        }

        public float AmbientFactor
        {
            get => ambientFactor;
            set
            {
                ambientFactor = value;
                OnChangeValue();
            }
        }

        public MSAA_Samples MSAA
        {
            get => msaa_samples;
            set
            {
                msaa_samples = value;
                OnChangeValue();
            }
        }

        public AmbientOcclusionScale AO_Scale
        {
            get => occlusion_scale;
            set
            {
                occlusion_scale = value;
                OnChangeValue();
            }
        }

        public override void OnStart()
        {
            OnChangeValue();
        }

        public void OnChangeValue()
        {
            renderSetting = Renderer3D.ActiveRenderingPipeline.GetRenderSettings();

            renderSetting.useBloom = useBloom;
            renderSetting.useSSAO = useSSAO;
            renderSetting.Exposure = exposure;
            renderSetting.bloomIntensity = bloomIntensity;
            renderSetting.tonemapFunction = tonemapFunction;
            renderSetting.useIBL = useIBL;
            renderSetting.useAntialiasing = useAntialiasing;
            renderSetting.useSkybox = renderSkybox;
            renderSetting.MSAA = msaa_samples;
            renderSetting.AmbientColor = ambientColor;
            renderSetting.AmbientFactor = ambientFactor;
            renderSetting.occlusionScale = occlusion_scale;

            Renderer3D.ActiveRenderingPipeline.SetRenderSettings(renderSetting);
        }

        public override void OnUpdate(float dt)
        {

        }

        public override void OnDestroy()
        {

        }

    }
}
