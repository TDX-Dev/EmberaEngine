using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        static int sizeX = 1024, sizeY = 1024;
        static Vector2i irradianceMapSize = new Vector2i(32, 32);


        static Vector2i prefilterMapSize = new Vector2i(1024, 1024);
        static int maxMipmapCount = 5;

        static Vector2i brdfLUTSize = new Vector2i(512, 512);


        static bool convertHDRItoCubemap = false;

        static Matrix4 irradianceProjection;
        static Matrix4[] irradianceViewMatrices;

        public static void LoadCubemap()
        {

        }
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
            {
                skyboxTexture.TexImage2D(sizeX, sizeY, OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMapPositiveX + i, PixelInternalFormat.Rgb32f, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            }
            skyboxTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            skyboxTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);


            irradianceTexture = new Texture(TextureTarget2d.TextureCubeMap);

            for (int i = 0; i < 6; i++)
            {
                irradianceTexture.TexImage2D(irradianceMapSize.X, irradianceMapSize.Y, OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMapPositiveX + i, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            }
            irradianceTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            irradianceTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            preFilterTexture = new Texture(TextureTarget2d.TextureCubeMap);

            for (int mip = 0; mip < maxMipmapCount; mip++)
            {
                int mipWidth = (int)(prefilterMapSize.X * MathF.Pow(0.5f, mip));
                int mipHeight = (int)(prefilterMapSize.Y * MathF.Pow(0.5f, mip));
                for (int i = 0; i < 6; i++)
                {
                    preFilterTexture.TexImage2D(mipWidth, mipHeight, OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMapPositiveX + i, PixelInternalFormat.Rgb16f, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                }
            }

            preFilterTexture.SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            preFilterTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);
            preFilterTexture.GenerateMipmap();

            brdfLUTTexture = new Texture(TextureTarget2d.Texture2D);
            brdfLUTTexture.TexImage2D(brdfLUTSize.X, brdfLUTSize.Y, PixelInternalFormat.Rg16f, PixelFormat.Rg, PixelType.Float, IntPtr.Zero);
            brdfLUTTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            brdfLUTTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);


            cubemapCreationFB = new Framebuffer("Cubemap Creation FB");
            irradianceMapFB = new Framebuffer("Irradiance Map FB");
            prefilterMapFB = new Framebuffer("Prefilter Map FB");

            // temporary
            skyboxShader = new Shader("Engine/Content/Shaders/3D/basic/skybox");
            panoramaConvertShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert","Engine/Content/Shaders/3D/basic/panoramicToCubemap.frag");
            irradianceMapShader = new Shader("Engine/Content/Shaders/3D/basic/skybox.vert", "Engine/Content/Shaders/3D/basic/irradianceMap.frag");
            preFilterMapShader = new Shader("Engine/Content/Shaders/3D/basic/skybox.vert", "Engine/Content/Shaders/3D/basic/prefilterMap.frag");
            brdfLUTShader = new Shader("Engine/Content/Shaders/3D/basic/fullscreen.vert", "Engine/Content/Shaders/3D/basic/brdfLUT.frag");

            // Setting up irradiance map view matrices

            irradianceProjection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), 1.0f, 0.1f, 10.0f);
            irradianceViewMatrices =
            [
                Matrix4.LookAt(Vector3.Zero, new Vector3(1.0f,  0.0f,  0.0f), -Vector3.UnitY), // Right
                Matrix4.LookAt(Vector3.Zero, new Vector3(-1.0f,  0.0f,  0.0f), -Vector3.UnitY), // Left
                Matrix4.LookAt(Vector3.Zero, new Vector3( 0.0f,  1.0f,  0.0f),  Vector3.UnitZ),  // Up
                Matrix4.LookAt(Vector3.Zero, new Vector3( 0.0f, -1.0f,  0.0f), -Vector3.UnitZ),  // Down
                Matrix4.LookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f,  1.0f), -Vector3.UnitY),  // Forward
                Matrix4.LookAt(Vector3.Zero, new Vector3( 0.0f,  0.0f, -1.0f), -Vector3.UnitY),  // Backward
            ];

            GraphicsState.SetCubemapSeamless(true);
        }

        static int frameCounter = 0;
        public static void Render()
        {
            frameCounter++;
            if (convertHDRItoCubemap && frameCounter > 0)
            {
                GraphicsState.SetCulling(false);
                GraphicsState.SetDepthTest(false);
                GraphicsState.SetBlending(false);
                //GraphicsState.SetCubemapSeamless(true);

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

            // Clear before drawing skybox
            //GraphicsState.Clear(true, true);

            // Use proper depth testing (test yes, write no)
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetDepthMask(false);
            GraphicsState.SetCulling(false);

            skyboxShader.Use();
            skyboxShader.SetMatrix4("W_PROJECTION_MATRIX", camera.GetProjectionMatrix());
            skyboxShader.SetMatrix4("W_VIEW_MATRIX", new Matrix4(new Matrix3(camera.GetViewMatrix())));
            skyboxShader.SetInt("SKYBOX_TEXTURE", 0);
            skyboxTexture.SetActiveUnit(TextureUnit.Texture0);
            skyboxTexture.Bind(OpenTK.Graphics.OpenGL.TextureTarget.TextureCubeMap);

            //CubeMesh.Draw();

            GraphicsState.SetDepthTest(true);
            GraphicsState.SetDepthMask(true);
            GraphicsState.SetCulling(true);
        }


        public static Texture GetIrradianceMap()
        {
            return irradianceTexture;
        }

        public static Texture GetPreFilterMap()
        {
            return preFilterTexture;
        }

        public static Texture GetBRDFLUT()
        {
            return brdfLUTTexture;
        }

        static void GenerateBRDFLUT()
        {
            OpenTK.Graphics.OpenGL.FramebufferErrorCode status = OpenTK.Graphics.OpenGL.GL.CheckFramebufferStatus(OpenTK.Graphics.OpenGL.FramebufferTarget.Framebuffer);
            if (status != OpenTK.Graphics.OpenGL.FramebufferErrorCode.FramebufferComplete)
            {
                Console.WriteLine($"[SkyboxManager] FBO incomplete: {status}");
            }

            Console.WriteLine("BRDF ABOVE");


            prefilterMapFB.Bind();
            GraphicsState.Clear(true, true);

            prefilterMapFB.SetFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, brdfLUTTexture);

            GraphicsState.SetViewport(0, 0, brdfLUTSize.X, brdfLUTSize.Y);

            brdfLUTShader.Use();
            GraphicsState.Clear(true);

            Graphics.DrawFullScreenTri();

        }

        static void GeneratePreFilterMap()
        {
            prefilterMapFB.Bind();
            GraphicsState.Clear(true, true);

            GraphicsState.SetViewport(0, 0, prefilterMapSize.X, prefilterMapSize.Y);

            preFilterMapShader.Use();
            preFilterMapShader.SetInt("SKYBOX_TEXTURE", 0);
            preFilterMapShader.SetMatrix4("W_PROJECTION_MATRIX", irradianceProjection);

            skyboxTexture.SetActiveUnit(TextureUnit.Texture0);
            skyboxTexture.Bind();

            for (int mip = 0; mip < maxMipmapCount; ++mip)
            {
                Vector2 mipSize = new Vector2((prefilterMapSize.X * (float)Math.Pow(0.5f, mip)), (prefilterMapSize.Y * (float)Math.Pow(0.5f, mip)));

                GraphicsState.SetViewport(0, 0, (int)mipSize.X, (int)mipSize.Y);

                float roughness = (float)mip / (float)(maxMipmapCount - 1);
                preFilterMapShader.SetFloat("roughness", roughness);

                for (int i = 0; i < 6; ++i)
                {
                    preFilterMapShader.SetMatrix4("W_VIEW_MATRIX", irradianceViewMatrices[i]);
                    prefilterMapFB.SetFramebufferTextureLayer(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, preFilterTexture, mip, i);
                    OpenTK.Graphics.OpenGL.FramebufferErrorCode status = OpenTK.Graphics.OpenGL.GL.CheckFramebufferStatus(OpenTK.Graphics.OpenGL.FramebufferTarget.Framebuffer);
                    if (status != OpenTK.Graphics.OpenGL.FramebufferErrorCode.FramebufferComplete)
                    {
                        Console.WriteLine($"[SkyboxManager] FBO incomplete: {status}");
                    }

                    Console.WriteLine("PREFILTER ABOVE");

                    CubeMesh.Draw();
                }


            }

            preFilterTexture.GenerateMipmap();

            Renderer3D.SetViewportDimensions();
        }

        static void GenerateIrradianceMap()
        {
            irradianceMapFB.Bind();
            GraphicsState.Clear(true, true);

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
                OpenTK.Graphics.OpenGL.FramebufferErrorCode status = OpenTK.Graphics.OpenGL.GL.CheckFramebufferStatus(OpenTK.Graphics.OpenGL.FramebufferTarget.Framebuffer);
                if (status != OpenTK.Graphics.OpenGL.FramebufferErrorCode.FramebufferComplete)
                {
                    Console.WriteLine($"[SkyboxManager] FBO incomplete: {status}");
                }

                Console.WriteLine("irradiance ABOVE");
                CubeMesh.Draw();
            }

            irradianceTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);

            Renderer3D.SetViewportDimensions();

        }

        static void ConvertHDRIToCubemap()
        {
            cubemapCreationFB.Bind();
            GraphicsState.Clear(true, true);

            GraphicsState.SetViewport(0, 0, sizeX, sizeY);
            panoramaConvertShader.Use();
            for (int i = 0; i < 6; i++)
            {
                cubemapCreationFB.SetFramebufferTextureLayer(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, skyboxTexture, 0, i);
                OpenTK.Graphics.OpenGL.FramebufferErrorCode status = OpenTK.Graphics.OpenGL.GL.CheckFramebufferStatus(OpenTK.Graphics.OpenGL.FramebufferTarget.Framebuffer);
                if (status != OpenTK.Graphics.OpenGL.FramebufferErrorCode.FramebufferComplete)
                {
                    Console.WriteLine($"[SkyboxManager] FBO incomplete: {status}");
                }

                Console.WriteLine("HDRI ABOVE");
                panoramaConvertShader.SetInt("face", i);
                panoramaConvertShader.SetInt("panoramicTexture", 0);
                GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
                loadedTexture.Bind();

                Graphics.DrawFullScreenTri();
            }
            skyboxTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            skyboxTexture.GenerateMipmap();

            GraphicsState.CheckFBError();

            Framebuffer.Unbind();
            Renderer3D.SetViewportDimensions();
        }

    }
}
