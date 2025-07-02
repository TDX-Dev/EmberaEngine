using EmberaEngine.Engine.Utilities;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Components;
using OpenTK.Graphics.OpenGL;

namespace ElementalEditor.Editor.Utils
{

    [Flags]
    public enum GizmoType
    {
        None = 0,
        PhysicsCollider = 1 << 0,
        Light = 1 << 1,
        Texture = 1 << 2,
        Circle = 1 << 3,
        Cube = 1 << 4,
        All = ~0
    }

    public abstract class GizmoObject
    {
        public abstract Type ComponentType { get; }
        public abstract void Initialize();
        public abstract void OnRender(Component component);
    }



    public static class Guizmo3D
    {
        static Shader GizmoTextureShader;
        static Shader lineMeshShader;
        static Shader gridShader;
        static Camera renderCamera;

        static Mesh Cube;
        static Mesh Quad;
        static Mesh Circle;

        public static GizmoType EnabledGizmos = GizmoType.All;

        public static void Initialize()
        {
            GizmoTextureShader = new Shader("Editor/Assets/Shaders/gizmoTexture");
            lineMeshShader = new Shader("Editor/Assets/Shaders/base");
            gridShader = new Shader("Editor/Assets/Shaders/gridshader");

            Cube = Graphics.GetWireFrameCube();
            Quad = Graphics.GetQuad();
            Circle = Graphics.GetCircle();
        }

        public static void Render(Scene scene)
        {
            renderCamera = Renderer3D.GetRenderCamera();
            if (renderCamera == null) return;

            GizmoRegistry.RenderAll(scene);
        }

