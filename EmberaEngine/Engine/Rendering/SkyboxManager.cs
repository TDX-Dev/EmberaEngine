using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;

namespace EmberaEngine.Engine.Rendering
{
    public static class SkyboxManager
    {
        static Texture skyboxTexture;
        static Texture irradianceTexture;
        static Texture preFilterTexture;
        static Texture brdfLUTTexture;

        static Shader skyboxShader;
        static Shader panoramaConvertShader;
        static Shader irradianceMapShader;
        static Shader preFilterMapShader;
        static Shader brdfLUTShader;

        static Mesh CubeMesh;
        static Texture loadedTexture;

        static Framebuffer cubemapCreationFB;
        static Framebuffer irradianceMapFB;
        static Framebuffer prefilterMapFB;
        static Framebuffer brdfLUTFB;

        static int sizeX = 1024, sizeY = 1024;
        static Vector2i irradianceMapSize = new Vector2i(32, 32);
        static Vector2i prefilterMapSize = new Vector2i(1024, 1024);
        static int maxMipmapCount = 5;
        static Vector2i brdfLUTSize = new Vector2i(512, 512);

        static bool convertHDRItoCubemap = false;

        static Matrix4 irradianceProjection;
        static Matrix4[] irradianceViewMatrices;

        public static void LoadHDRI(Texture texture)
        {
            loadedTexture = texture;
            convertHDRItoCubemap = true;
        }

        public static void Initialize()
        {
            CubeMesh = Graphics.GetCube();

            skyboxTexture = new Texture(TextureTarget2d.TextureCubeMap);
            for (int i = 0; i < 6; i++)
                skyboxTexture.TexImage2D(
                    sizeX, sizeY,
                    OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMapPositiveX + i,
                    PixelInternalFormat.Rgb32f,
                    PixelFormat.Rgb,
                    PixelType.Float,
                    IntPtr.Zero
                );
            skyboxTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear); // No mipmap filtering

