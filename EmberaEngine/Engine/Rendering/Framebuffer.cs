using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using EmberaEngine.Engine.Core;
using System.Reflection.Metadata;

namespace EmberaEngine.Engine.Rendering
{
    public class Framebuffer : IDisposable
    {
        private bool _disposed;

        static int currentlyBound;
        
        public bool isBound { get { return currentlyBound == this.rendererID; }}

        string debugName;
        int rendererID;
        List<Texture> TextureAttachments = new();


        public Framebuffer(string debugName = "N/A")
        {
            this.debugName = debugName;
            GL.CreateFramebuffers(1, out rendererID);
        }

        public void Clear(ClearBufferMask mask, FramebufferTarget target = FramebufferTarget.Framebuffer)
        {
            GL.Clear(mask);
        }

        public int GetRendererID()
        {
            return rendererID;
        }

        public void AttachFramebufferTexture(FramebufferAttachment attachment, Texture tex, int level = 0)
        {
            GL.NamedFramebufferTexture(rendererID, attachment, tex.GetRendererID(), level);

            TextureAttachments.Add(tex);
        }

        public void AttachFramebufferTexture(FramebufferAttachment attachment, TextureTarget textureTarget, Texture tex, int level = 0)
        {
            Bind();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, textureTarget, tex.GetRendererID(), level);
            TextureAttachments.Add(tex);
        }

        public void AttachFramebufferTextureLayer(FramebufferAttachment attachment, Texture tex, int level = 0, int layer = 0)
        {
            Bind();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.TextureCubeMapPositiveX + layer, tex.GetRendererID(), level);
            TextureAttachments.Add(tex);
        }

        public void DetachFrameBufferTexture(int index)
        {
            TextureAttachments.RemoveAt(index);
        }

        public void SetFramebufferTexture(FramebufferAttachment attachment, Texture tex, int level = 0)
        {
            GL.NamedFramebufferTexture(rendererID, attachment, tex.GetRendererID(), level);
        }

        public void SetFramebufferTextureLayer(FramebufferAttachment attachment, Texture tex, int level = 0, int layer = 0)
        {
            Bind();
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, attachment, tex.GetRendererID(), level, layer);
        }



        public void SetDrawBuffers(ReadOnlySpan<DrawBuffersEnum> drawBuffers)
        {
            if (drawBuffers.Length > 0)
            {
                unsafe
                {
                    fixed (DrawBuffersEnum* ptr = drawBuffers)
                    {
                        GL.NamedFramebufferDrawBuffers(rendererID, drawBuffers.Length, ptr);
                    }
                }
            }
        }

        public Texture GetFramebufferTexture(int index)
        {
            return TextureAttachments[index];
        }

        public void SetParameter(FramebufferDefaultParameter fdp, int param)
        {
            GL.NamedFramebufferParameter(rendererID, fdp, param);
        }

        public void Bind(FramebufferTarget target = FramebufferTarget.Framebuffer)
        {
            if (Framebuffer.currentlyBound == rendererID) return;
            GL.BindFramebuffer(target, rendererID);

            Framebuffer.currentlyBound = this.rendererID;
        }



        public static void Clear(int id, ClearBufferMask clearBufferMask, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            GL.Clear(clearBufferMask);
        }

        public static void ClearColor(int id, (float,float,float,float) color, ClearBufferMask clearBufferMask, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            GL.Clear(clearBufferMask);
            GL.ClearColor(color.Item1, color.Item2, color.Item3, color.Item4);
        }

        public static void BlitFrameBuffer(
            Framebuffer src, Framebuffer dst,
            (int, int, int, int) srcXY,
            (int, int, int, int) dstXY,
            ClearBufferMask mask,
            BlitFramebufferFilter filter,
            ReadBufferMode readBufferMode = ReadBufferMode.ColorAttachment0,
            DrawBufferMode drawBufferMode = DrawBufferMode.ColorAttachment0)
        {
            // Set source read buffer
            GL.NamedFramebufferReadBuffer(src.GetRendererID(), readBufferMode);

            // Set destination draw buffer
            GL.NamedFramebufferDrawBuffer(dst.GetRendererID(), drawBufferMode);

            // Blit from src to dst
            GL.BlitNamedFramebuffer(
                src.GetRendererID(), dst.GetRendererID(),
                srcXY.Item1, srcXY.Item2, srcXY.Item3, srcXY.Item4,
                dstXY.Item1, dstXY.Item2, dstXY.Item3, dstXY.Item4,
                mask, filter
            );
        }


        public static void Unbind(FramebufferTarget target = FramebufferTarget.Framebuffer)
        {
            GL.BindFramebuffer(target, 0);
        }

        ~Framebuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;


            if (rendererID != 0)
            {
                int handleToDelete = rendererID;
                MainThreadDispatcher.Queue(() =>
                {
                    GL.DeleteFramebuffer(handleToDelete);
                });
                rendererID = 0;
            }

            _disposed = true;
        }
    }
}