        private static Matrix4 CreateModelMatrix(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            return Matrix4.CreateScale(scale)
                 * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X))
                 * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y))
                 * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z))
                 * Matrix4.CreateTranslation(position);
        }

        public static void DrawGrid()
        {
            GraphicsState.SetDepthMask(false);

            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(EmberaEngine.Engine.Rendering.BlendingFactor.SrcAlpha, EmberaEngine.Engine.Rendering.BlendingFactor.OneMinusSrcAlpha);

            gridShader.Use();
            gridShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            gridShader.SetMatrix4("W_VIEW_MATRIX",renderCamera.GetViewMatrix());
            gridShader.SetVector3("C_VIEWPOS", renderCamera.position);
            //GridShaderEx.SetBool("depthBehind", true);

            Quad.Draw();

            GraphicsState.SetDepthMask(true);
        }

        private static void SetupLineShader(Matrix4 modelMatrix)
        {
            lineMeshShader.Use();
            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            lineMeshShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            lineMeshShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());

            GraphicsState.SetDepthMask(false);
            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Line);
            GraphicsState.SetLineWidth(2);
        }

        public static void RenderCube(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            if (!EnabledGizmos.HasFlag(GizmoType.Cube) || renderCamera == null)
                return;

            GraphicsState.SetLineSmooth(true);
            var modelMatrix = CreateModelMatrix(position, scale, rotation);
            SetupLineShader(modelMatrix);

            Cube.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);

            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Fill);

            GraphicsState.SetDepthMask(true);
        }

        public static void RenderLightCircle(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            if (!EnabledGizmos.HasFlag(GizmoType.Light) || renderCamera == null)
                return;

            GraphicsState.SetLineSmooth(true);
            scale *= 2;

            Vector3[] rotations =
            {
            rotation,
            new Vector3(rotation.X, rotation.Y + 90, rotation.Z),
            new Vector3(rotation.X + 90, rotation.Y, rotation.Z)
        };

            foreach (var rot in rotations)
            {
                var modelMatrix = CreateModelMatrix(position, scale, rot);
                SetupLineShader(modelMatrix);
                Circle.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);
            }

            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Fill);
            GraphicsState.SetDepthMask(true);
        }

        public static void RenderLine(Vector3 position1, Vector3 position2, Color4 color)
        {
            LineRenderUtils.RenderLine(renderCamera, position1, position2, color, 0.05f);
        }

        public static void RenderCircle(Vector3 position, Vector3 scale, Vector3 rotation)
        {
            if (!EnabledGizmos.HasFlag(GizmoType.Circle) || renderCamera == null)
                return;

            GraphicsState.SetLineSmooth(true);
            var modelMatrix = CreateModelMatrix(position, scale, rotation);
            SetupLineShader(modelMatrix);

            Circle.VAO.Render(OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop);

            GraphicsState.SetPolygonMode(OpenTK.Graphics.OpenGL.MaterialFace.FrontAndBack, OpenTK.Graphics.OpenGL.PolygonMode.Fill);
        }
        public static void RenderTexture(Texture texture, Vector3 position, Vector3 scale)
        {
            if (!EnabledGizmos.HasFlag(GizmoType.Texture) || renderCamera == null)
                return;


            GizmoTextureShader.Use();
            GizmoTextureShader.SetInt("INPUT_TEXTURE", 0);

            // Proper billboarding with scale
            Matrix4 viewMatrix = renderCamera.GetViewMatrix();
            Matrix4 rotationMatrix = new Matrix4(
                new Vector4(viewMatrix.Row0.Xyz, 0),
                new Vector4(viewMatrix.Row1.Xyz, 0),
                new Vector4(viewMatrix.Row2.Xyz, 0),
                new Vector4(0, 0, 0, 1)
            );
            rotationMatrix = Matrix4.Transpose(rotationMatrix);

            Matrix4 modelMatrix = Matrix4.CreateScale(scale * 2.5f) * rotationMatrix * Matrix4.CreateTranslation(position);

            GizmoTextureShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            GizmoTextureShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            GizmoTextureShader.SetMatrix4("W_VIEW_MATRIX", viewMatrix);

            GraphicsState.SetTextureActiveBinding(EmberaEngine.Engine.Core.TextureUnit.Texture0);
            texture.Bind();

            Quad.Draw();
        }


        public static void DrawCapsule(Vector3 center, float height, float radius, Vector3 rot, Color4 color, int segments = 16)
        {
            Quaternion rotation = Helper.ToOpenTKQuaternion(Helper.ToQuaternion(Helper.ToNumerics3(Helper.ToRadians(rot))));
            float halfHeight = (height - 2 * radius) / 2f;
            Vector3 up = Vector3.UnitY;

            // Ends of the cylinder part
            Vector3 topCenter = center + Vector3.Transform(up * halfHeight, rotation);
            Vector3 bottomCenter = center - Vector3.Transform(up * halfHeight, rotation);

            // Cylinder edges
            for (int i = 0; i < segments; i++)
            {
                float theta1 = MathHelper.TwoPi * i / segments;
                float theta2 = MathHelper.TwoPi * (i + 1) / segments;

                Vector3 dir1 = new Vector3(MathF.Cos(theta1), 0, MathF.Sin(theta1)) * radius;
                Vector3 dir2 = new Vector3(MathF.Cos(theta2), 0, MathF.Sin(theta2)) * radius;

                Vector3 p1Top = topCenter + Vector3.Transform(dir1, rotation);
                Vector3 p1Bottom = bottomCenter + Vector3.Transform(dir1, rotation);
                Vector3 p2Top = topCenter + Vector3.Transform(dir2, rotation);
                Vector3 p2Bottom = bottomCenter + Vector3.Transform(dir2, rotation);

                LineRenderUtils.RenderLine(renderCamera, p1Bottom, p1Top, Color4.FloralWhite, 0.1f);
                LineRenderUtils.RenderLine(renderCamera, p1Top, p2Top, Color4.FloralWhite, 0.1f);
                LineRenderUtils.RenderLine(renderCamera, p1Bottom, p2Bottom, Color4.FloralWhite, 0.1f);
            }

            // Hemisphere arcs
            int hemisphereSegments = segments / 2;
            for (int i = 1; i < hemisphereSegments; i++)
            {
                float phi = MathHelper.PiOver2 * i / hemisphereSegments;
                float y = MathF.Sin(phi) * radius;
                float r = MathF.Cos(phi) * radius;

                for (int j = 0; j < segments; j++)
                {
                    float theta1 = MathHelper.TwoPi * j / segments;
                    float theta2 = MathHelper.TwoPi * (j + 1) / segments;

                    Vector3 dir1 = new Vector3(MathF.Cos(theta1) * r, y, MathF.Sin(theta1) * r);
                    Vector3 dir2 = new Vector3(MathF.Cos(theta2) * r, y, MathF.Sin(theta2) * r);

                    // Top hemisphere
                    Vector3 topP1 = topCenter + Vector3.Transform(dir1, rotation);
                    Vector3 topP2 = topCenter + Vector3.Transform(dir2, rotation);
                    LineRenderUtils.RenderLine(renderCamera, topP1, topP2, Color4.FloralWhite, 0.1f);

                    // Bottom hemisphere (mirror y)
                    dir1.Y = -dir1.Y;
                    dir2.Y = -dir2.Y;
                    Vector3 bottomP1 = bottomCenter + Vector3.Transform(dir1, rotation);
                    Vector3 bottomP2 = bottomCenter + Vector3.Transform(dir2, rotation);
                    LineRenderUtils.RenderLine(renderCamera, bottomP1, bottomP2, Color4.FloralWhite, 0.1f);
                }
            }
        }

    }

}
