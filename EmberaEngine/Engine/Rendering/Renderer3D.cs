using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{

    public interface IRenderPipeline
    {
        void Initialize(int width, int height);
        void BeginRender();
        void Render();
        void EndRender();

        void Resize(int width, int height);

        Framebuffer GetOutputFrameBuffer();

        Material GetDefaultMaterial();

        List<IRenderPass> GetPasses();
        RenderSetting GetRenderSettings();
        void SetRenderSettings(RenderSetting settings);
    }

    public interface IRenderPass
    {
        bool GetState();
        void SetState(bool value);

        void Initialize(int width, int height);
        void Resize(int width, int height);
        void Apply(FrameData frameData);
        Framebuffer GetOutputFramebuffer();
    }

    public class FrameData
    {
        public Camera Camera;
        public List<MeshEntry> Meshes;
        public Framebuffer GBuffer;
        public Framebuffer EffectFrameBuffer; // this is sort of confusing and must be changed to elsewhere or a better system
                                              // its just a way to send to the effect what framebuffer/texture you want as input.
                                              // i implemented this for bloom, as i had no other way to send a input texture.
        public Texture EffectTexture;
        public int selectedObjectCustombitflag;
    }

    public class MeshEntry
    {
        public Mesh Mesh;
        public Matrix4 Transform;
    }


    public class Renderer3D
    {

        public static Camera renderCamera;

        public static IRenderPipeline ActiveRenderingPipeline;

        static Texture MSCompositeBufferTexture;
        static Texture MSCompositeBufferEmissionTexture;
        static Texture MSDepthBufferTexture;

        static Texture CompositeBufferTexture;
        static Texture CompositeBufferEmissionTexture;
        static Texture DepthBufferTexture;

        static Framebuffer CompositeBuffer;
        static Framebuffer ResolvedBuffer;

        static List<MeshEntry> meshes;

        static FrameData frameData;

        static int numSamples = 8;
        static bool useMSAA = false;

        public static void Initialize(int width, int height)
        {
            Console.WriteLine("INITIALIZE CALLED");
            //cameras = new List<Camera>();
            meshes = new List<MeshEntry>();

            // Setting up composite buffer
            MSCompositeBufferTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSCompositeBufferTexture.TexImageMultisample2D(width, height, numSamples, PixelInternalFormat.Rgba16f, IntPtr.Zero);
            //MSCompositeBufferTexture.GenerateMipmap();

            MSCompositeBufferEmissionTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSCompositeBufferEmissionTexture.TexImageMultisample2D(width, height, numSamples, PixelInternalFormat.Rgba16f, IntPtr.Zero);

            MSDepthBufferTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSDepthBufferTexture.TexImageMultisample2D(width, height, numSamples, PixelInternalFormat.Depth24Stencil8, IntPtr.Zero);


            CompositeBuffer = new Framebuffer("Renderer3D_MSAA_Composite_Framebuffer");
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0,  OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample , MSCompositeBufferTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample , MSCompositeBufferEmissionTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSDepthBufferTexture);
            CompositeBuffer.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0, OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment1]);


            CompositeBufferTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            //CompositeBufferTexture.GenerateMipmap();

            CompositeBufferEmissionTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferEmissionTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferEmissionTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            //CompositeBufferEmissionTexture.GenerateMipmap();

            DepthBufferTexture = new Texture(TextureTarget2d.Texture2D);
            DepthBufferTexture.TexImage2D(width, height, PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            DepthBufferTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);


            ResolvedBuffer = new Framebuffer("Renderer3D_Composite_Framebuffer");
            ResolvedBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, CompositeBufferTexture);
            ResolvedBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1, CompositeBufferEmissionTexture);
            ResolvedBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, DepthBufferTexture);
            ResolvedBuffer.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0, OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment1]);

            frameData = new();

            LightManager.Initialize();
            MaterialManager.Initialize();
            
            ActiveRenderingPipeline.Initialize(width, height);
        }

        //public static void RegisterCamera(Camera camera)
        //{
        //    cameras.Add(camera);
        //    camera.rendererID = cameras.Count;
        //}

        //public static void RemoveCamera(Camera camera)
        //{
        //    cameras.Remove(camera);
        //}

        public static MeshEntry RegisterMesh(Mesh mesh)
        {
            MeshEntry entry = new MeshEntry()
            {
                Mesh = mesh,
                Transform = Matrix4.Identity
            };
            meshes.Add(entry);
            return entry;
        }

        public static void RemoveMesh(MeshEntry mesh)
        {
            meshes.Remove(mesh);
        }

        public static FrameData GetFrameData()
        {
            return frameData;
        }

        public static List<MeshEntry> GetMeshes()
        {
            return meshes;
        }

        public static void SetRenderCamera(Camera camera)
        {
            renderCamera = camera;
            ActiveRenderingPipeline.Resize(Renderer.Width, Renderer.Height);
        }

        public static Camera GetRenderCamera()
        {
            return renderCamera;

            //for (int i = 0; i < cameras.Count; i++)
            //{
            //    if (cameras[i].isDefault)
            //    {
            //        return cameras[i];
            //    }
            //}

            //return null;
        }

        public static void BeginRender()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            frameData.Camera = camera;
            frameData.Meshes = GetMeshes();

            LightManager.UpdateLights();
            ActiveRenderingPipeline.BeginRender();
        }

        public static void Render()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            ActiveRenderingPipeline.Render();
        }

        public static void EndRender()
        {
            Camera camera = GetRenderCamera();
            if (camera == null) return;

            ActiveRenderingPipeline.EndRender();
        }

        public static void ApplyPerFrameSettings(Camera camera)
        {
            GraphicsState.ClearColor(camera.ClearColor);
            GraphicsState.Clear(true, true);
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
            GraphicsState.SetCulling(true);
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void SetViewportDimensions()
        {
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
        }

        public static void Resize(int width, int height)
        {
            CompositeBuffer.DetachFrameBufferTexture(0);
            CompositeBuffer.DetachFrameBufferTexture(0);
            CompositeBuffer.DetachFrameBufferTexture(0);

            MSCompositeBufferTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSCompositeBufferTexture.TexImageMultisample2D(width, height, numSamples, PixelInternalFormat.Rgba16f, IntPtr.Zero);

            MSCompositeBufferEmissionTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSCompositeBufferEmissionTexture.TexImageMultisample2D(width, height, numSamples, PixelInternalFormat.Rgba16f, IntPtr.Zero);

            MSDepthBufferTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSDepthBufferTexture.TexImageMultisample2D(width, height, numSamples, PixelInternalFormat.Depth24Stencil8, IntPtr.Zero);


            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);

            CompositeBufferEmissionTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferEmissionTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            CompositeBufferEmissionTexture.SetWrapMode(TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge);

            DepthBufferTexture.TexImage2D(width, height, PixelInternalFormat.Depth24Stencil8, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            DepthBufferTexture.SetFilter(TextureMinFilter.Nearest, TextureMagFilter.Nearest);


            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSCompositeBufferTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSCompositeBufferEmissionTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSDepthBufferTexture);
            CompositeBuffer.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0, OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment1]);

            ActiveRenderingPipeline.Resize(width, height);
        }

        public static void SetMSAA(int samples)
        {
            if (samples == 1)
            {
                useMSAA = false;
                Console.WriteLine("Rendering without MSAA");
                return;
            } else
            {
                useMSAA = true;
            }
                numSamples = samples;

            CompositeBuffer.DetachFrameBufferTexture(0);
            CompositeBuffer.DetachFrameBufferTexture(0);
            CompositeBuffer.DetachFrameBufferTexture(0);

            MSCompositeBufferTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSCompositeBufferTexture.TexImageMultisample2D(Renderer.Width, Renderer.Height, numSamples, PixelInternalFormat.Rgba16f, IntPtr.Zero);

            MSCompositeBufferEmissionTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSCompositeBufferEmissionTexture.TexImageMultisample2D(Renderer.Width, Renderer.Height, numSamples, PixelInternalFormat.Rgba16f, IntPtr.Zero);

            MSDepthBufferTexture = new Texture(TextureTargetd.Texture2DMultisample);
            MSDepthBufferTexture.TexImageMultisample2D(Renderer.Width, Renderer.Height, numSamples, PixelInternalFormat.Depth24Stencil8, IntPtr.Zero);

            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSCompositeBufferTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment1, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSCompositeBufferEmissionTexture);
            CompositeBuffer.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.DepthStencilAttachment, OpenTK.Graphics.OpenGL.TextureTarget.Texture2DMultisample, MSDepthBufferTexture);
            CompositeBuffer.SetDrawBuffers([OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment0, OpenTK.Graphics.OpenGL.DrawBuffersEnum.ColorAttachment1]);

        }

        public static int GetMSAA()
        {
            return numSamples;
        }

        public static void ResolveCompositeMS()
        {
            if (!useMSAA)
            {
                return;
            }
            ResolvedBuffer.Bind();
            GraphicsState.SetDepthTest(true);
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);

            Framebuffer.BlitFrameBuffer(CompositeBuffer, ResolvedBuffer, (0, 0, Renderer.Width, Renderer.Height), (0, 0, Renderer.Width, Renderer.Height), OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest, OpenTK.Graphics.OpenGL.ReadBufferMode.ColorAttachment0, OpenTK.Graphics.OpenGL.DrawBufferMode.ColorAttachment0);
            Framebuffer.BlitFrameBuffer(CompositeBuffer, ResolvedBuffer, (0, 0, Renderer.Width, Renderer.Height), (0, 0, Renderer.Width, Renderer.Height), OpenTK.Graphics.OpenGL.ClearBufferMask.ColorBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest, OpenTK.Graphics.OpenGL.ReadBufferMode.ColorAttachment1, OpenTK.Graphics.OpenGL.DrawBufferMode.ColorAttachment1);
            Framebuffer.BlitFrameBuffer(CompositeBuffer, ResolvedBuffer, (0, 0, Renderer.Width, Renderer.Height), (0, 0, Renderer.Width, Renderer.Height), OpenTK.Graphics.OpenGL.ClearBufferMask.DepthBufferBit, OpenTK.Graphics.OpenGL.BlitFramebufferFilter.Nearest);
        }

        public static Framebuffer GetComposite()
        {
            return useMSAA ? CompositeBuffer : ResolvedBuffer;
        }

        public static Framebuffer GetResolved()
        {
            return ResolvedBuffer;
        }

        public static Framebuffer GetOutputFrameBuffer()
        {
            return ActiveRenderingPipeline.GetOutputFrameBuffer();
        }
    }
}
