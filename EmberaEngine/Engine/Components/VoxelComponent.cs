using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.Components
{
    class VoxelComponent : Component
    {
        public override string Type => nameof(VoxelComponent);

        private int xGrid = 16;
        private int zGrid = 16;
        private int maxHeight = 10;
        private float scale = 0.1f;

        private int[,] heightMap;

        public override void OnStart()
        {
            gameObject.AddComponent<MeshRenderer>();
            UpdateChunk();
        }

        public override void OnUpdate(float dt) { }

        private void UpdateChunk()
        {
            MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
            mr?.RemoveMesh();

            heightMap = new int[xGrid, zGrid];
            List<Vertex> vertices = new List<Vertex>();

            // Generate height map
            for (int x = 0; x < xGrid; x++)
            {
                for (int z = 0; z < zGrid; z++)
                {
                    float nx = (x + 0) * scale;
                    float nz = (z + 0) * scale;

                    heightMap[x, z] = (int)(Perlin.Noise(nx, nz) * maxHeight);
                }
            }


            // Generate geometry
            for (int x = 0; x < xGrid; x++)
            {
                for (int z = 0; z < zGrid; z++)
                {
                    int h = heightMap[x, z];
                    for (int y = 0; y < h; y++)
                    {
                        bool isTop = y == h - 1;
                        bool exposeLeft = x == 0 || y >= heightMap[x - 1, z];
                        bool exposeRight = x == xGrid - 1 || y >= heightMap[x + 1, z];
                        bool exposeFront = z == zGrid - 1 || y >= heightMap[x, z + 1];
                        bool exposeBack = z == 0 || y >= heightMap[x, z - 1];

                        if (isTop) AddTopFace(vertices, x, y, z);
                        if (exposeLeft) AddLeftFace(vertices, x, y, z);
                        if (exposeRight) AddRightFace(vertices, x, y, z);
                        if (exposeFront) AddFrontFace(vertices, x, y, z);
                        if (exposeBack) AddBackFace(vertices, x, y, z);
                    }
                }
            }

            VertexBuffer VBO = new VertexBuffer(Vertex.VertexInfo, vertices.Count);
            VBO.SetData(vertices.ToArray(), vertices.Count);

            VertexArray VAO = new VertexArray(VBO);

            Mesh mesh = new Mesh();
            mesh.SetVertexArrayObject(VAO);
            PBRMaterial mat = (PBRMaterial)Renderer3D.ActiveRenderingPipeline.GetDefaultMaterial();
            mat.Albedo =  new Color4(UtilRandom.GetFloat(), UtilRandom.GetFloat(), UtilRandom.GetFloat(), 1);
            //mesh.MaterialIndex = MaterialManager.AddMaterial(mat);

            mr.SetMesh(mesh);
        }

        private float Noise(int x, int z)
        {
            float nx = x * scale;
            float nz = z * scale;
            return (MathF.Sin(nx * 2f) * MathF.Cos(nz * 2f) * 0.5f + 0.5f);
        }

        // Add cube face functions
        private void AddTopFace(List<Vertex> v, int x, int y, int z)
        {
            Vector3 normal = Vector3.UnitY;
            v.AddRange(Quad(
                new Vector3(x, y + 1, z + 1),     // top-left
                new Vector3(x + 1, y + 1, z + 1), // top-right
                new Vector3(x + 1, y + 1, z),     // bottom-right
                new Vector3(x, y + 1, z),         // bottom-left
                normal));

        }

        private void AddLeftFace(List<Vertex> v, int x, int y, int z)
        {
            Vector3 normal = -Vector3.UnitX;
            v.AddRange(Quad(
                new Vector3(x, y, z),
                new Vector3(x, y, z + 1),
                new Vector3(x, y + 1, z + 1),
                new Vector3(x, y + 1, z),
                normal));
        }

        private void AddRightFace(List<Vertex> v, int x, int y, int z)
        {
            Vector3 normal = Vector3.UnitX;
            v.AddRange(Quad(
                new Vector3(x + 1, y, z + 1),
                new Vector3(x + 1, y, z),
                new Vector3(x + 1, y + 1, z),
                new Vector3(x + 1, y + 1, z + 1),
                normal));
        }

        private void AddFrontFace(List<Vertex> v, int x, int y, int z)
        {
            Vector3 normal = Vector3.UnitZ;
            v.AddRange(Quad(
                new Vector3(x, y, z + 1),
                new Vector3(x + 1, y, z + 1),
                new Vector3(x + 1, y + 1, z + 1),
                new Vector3(x, y + 1, z + 1),
                normal));
        }

        private void AddBackFace(List<Vertex> v, int x, int y, int z)
        {
            Vector3 normal = -Vector3.UnitZ;
            v.AddRange(Quad(
                new Vector3(x + 1, y, z),
                new Vector3(x, y, z),
                new Vector3(x, y + 1, z),
                new Vector3(x + 1, y + 1, z),
                normal));
        }

        private List<Vertex> Quad(Vector3 bl, Vector3 br, Vector3 tr, Vector3 tl, Vector3 normal)
        {
            Vector2 uv0 = new Vector2(0, 0);
            Vector2 uv1 = new Vector2(1, 0);
            Vector2 uv2 = new Vector2(1, 1);
            Vector2 uv3 = new Vector2(0, 1);

            // Use first triangle (bl, br, tr) to compute tangent/bitangent
            Vector3 edge1 = br - bl;
            Vector3 edge2 = tr - bl;
            Vector2 deltaUV1 = uv1 - uv0;
            Vector2 deltaUV2 = uv2 - uv0;

            float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

            Vector3 tangent = new Vector3(
                f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X),
                f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y),
                f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z)
            );
            tangent = Vector3.Normalize(tangent);

            Vector3 bitangent = new Vector3(
                f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X),
                f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y),
                f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z)
            );
            bitangent = Vector3.Normalize(bitangent);

            return new List<Vertex>
    {
        new Vertex(bl, normal, uv0, tangent, bitangent),
        new Vertex(br, normal, uv1, tangent, bitangent),
        new Vertex(tr, normal, uv2, tangent, bitangent),

        new Vertex(tr, normal, uv2, tangent, bitangent),
        new Vertex(tl, normal, uv3, tangent, bitangent),
        new Vertex(bl, normal, uv0, tangent, bitangent)
    };
        }

    }


    public static class Perlin
    {
        private static readonly int[] permutation = new int[256]
        {
        151,160,137,91,90,15,131,13,201,95,96,53,194,233,7,225,
        140,36,103,30,69,142,8,99,37,240,21,10,23,190, 6,148,
        247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,
        57,177,33,88,237,149,56,87,174,20,125,136,171,168, 68,175,
        74,165,71,134,139,48,27,166,77,146,158,231,83,111,229,122,
        60,211,133,230,220,105,92,41,55,46,245,40,244,102,143,54,
        65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
        200,196,135,130,116,188,159,86,164,100,109,198,173,186, 3,64,
        52,217,226,250,124,123,5,202,38,147,118,126,255,82,85,212,
        207,206,59,227,47,16,58,17,182,189,28,42,223,183,170,213,
        119,248,152, 2,44,154,163,70,221,153,101,155,167,43,172,9,
        129,22,39,253,19,98,108,110,79,113,224,232,178,185,112,104,
        218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,
        81,51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,
        184, 84,204,176,115,121,50,45,127, 4,150,254,138,236,205,93,
        222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
        };

        private static readonly int[] p;

        static Perlin()
        {
            p = new int[512];
            for (int i = 0; i < 256; i++)
            {
                p[i] = permutation[i];
                p[256 + i] = permutation[i];
            }
        }

        public static float Noise(float x, float y)
        {
            int xi = (int)MathF.Floor(x) & 255;
            int yi = (int)MathF.Floor(y) & 255;

            float xf = x - MathF.Floor(x);
            float yf = y - MathF.Floor(y);

            float u = Fade(xf);
            float v = Fade(yf);

            int aa = p[p[xi] + yi];
            int ab = p[p[xi] + yi + 1];
            int ba = p[p[xi + 1] + yi];
            int bb = p[p[xi + 1] + yi + 1];

            float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

            return (Lerp(x1, x2, v) + 1f) / 2f;
        }

        private static float Fade(float t) =>
            t * t * t * (t * (t * 6f - 15f) + 10f);

        private static float Lerp(float a, float b, float t) =>
            a + t * (b - a);

        private static float Grad(int hash, float x, float y)
        {
            int h = hash & 15;
            float u = (h < 8) ? x : y;
            float v = (h < 4) ? y : ((h == 12 || h == 14) ? x : 0);
            return (((h & 1) == 0) ? u : -u) + (((h & 2) == 0) ? v : -v);
        }
    }


}
