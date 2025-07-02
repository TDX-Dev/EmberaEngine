using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace ElementalEditor.Editor.Utils
{
    public static class LineRenderUtils
    {
        static VertexArray VAO;
        static Shader DummyShader = new Shader("Editor/Assets/Shaders/linerender");
        public static float DefaultThickness = 1f;

        static LineRenderUtils()
        {
            // Create a dummy buffer that will be reused
            List<Vertex> dummyVertices = new List<Vertex>
            {
                new Vertex(Vector3.Zero, Vector3.One, Vector2.Zero),
                new Vertex(Vector3.Zero, Vector3.One, Vector2.Zero),
                new Vertex(Vector3.Zero, Vector3.One, Vector2.Zero),
                new Vertex(Vector3.Zero, Vector3.One, Vector2.Zero),
                new Vertex(Vector3.Zero, Vector3.One, Vector2.Zero),
                new Vertex(Vector3.Zero, Vector3.One, Vector2.Zero)
            };

            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, dummyVertices.Count, false);
            vertexBuffer.SetData(dummyVertices.ToArray(), dummyVertices.Count);
            VAO = new VertexArray(vertexBuffer);
        }

        public static void RenderLine(Camera camera, Vector3 start, Vector3 end, Color4 color, float? thicknessOverride = null, bool drawGuide = false)
        {
            float thickness = thicknessOverride ?? DefaultThickness;

            if (drawGuide)
            {
                Vector3 center = (start + end) * 0.5f;
                Vector3 guideDir = Vector3.Cross((end - start).Normalized(), camera.front).Normalized();
                RenderThickLine(camera, center - guideDir * 5f, center + guideDir * 5f, color, 0.2f);
            }

            RenderThickLine(camera, start, end, color, thickness);
        }

        private static void RenderThickLine(Camera camera, Vector3 start, Vector3 end, Color4 color, float thickness)
        {
            Vector3 lineDir = (end - start).Normalized();
            Vector3 viewDir = (camera.position - (start + end) * 0.5f).Normalized();

            // Safer perpendicular vector calculation with fallback for degenerate cases
            Vector3 perpendicular = Vector3.Cross(lineDir, viewDir);
            if (perpendicular.LengthSquared < 1e-5f)
            {
                perpendicular = Vector3.Cross(lineDir, Vector3.UnitY);
                if (perpendicular.LengthSquared < 1e-5f)
                {
                    perpendicular = Vector3.Cross(lineDir, Vector3.UnitZ);
                }
            }
            perpendicular = perpendicular.Normalized();

            Vertex[] vertices = GenerateLineQuad(start, end, perpendicular, thickness);
            VAO.VertexBuffer.SetData(vertices, vertices.Length);

            DummyShader.Use();
            DummyShader.SetMatrix4("W_MODEL_MATRIX", Matrix4.Identity);
            DummyShader.SetMatrix4("W_VIEW_MATRIX", camera.GetViewMatrix());
            DummyShader.SetMatrix4("W_PROJECTION_MATRIX", camera.GetProjectionMatrix());
            DummyShader.SetVector4("LINE_COLOR", new Vector4(color.R, color.G, color.B, color.A));

            VAO.Render();
        }

        private static Vertex[] GenerateLineQuad(Vector3 start, Vector3 end, Vector3 perpendicular, float thickness)
        {
            Vector3 offset = perpendicular * (thickness * 0.5f);

            Vector3 v1 = start + offset;
            Vector3 v2 = start - offset;
            Vector3 v3 = end + offset;
            Vector3 v4 = end - offset;

            return new Vertex[]
            {
                new Vertex(v1, Vector3.One, Vector2.Zero),
                new Vertex(v2, Vector3.One, Vector2.UnitX),
                new Vertex(v4, Vector3.One, Vector2.UnitY),
                new Vertex(v4, Vector3.One, Vector2.One),
                new Vertex(v1, Vector3.One, Vector2.Zero),
                new Vertex(v3, Vector3.One, Vector2.UnitY)
            };
        }
    }
}
