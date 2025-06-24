using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Utilities
{
    public class Primitives
    {


        public static Vertex[] GetCircle(int segments = 64)
        {
            Vertex[] vertices = new Vertex[segments];

            Vector3 normal = new Vector3(0, 0, 1); // Facing forward

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / segments);
                float x = MathF.Cos(angle) * 0.5f;
                float y = MathF.Sin(angle) * 0.5f;

                Vector3 pos = new Vector3(x, y, 0);
                Vector2 uv = new Vector2(x + 0.5f, y + 0.5f); // optional

                vertices[i] = new Vertex(pos, normal, uv);
            }

            return vertices;
        }




        public static Vertex[] GetQuadVertex()
        {
            return new Vertex[]
            {
                // First triangle
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 0)),
                new Vertex(new Vector3( 0.5f, -0.5f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 0)),
                new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 1)),

                // Second triangle
                new Vertex(new Vector3( 0.5f,  0.5f, 0.0f), new Vector3(0, 0, 1), new Vector2(1, 1)),
                new Vertex(new Vector3(-0.5f,  0.5f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 1)),
                new Vertex(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0, 0, 1), new Vector2(0, 0)),
            };
        }

        public static Vertex[] GetWireframeCubeVertices()
        {
            Vector3 dummyNormal = new Vector3(0, 0, 1);       // Arbitrary normal
            Vector2 dummyUV = new Vector2(0, 0);              // Arbitrary UV

            return new Vertex[]
            {
                // Bottom square
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), dummyNormal, dummyUV),

                // Top square
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), dummyNormal, dummyUV),

                // Vertical edges
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), dummyNormal, dummyUV),

                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), dummyNormal, dummyUV),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), dummyNormal, dummyUV),
            };
        }



        public static Vertex[] GetCubeVertex()
        {
            return new Vertex[]
            {
        // Front face (+Z)
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(1, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(0, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, 0, 1), new Vector2(0, 0)),

        // Back face (-Z)
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0, 0)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1, 0)),
        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0, 1)),
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, 0, -1), new Vector2(0, 0)),

        // Left face (-X)
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0, 0)),
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-1, 0, 0), new Vector2(1, 0)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1, 0, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1, 0, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1, 0, 0), new Vector2(0, 0)),

        // Right face (+X)
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(1, 0, 0), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(1, 0, 0), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(1, 0, 0), new Vector2(0, 1)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(1, 0, 0), new Vector2(0, 0)),

        // Bottom face (-Y)
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(1, 0)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, -1, 0), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, -1, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, -1, 0), new Vector2(0, 1)),
        new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1, 0), new Vector2(0, 0)),

        // Top face (+Y)
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0, 1, 0), new Vector2(0, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0, 1, 0), new Vector2(1, 0)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(1, 1)),
        new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(1, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0, 1, 0), new Vector2(0, 1)),
        new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0, 1, 0), new Vector2(0, 0)),
            };
        }


        public static Vertex[] GetPlaneVertices()
        {
            return new Vertex[] {
                new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-1.0f, -1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-1.0f,  1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-1.0f, 1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(0.0f, 1.0f))
            };
        }

        public static Vertex[] GetSphereVertices(int segments = 32, double radius = 1.0)
        {
            List<Vertex> vertices = new List<Vertex>();

            double phi;
            double theta;
            float x;
            float y;
            float z;
            double[] vertex;

            for (int i = 0; i < segments; i++)
            {
                phi = Math.PI * i / (segments - 1);
                for (int j = 0; j < segments; j++)
                {
                    theta = 2 * Math.PI * j / (segments - 1);
                    x = (float)(radius * Math.Sin(phi) * Math.Cos(theta));
                    y = (float)(radius * Math.Sin(phi) * Math.Sin(theta));
                    z = (float)(radius * Math.Cos(phi));
                    vertices.Add(new Vertex( new Vector3(x,y,z), Vector3.One, Vector2.One));
                }
            }
            return vertices.ToArray();
        }
    }

    public readonly struct VertexAttribute
        {
            public readonly string Name;
            public readonly int Index;
            public readonly int ComponentCount;
            public readonly int Offset;

            public VertexAttribute(string name, int index, int componentcount, int offset)
            {
                Name = name;
                Index = index;
                ComponentCount = componentcount;
                Offset = offset;
            }
        }

    public sealed class VertexInfo
    {
        public readonly Type Type;
        public readonly int SizeInBytes;
        public readonly VertexAttribute[] VertexAttributes;

        public VertexInfo(Type type, params VertexAttribute[] attributes)
        {

            this.Type = type;
            this.VertexAttributes = attributes;
            this.SizeInBytes = 0;

            for (int i = 0; i < VertexAttributes.Length; i++)
            {
                VertexAttribute attribute = this.VertexAttributes[i];
                this.SizeInBytes += attribute.ComponentCount * sizeof(float);
            }


        }


    }


    public readonly struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TexCoord;
        public readonly Vector3 Tangent;
        public readonly Vector3 BiTangent;
        public readonly Vector2 SecondaryTexCoord;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(Vertex),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("Normal", 1, 3, 3 * sizeof(float)),
            new VertexAttribute("TexCoord", 2, 2, 6 * sizeof(float)),
            new VertexAttribute("Tangent", 3, 3, 8 * sizeof(float)),
            new VertexAttribute("BiTangent", 4, 3, 11 * sizeof(float)),
            new VertexAttribute("TexCoord2", 5, 2, 14 * sizeof(float))
            );

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = texcoord;
            this.Tangent = Vector3.Zero;
            this.BiTangent = Vector3.Zero;
            this.SecondaryTexCoord = texcoord;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord, Vector3 tangent, Vector3 bitangent)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = texcoord;
            this.Tangent = tangent;
            this.BiTangent = bitangent;
            this.SecondaryTexCoord = texcoord;
        }

    }

    public readonly struct LineVertex
    {
        public readonly Vector3 Position;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(LineVertex),
            new VertexAttribute("Position", 0, 3, 0)
            );

        public LineVertex(Vector3 Position)
        {
            this.Position = Position;
        }
    }
}
