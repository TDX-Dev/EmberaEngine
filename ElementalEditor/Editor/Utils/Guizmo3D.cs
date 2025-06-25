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
        static Camera renderCamera;

        static Mesh Cube;
        static Mesh Quad;
        static Mesh Circle;

        public static GizmoType EnabledGizmos = GizmoType.All;

        public static void Initialize()
        {
            GizmoTextureShader = new Shader("Editor/Assets/Shaders/gizmoTexture");
            lineMeshShader = new Shader("Editor/Assets/Shaders/base");

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

        private static void SetupLineShader(Matrix4 modelMatrix)
        {
            lineMeshShader.Use();
            lineMeshShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            lineMeshShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            lineMeshShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());

            GraphicsState.SetDepthTest(true);
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

            Matrix4 modelMatrix = Matrix4.CreateScale(scale) *
                                  Matrix4.LookAt(position, renderCamera.position, -Vector3.UnitY);
            modelMatrix.Invert();

            GizmoTextureShader.SetMatrix4("W_MODEL_MATRIX", modelMatrix);
            GizmoTextureShader.SetMatrix4("W_PROJECTION_MATRIX", renderCamera.GetProjectionMatrix());
            GizmoTextureShader.SetMatrix4("W_VIEW_MATRIX", renderCamera.GetViewMatrix());

            GraphicsState.SetTextureActiveBinding(TextureUnit.Texture0);
            texture.Bind();

            Quad.Draw();
        }
    }

}
