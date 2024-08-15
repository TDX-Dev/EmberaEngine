using System;
using System.Collections.Generic;

using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;

namespace EmberaEngine.Engine.Utilities
{
    public class Mesh : IDisposable
    {

        public static int TotalMeshCount = 0;

        public VertexBuffer VBO;
        public IndexBuffer IBO;
        public VertexArray VAO;

        Vertex[] Vertices;

        public int MaterialIndex;

        public int MeshID;
        public int VertexCount;
        public string path;
        public string name;
        public string fileID;
        public bool Renderable = true;
        

        bool IsStatic = true;

        bool isdisposed = false;

        public Mesh()
        {
            TotalMeshCount += 1;
            MeshID = UtilRandom.Next(RenderGraph.MAX_MESH_COUNT);
        }

        ~Mesh()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isdisposed) { return; }
            TotalMeshCount -= 1;
            VBO?.Dispose();
            IBO?.Dispose();
            VAO?.Dispose();
            isdisposed = true;
        }

        public void Draw()
        {
            if (!Renderable || VAO == null) {
                Console.WriteLine("Non renderable"); 
                return; 
            }

            if (IBO == null)
            {
                VAO.Render();
            } else
            {
                VAO.Render(IBO);
            }
        }

        public void SetPath(string path)
        {
            this.path = path;
        }

        public string GetPath()
        {
            return path;
        }

        public void SetStatic(bool value)
        {
            IsStatic = value;
        }

        public void SetVertices(Vertex[] vertices)
        {
            VBO = new VertexBuffer(Vertex.VertexInfo, vertices.Length, IsStatic);
            VBO.SetData(vertices, vertices.Length);
            VAO = new VertexArray(VBO);
            this.VertexCount = vertices.Length;
            this.Vertices = vertices;
        }

        public void SetVertexArrayObject(VertexArray vao)
        {
            VAO = vao;
        }

        public void SetIndices(int[] indices)
        {
            IBO = new IndexBuffer(indices.Length, IsStatic);
            IBO.SetData(indices, indices.Length);
        }

        public Vertex[] GetVertices()
        {
            return Vertices;
        }
    }
}