            // Irradiance
            irradianceTexture = new Texture(TextureTarget2d.TextureCubeMap);
            for (int i = 0; i < 6; i++)
                irradianceTexture.TexImage2D(irradianceMapSize.X, irradianceMapSize.Y, OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMapPositiveX + i, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            irradianceTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            irradianceTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            // Create a cubemap texture for prefiltering
            preFilterTexture = new Texture(TextureTarget2d.TextureCubeMap);
            preFilterTexture.Bind();

            // Allocate immutable storage for all 6 faces with all mip levels
            for (int i = 0; i < 6; i++)
            {
                preFilterTexture.TexStorage2D(
                    prefilterMapSize.X, prefilterMapSize.Y,
                    maxMipmapCount,
                    TextureTarget2d.TextureCubeMap,
                    SizedInternalFormat.Rgb16f
                );

            }

            // Set filtering and wrapping
            preFilterTexture.SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            preFilterTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            preFilterTexture.SetBaseLevel(0);
            preFilterTexture.SetMaxLevel(maxMipmapCount - 1);

            // BRDF LUT
            brdfLUTTexture = new Texture(TextureTarget2d.Texture2D);
            brdfLUTTexture.TexImage2D(brdfLUTSize.X, brdfLUTSize.Y, PixelInternalFormat.Rg16f, PixelFormat.Rg, PixelType.Float, IntPtr.Zero);
            brdfLUTTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            brdfLUTTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            // Framebuffers
            cubemapCreationFB = new Framebuffer("Cubemap Creation FB");
            irradianceMapFB = new Framebuffer("Irradiance Map FB");
            prefilterMapFB = new Framebuffer("Prefilter Map FB");
            brdfLUTFB = new Framebuffer("BRDF LUT FB");

            // Shaders
            skyboxShader = new Shader("Engine/Content/Shaders/3D/basic/skybox");
            panoramaConvertShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Engine/Content/Shaders/3D/basic/panoramicToCubemap.frag");
            irradianceMapShader = new Shader("Engine/Content/Shaders/3D/basic/skybox.vert", "Engine/Content/Shaders/3D/basic/irradianceMap.frag");
            preFilterMapShader = new Shader("Engine/Content/Shaders/3D/basic/skybox.vert", "Engine/Content/Shaders/3D/basic/prefilterMap.frag");
            brdfLUTShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Engine/Content/Shaders/3D/basic/brdfLUT.frag");

            irradianceProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, 0.1f, 10.0f);
            irradianceViewMatrices = new[]
            {
                Matrix4.LookAt(Vector3.Zero, Vector3.UnitX, -Vector3.UnitY),
                Matrix4.LookAt(Vector3.Zero, -Vector3.UnitX, -Vector3.UnitY),
                Matrix4.LookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ),
                Matrix4.LookAt(Vector3.Zero, -Vector3.UnitY, -Vector3.UnitZ),
                Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, -Vector3.UnitY),
                Matrix4.LookAt(Vector3.Zero, -Vector3.UnitZ, -Vector3.UnitY)
            };

            GraphicsState.SetCubemapSeamless(true);
        }

        public static void Render()
        {
            if (convertHDRItoCubemap)
            {
                GraphicsState.SetCulling(false);
                GraphicsState.SetDepthTest(false);
                GraphicsState.SetBlending(false);

                ConvertHDRIToCubemap();
                GenerateIrradianceMap();
                GeneratePreFilterMap();
                GenerateBRDFLUT();

                GraphicsState.SetCulling(true);
                GraphicsState.SetDepthTest(true);
                GraphicsState.SetBlending(true);

                convertHDRItoCubemap = false;
            }
        }

        public static void RenderCube()
        {
            Camera camera = Renderer3D.GetRenderCamera();

            GraphicsState.SetDepthTest(true);
            GraphicsState.SetDepthMask(false); // disable depth writes
            GraphicsState.SetCulling(false);

            skyboxShader.Use();
            skyboxShader.SetMatrix4("W_PROJECTION_MATRIX", camera.GetProjectionMatrix());
            skyboxShader.SetMatrix4("W_VIEW_MATRIX", new Matrix4(new Matrix3(camera.GetViewMatrix())));
            skyboxShader.SetInt("SKYBOX_TEXTURE", 0);

            skyboxTexture.SetActiveUnit(TextureUnit.Texture0);
            skyboxTexture.Bind(OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMap);

            CubeMesh.Draw();

            GraphicsState.SetDepthMask(true); // re-enable depth writes
            GraphicsState.SetCulling(true);
        }


        static void ConvertHDRIToCubemap()
        {
            Console.WriteLine("HDRI START");
            cubemapCreationFB.Bind();
            GraphicsState.SetViewport(0, 0, sizeX, sizeY);
            panoramaConvertShader.Use();

            for (int i = 0; i < 6; i++)
            {
                cubemapCreationFB.SetFramebufferTextureLayer(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, skyboxTexture, 0, i);
                panoramaConvertShader.SetInt("face", i);
                panoramaConvertShader.SetInt("panoramicTexture", 0);
                GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
                loadedTexture.Bind();

                Graphics.DrawFullScreenTri();
            }

            Framebuffer.Unbind();
            Renderer3D.SetViewportDimensions();
            Console.WriteLine("HDRI END");
        }

        static void GenerateIrradianceMap()
        {
            Console.WriteLine("IRRADIANCE START");
            irradianceMapFB.Bind();
            GraphicsState.SetViewport(0, 0, irradianceMapSize.X, irradianceMapSize.Y);
            irradianceMapShader.Use();
            irradianceMapShader.SetMatrix4("W_PROJECTION_MATRIX", irradianceProjection);
            irradianceMapShader.SetInt("SKYBOX_TEXTURE", 0);
            skyboxTexture.SetActiveUnit(TextureUnit.Texture0);
            skyboxTexture.Bind();

            for (int i = 0; i < 6; i++)
            {
                irradianceMapShader.SetMatrix4("W_VIEW_MATRIX", irradianceViewMatrices[i]);
                irradianceMapFB.SetFramebufferTextureLayer(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, irradianceTexture, 0, i);
                CubeMesh.Draw();
            }

            Framebuffer.Unbind();
            Renderer3D.SetViewportDimensions();
            Console.WriteLine("IRRADIANCE END");
        }

        static void GeneratePreFilterMap()
        {
            Console.WriteLine("PREFILTER START");
            prefilterMapFB.Bind();
            preFilterMapShader.Use();
            preFilterMapShader.SetInt("SKYBOX_TEXTURE", 0);
            preFilterMapShader.SetMatrix4("W_PROJECTION_MATRIX", irradianceProjection);
            skyboxTexture.SetActiveUnit(TextureUnit.Texture0);
            skyboxTexture.Bind();

            for (int mip = 0; mip < maxMipmapCount; mip++)
            {
                int mipWidth = (int)(prefilterMapSize.X * MathF.Pow(0.5f, mip));
                int mipHeight = (int)(prefilterMapSize.Y * MathF.Pow(0.5f, mip));
                GraphicsState.SetViewport(0, 0, mipWidth, mipHeight);

                float roughness = mip / (float)(maxMipmapCount - 1);
                preFilterMapShader.SetFloat("roughness", roughness);

                for (int i = 0; i < 6; i++)
                {
                    preFilterMapShader.SetMatrix4("W_VIEW_MATRIX", irradianceViewMatrices[i]);
                    prefilterMapFB.SetFramebufferTextureLayer(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, preFilterTexture, mip, i);
                    CubeMesh.Draw();
                }
            }

            Framebuffer.Unbind();
            Renderer3D.SetViewportDimensions();

            Console.WriteLine("PREFILTER END");
        }

        static void GenerateBRDFLUT()
        {
            brdfLUTFB.Bind();
            brdfLUTFB.SetFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, brdfLUTTexture);
            GraphicsState.SetViewport(0, 0, brdfLUTSize.X, brdfLUTSize.Y);
            brdfLUTShader.Use();
            GraphicsState.Clear(true, true);
            Graphics.DrawFullScreenTri();
            Framebuffer.Unbind();
        }

        public static Texture GetIrradianceMap() => irradianceTexture;
        public static Texture GetPreFilterMap() => preFilterTexture;
        public static Texture GetBRDFLUT() => brdfLUTTexture;
    }
}
