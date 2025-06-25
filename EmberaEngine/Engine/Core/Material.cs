using EmberaEngine.Engine.Rendering;
using MessagePack;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    [Flags]
    public enum MaterialFeatures
    {
        None = 0,
        EnvironmentLighting = 1 << 0,
        Shadows = 1 << 1,
    }


    public interface IMaterial
    {
        [IgnoreMember]
        Shader shader { get; }
        void Apply();
    }
    public abstract class Material : IMaterial
    {
        public Guid Id = Guid.NewGuid();
        public Shader shader { get; set; }
        public Dictionary<string, object> properties = new Dictionary<string, object>();

        public MaterialFeatures features;
        public int textureUnitCount;

        public void SetProperty(string name, object value) => properties[name] = value;

        public virtual void Apply()
        {
            shader.Use();

            foreach (KeyValuePair<string, object> kvp in properties)
            {
                shader.Set(kvp.Key, kvp.Value);
            }
        }
    }


    public class PBRMaterial : Material
    {
        private Vector4 albedo;
        private Vector3 emissionColor;
        private float emissionStrength;
        private float metallic;
        private float roughness;

        private Texture diffuseTexture;
        private Texture normalTexture;
        private Texture roughnessTexture;
        private Texture emissionTexture;

        public bool isDiffuseSet;
        public bool isNormalSet;
        public bool isSpecularSet;
        public bool isRoughnessSet;
        public bool isEmissionSet;

        #region PROPERTIES
        public Color4 Albedo
        {
            get => new Color4(albedo.X, albedo.Y, albedo.Z, albedo.W);
            set
            {
                albedo = new Vector4(value.R, value.G, value.B, value.A);
                OnChangeValue();
            }
        }

        public Color4 Emission
        {
            get => new Color4(emissionColor.X , emissionColor.Y, emissionColor.Z, 1);
            set
            {
                emissionColor = new Vector3(value.R, value.G, value.B);
                OnChangeValue();
            }
        }

        public float EmissionStrength
        {
            get => emissionStrength;
            set
            {
                emissionStrength = value;
                OnChangeValue();
            }
        }


        public float Metallic
        {
            get => metallic;
            set
            {
                metallic = value;
                OnChangeValue();
            }
        }


        public float Roughness
        {
            get => roughness;
            set
            {
                roughness = value;
                OnChangeValue();
            }
        }

        public Texture DiffuseTexture
        {
            get => diffuseTexture;
            set
            {
                diffuseTexture = value;
                isDiffuseSet = true;
                OnChangeValue();
            }
        }

        public Texture NormalTexture
        {
            get => normalTexture;
            set
            {
                normalTexture = value;
                isNormalSet = true;
                OnChangeValue();
            }
        }

        public Texture RoughnessTexture
        {
            get => roughnessTexture;
            set
            {
                roughnessTexture = value;
                isRoughnessSet = true;
                OnChangeValue();
            }
        }

        public Texture EmissionTexture
        {
            get => emissionTexture;
            set
            {
                emissionTexture = value;
                isEmissionSet = true;
                OnChangeValue();
            }
        }
        #endregion

        public PBRMaterial()
        {
            base.shader = ShaderRegistry.GetShader("CLUSTERED_PBR");
        }

        public PBRMaterial(Shader shader)
        {
            base.shader = shader;
        }

        public void SetDefaults()
        {
            Albedo = Color4.White;
            Emission = Color4.White;
            EmissionStrength = 0f;
            Metallic = 0f;
            Roughness = 1f;
            isDiffuseSet = false;
            isEmissionSet = false;
            isNormalSet = false;
            isRoughnessSet = false;

            //DiffuseTexture = Texture.White2DTex;
            //NormalTexture = Texture.White2DTex;
            //EmissionTexture = Texture.Black2DTex;
            //RoughnessTexture = Texture.White2DTex;

            textureUnitCount = 4;


            features = MaterialFeatures.EnvironmentLighting | MaterialFeatures.Shadows;
        }

        public override void Apply()
        {
            base.Apply();

            shader.Set("material.DIFFUSE_TEX", 0);
            shader.Set("material.NORMAL_TEX", 1);
            shader.Set("material.ROUGHNESS_TEX", 2);
            shader.Set("material.EMISSION_TEX", 3);

            shader.Set("material.useDiffuseMap", isDiffuseSet ? 1 : 0);
            shader.Set("material.useNormalMap", isNormalSet ? 1 : 0);
            shader.Set("material.useRoughnessMap", isRoughnessSet ? 1 : 0);
            shader.Set("material.useEmissionMap", isEmissionSet ? 1 : 0);

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            if (isDiffuseSet)
            {
                diffuseTexture.Bind();
            } else
            {
                Texture.White2DTex.Bind();
            }

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture1);
            if (isNormalSet)
            {
                normalTexture.Bind();
            } else
            {
                Texture.White2DTex.Bind();
            }

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture2);
            if (isRoughnessSet)
            {
                roughnessTexture.Bind();
            } else
            {
                Texture.White2DTex.Bind();
            }

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture3);
            if (isEmissionSet)
            {
                emissionTexture.Bind();
            } else
            {
                Texture.White2DTex.Bind();
            }
        }

        public void OnChangeValue()
        {
            SetProperty("material.albedo", albedo);
            SetProperty("material.emission", emissionColor);
            SetProperty("material.emissionStr", emissionStrength);
            SetProperty("material.metallic", metallic);
            SetProperty("material.roughness", roughness);

        }

    }
}
