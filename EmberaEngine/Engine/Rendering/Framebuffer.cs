using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using EmberaEngine.Engine.Core;

namespace EmberaEngine.Engine.Rendering
{
    public class Framebuffer
    {
        int rendererID;
        List<Texture> TextureAttachments = new();


        public Framebuffer()
        {
            GL.CreateFramebuffers(1, out rendererID);
        }

        public void Clear(ClearBufferMask mask)
        {
            GL.Clear(mask);
        }

        public int GetRendererID()
        {
            return rendererID;
        }

        public void AttachFramebufferTexture(FramebufferAttachment attachment, Texture tex, int layer = 0)
        {
            GL.NamedFramebufferTexture(rendererID, attachment, tex.GetRendererID(), layer);

            TextureAttachments.Add(tex);
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
            GL.BindFramebuffer(target, rendererID);
        }


        public static void Clear(int id, ClearBufferMask clearBufferMask, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            GL.BindFramebuffer(framebufferTarget, id);
            GL.Clear(clearBufferMask);
            GL.BindFramebuffer(framebufferTarget, 0);
        }

        public static void ClearColor(int id, (float,float,float,float) color, ClearBufferMask clearBufferMask, FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer)
        {
            GL.BindFramebuffer(framebufferTarget, id);
            GL.Clear(clearBufferMask);
            GL.ClearColor(color.Item1, color.Item2, color.Item3, color.Item4);
            GL.BindFramebuffer(framebufferTarget, 0);
        }

        public static void BlitFrameBuffer(Framebuffer buffer1, Framebuffer buffer2, (int,int,int,int)srcXY, (int,int,int,int)dstXY, ClearBufferMask mask, BlitFramebufferFilter filter)
        {
            GL.BlitNamedFramebuffer(buffer1.GetRendererID(), buffer2.GetRendererID(), srcXY.Item1, srcXY.Item2, srcXY.Item3, srcXY.Item4, dstXY.Item1, dstXY.Item2, dstXY.Item3, dstXY.Item4, mask, filter);
        }
    }
}
