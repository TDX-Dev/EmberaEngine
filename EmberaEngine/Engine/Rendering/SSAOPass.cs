using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.Rendering
{
    internal class SSAOPass : IRenderPass
    {
        Framebuffer SSAOFB;
        Framebuffer SSAOBlurFB;

        Texture SSAOTexture;
        Texture SSAOBlurTexture;
        Texture NoiseTexture;

        Shader SSAOShader;
        Shader SSAOBlurShader;

        int SampleKernelSize = 64;
        Vector3[] SampleKernelValues;

        Vector2 screenDimensions;
        float renderScale = 0.25f;
        bool isActive = true;

        public bool GetState() => isActive;
        public void SetState(bool value) => isActive = value;

        public void Initialize(int width, int height)
        {
            screenDimensions = new Vector2(width, height);
            Vector3[] RandomizedNoise = Helper.GenerateNoise(16);
            SampleKernelValues = GenerateKernel(SampleKernelSize).ToArray();

            NoiseTexture = new Texture(TextureTarget2d.Texture2D);
            NoiseTexture.TexImage2D(4, 4, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float, RandomizedNoise);
            NoiseTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            NoiseTexture.SetWrapMode(TextureWrapMode.Repeat, TextureWrapMode.Repeat);
            NoiseTexture.GenerateMipmap();

            SSAOTexture = new Texture();
            SSAOTexture.TexImage2D((int)(screenDimensions.X * renderScale), (int)(screenDimensions.Y * renderScale), PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            SSAOBlurTexture = new Texture();
            SSAOBlurTexture.TexImage2D((int)(screenDimensions.X * renderScale), (int)(screenDimensions.Y * renderScale), PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOBlurTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);

            SSAOFB = new Framebuffer("SSAO Framebuffer");
            SSAOFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, SSAOTexture);
            SSAOFB.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0]);

            SSAOBlurFB = new Framebuffer("SSAO Blur Framebuffer");
            SSAOBlurFB.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, SSAOBlurTexture);
            SSAOBlurFB.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0]);

            SSAOShader = new Shader("Engine/Content/Shaders/3D/AO/ssao");

            for (int i = 0; i < SampleKernelSize; i++)
                SSAOShader.Set($"samples[{i}]", SampleKernelValues[i]);

            SSAOBlurShader = new Shader("Engine/Content/Shaders/3D/AO/ssao.vert", "Engine/Content/Shaders/3D/AO/ssaoBlur.frag");
        }

        public void Apply(FrameData frameData)
        {
            if (!isActive) return;

            float desiredScale = (int)Renderer3D.ActiveRenderingPipeline.GetRenderSettings().occlusionScale * 0.25f;
            if (Math.Abs(renderScale - desiredScale) > 0.001f)
            {
                renderScale = desiredScale;
                Resize((int)screenDimensions.X, (int)screenDimensions.Y);
            }

            SSAOFB.Bind();
            GraphicsState.SetViewport(0, 0, (int)(screenDimensions.X * renderScale), (int)(screenDimensions.Y * renderScale));
            GraphicsState.Clear(true, true);
            GraphicsState.SetCulling(false);
            GraphicsState.SetBlending(false);

            SSAOShader.Use();
            SSAOShader.Set("W_PROJECTION_MATRIX", frameData.Camera.GetProjectionMatrix());
            SSAOShader.Set("W_INVERSE_VIEW_MATRIX", Matrix4.Invert(frameData.Camera.GetViewMatrix()));
            SSAOShader.Set("W_VIEW_MATRIX", frameData.Camera.GetViewMatrix());
            SSAOShader.Set("gPosition", 0);
            SSAOShader.Set("gNormal", 1);
            SSAOShader.Set("texNoise", 2);
            SSAOShader.Set("gDepth", 3);
            SSAOShader.Set("screenDimensions", screenDimensions * renderScale);

            for (int i = 0; i < SampleKernelSize; i++)
                SSAOShader.Set($"samples[{i}]", SampleKernelValues[i]);

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            frameData.GBuffer.GetFramebufferTexture(1).Bind(); // position

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture1);
            frameData.GBuffer.GetFramebufferTexture(0).Bind(); // normal

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture2);
            NoiseTexture.Bind();

            Graphics.DrawFullScreenTri();

            // SSAO Blur pass
            SSAOBlurFB.Bind();
            GraphicsState.SetViewport(0, 0, (int)(screenDimensions.X * renderScale), (int)(screenDimensions.Y * renderScale));
            GraphicsState.Clear(true, true);

            SSAOBlurShader.Use();
            SSAOBlurShader.Set("INPUT_TEXTURE", 0);
            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            SSAOTexture.Bind();

            Graphics.DrawFullScreenTri();
        }

        static float ourLerp(float a, float b, float f) => a + f * (b - a);

        public static List<Vector3> GenerateKernel(int kernelSize = 64)
        {
            var ssaoKernel = new List<Vector3>();
            var random = new Random();

            for (int i = 0; i < kernelSize; ++i)
            {
                Vector3 sample = new Vector3(
                    (float)(random.NextDouble() * 2.0 - 1.0),
                    (float)(random.NextDouble() * 2.0 - 1.0),
                    (float)random.NextDouble()
                );

                sample = Vector3.Normalize(sample);
                sample *= (float)random.NextDouble();

                float scale = ourLerp(0.1f, 1.0f, ((float)i / kernelSize) * ((float)i / kernelSize));
                sample *= scale;

                ssaoKernel.Add(sample);
            }

            return ssaoKernel;
        }

        public Framebuffer GetOutputFramebuffer() => SSAOBlurFB;

        public void Resize(int width, int height)
        {
            screenDimensions = new Vector2(width, height);
            //SSAOShader.Set("screenDimensions", screenDimensions * renderScale);

            SSAOTexture.TexImage2D((int)(screenDimensions.X * renderScale), (int)(screenDimensions.Y * renderScale), PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            SSAOBlurTexture.TexImage2D((int)(screenDimensions.X * renderScale), (int)(screenDimensions.Y * renderScale), PixelInternalFormat.R16f, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            SSAOBlurTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        }
    }
}
