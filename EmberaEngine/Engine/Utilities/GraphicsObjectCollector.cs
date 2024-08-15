using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace EmberaEngine.Engine.Utilities
{
    static class GraphicsObjectCollector
    {

        static List<int> d_VertexArrays = new List<int>();

        static List<int> d_VertexBuffers = new List<int>();

        static List<int> d_FrameBuffers = new List<int>();

        static List<int> d_Textures = new List<int>();

        public static void AddVAOToDispose(int VAO)
        {
#if DEBUG
            //Console.WriteLine("DISPOSING VAO");
#endif
            d_VertexArrays.Add(VAO);
        }

        public static void AddVBOToDispose(int VBO)
        {
#if DEBUG
            //Console.WriteLine("DISPOSING VBO");
#endif
            d_VertexBuffers.Add(VBO);
        }

        public static void AddFBToDispose(int FBO)
        {
#if DEBUG
            //Console.WriteLine("DISPOSING FBO");
#endif
            d_FrameBuffers.Add(FBO);
        }

        public static void AddTexToDispose(int TO)
        {
#if DEBUG
            //Console.WriteLine("DISPOSING TEX OBJ");
#endif
            d_Textures.Add(TO);
        }

        public static void Dispose()
        {
            for (int i = 0; i < d_VertexArrays.Count; i++)
            {
                GL.BindVertexArray(0);
                GL.DeleteVertexArray(d_VertexArrays[i]);
            }
            for (int i = 0; i < d_VertexBuffers.Count; i++)
            {
                GL.DeleteBuffer(d_VertexBuffers[i]);
            }
            for (int i = 0; i < d_FrameBuffers.Count; i++)
            {
                GL.DeleteFramebuffer(d_FrameBuffers[i]);
            }
            for (int i = 0; i < d_Textures.Count; i++)
            {
                GL.DeleteTexture(d_Textures[i]);
            }
            d_VertexArrays.Clear();
            d_VertexBuffers.Clear();
            d_FrameBuffers.Clear();
            d_Textures.Clear();
        }
    }
}
