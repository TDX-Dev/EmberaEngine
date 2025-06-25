using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public enum TonemapFunction
    {
        ACES = 0,
        Filmic = 1,
        Reinhard = 2
    }

    public enum MSAA_Samples
    {
        Disabled = 1,
        X2 = 2,
        X4 = 4,
        X8 = 8,
        X16 = 16
    }

    public enum AmbientOcclusionScale
    {
        Low = 1, // 0.25
        Medium = 2, // 0.5
        High = 4, // 1
        Ultra = 8 // 2
    }

    public enum RenderMode
    {
        Solid,
        Wireframe,
        Unlit
    }


    public struct RenderSetting
    {
        public float Exposure;
        public float bloomIntensity;
        public bool useBloom;
        public bool useSSAO;
        public bool useAntialiasing;
        public bool useSkybox;
        public bool useIBL;
        public bool useShadows;
        public Color4 AmbientColor;
        public float AmbientFactor;
        public TonemapFunction tonemapFunction;
        public MSAA_Samples MSAA;
        public AmbientOcclusionScale occlusionScale;
        public RenderMode renderMode;
    }
}
