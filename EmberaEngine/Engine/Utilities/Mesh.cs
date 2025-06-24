using System;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Utilities
{
    public class Mesh : IDisposable
    {
        // GPU Resources
        public VertexBuffer VBO { get; private set; }
        public IndexBuffer IBO { get; private set; }
        public VertexArray VAO { get; private set; }

        // Geometry
        private Vertex[] vertices;
        private int[] indices;

        // Identification
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Path { get; private set; }
        public string Name { get; set; }
        public string FileID { get; set; }

        // Material Reference
        public Guid MaterialReference { get; set; }     // Asset GUID
        public uint MaterialRenderHandle { get; set; }  // Runtime render handle

        // State
        public bool IsRenderable { get; set; } = true;
        public bool IsHighlighted { get; set; } = false;
        public bool IsStatic { get; private set; } = true;

        // Transform
        internal Matrix4 WorldMatrix;

        private bool isDisposed = false;

        // Constructors
        public Mesh() { }

        ~Mesh()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (isDisposed) return;

            VBO?.Dispose();
            IBO?.Dispose();
            VAO?.Dispose();

            isDisposed = true;
            GC.SuppressFinalize(this);
        }

        public void Draw()
        {
            if (!IsRenderable || VAO == null)
            {
                return;
            }

            if (IBO != null)
                VAO.Render(IBO);
            else
                VAO.Render();
        }

        // Setters
        public void SetPath(string path) => Path = path;

        public void SetStatic(bool isStatic) => IsStatic = isStatic;

        public void SetVertices(Vertex[] vertexArray)
        {
            vertices = vertexArray;
            VBO = new VertexBuffer(Vertex.VertexInfo, vertexArray.Length, IsStatic);
            VBO.SetData(vertexArray, vertexArray.Length);
            VAO = new VertexArray(VBO);
        }

        public void SetIndices(int[] indexArray)
        {
            indices = indexArray;
            IBO = new IndexBuffer(indexArray.Length, IsStatic);
            IBO.SetData(indexArray, indexArray.Length);
        }

        public void SetVertexArrayObject(VertexArray vao) => VAO = vao;

        // Getters
        public Vertex[] GetVertices() => vertices;

        public int[] GetIndices() => indices;
    }
}
