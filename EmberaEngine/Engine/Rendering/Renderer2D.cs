using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Rendering;

namespace EmberaEngine.Engine.Rendering
{
    public class Renderer2D
    {

        static Shader Basic2DShader;
        static Shader Font2DShader;
        public static Matrix4 Projection;
        static Mesh PlaneMesh;

        // Framebuffer Textures
        static Texture CompositeBufferTexture;

        // Framebuffers
        static Framebuffer CompositeBuffer2D;

        public static void Initialize(int width, int height)
        {
            Basic2DShader = new Shader("Engine/Content/Shaders/2D/sprite2d");
            Font2DShader = new Shader("Engine/Content/Shaders/2D/font");

            Projection = Graphics.CreateOrthographic2D(width, height, 1f, -1f);

            Vertex[] vertices = Primitives.GetPlaneVertices();

            VertexBuffer vertexBuffer = new VertexBuffer(Vertex.VertexInfo, vertices.Length);
            vertexBuffer.SetData(vertices, vertices.Length);
            VertexArray PlaneVAO = new VertexArray(vertexBuffer);
            PlaneMesh = new Mesh();
            PlaneMesh.SetVertexArrayObject(PlaneVAO);

            // Setting up composite buffer
            CompositeBufferTexture = new Texture(TextureTarget2d.Texture2D);
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();

            CompositeBuffer2D = new Framebuffer();
            CompositeBuffer2D.AttachFramebufferTexture(OpenTK.Graphics.OpenGL.FramebufferAttachment.ColorAttachment0, CompositeBufferTexture);
        }

        public static void BeginRender()
        {

            CompositeBuffer2D.Bind();
            GraphicsState.Clear(true, true);
            GraphicsState.ClearColor(0, 0, 0, 1);
            GraphicsState.SetViewport(0, 0, Renderer.Width, Renderer.Height);
            GraphicsState.SetCulling(false);
            GraphicsState.SetDepthTest(false);
            GraphicsState.SetBlending(true);
            GraphicsState.SetBlendingFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public static void Render()
        {
            Basic2DShader.Use();

            Basic2DShader.SetMatrix4("W_PROJECTION_MATRIX", Projection);

            foreach (RenderCanvas value in CanvasManager.Canvases)
            {
                Basic2DShader.SetMatrix4("W_PROJECTION_MATRIX", value.Projection);

                RenderSprite[] sortedSprites = value.sprites.OrderByDescending(o => o.order).ToArray();

                for (int i = 0; i < sortedSprites.Length; i++)
                {
                    RenderSprite renderSprite = sortedSprites[i];

                    Matrix4 model = Matrix4.CreateScale(renderSprite.scale.X, renderSprite.scale.Y, 1f);
                    model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(renderSprite.rotationAngle));
                    model *= Matrix4.CreateTranslation(renderSprite.transform.X, renderSprite.transform.Y, -1f + (0.01f * renderSprite.order));

                    Basic2DShader.SetMatrix4("W_MODEL_MATRIX", model);

                    Basic2DShader.SetVector4("u_Color", renderSprite.SolidColor);
                    Basic2DShader.SetInt("u_Texture", 0);

                    renderSprite.Sprite.SetActiveUnit(TextureUnit.Texture0);
                    renderSprite.Sprite.Bind();

                    PlaneMesh.Draw();
                }
            }

            Font2DShader.Use();

            Font2DShader.SetMatrix4("W_PROJECTION_MATRIX", Projection);

            foreach (RenderCanvas value in CanvasManager.Canvases)
            {
                Font2DShader.SetMatrix4("W_PROJECTION_MATRIX", value.Projection);

                for (int i = 0; i < value.textObjects.Count; i++)
                {
                    RenderText textObject = value.textObjects[i];

                    Matrix4 model = Matrix4.CreateScale(-textObject.scale.X, textObject.scale.Y, 1f);
                    model *= Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(textObject.rotationAngle + 180));
                    model *= Matrix4.CreateTranslation(textObject.transform.X, textObject.transform.Y, -1f);

                    Font2DShader.SetMatrix4("W_MODEL_MATRIX", model);

                    Font2DShader.SetInt("u_Texture", 0);

                    textObject.fontTexture.SetActiveUnit(TextureUnit.Texture0);
                    textObject.fontTexture.Bind();

                    textObject.textMesh.Draw();
                }
            }

        }

        public static void EndRender()
        {
            GraphicsState.ClearTextureBinding2D();
            GraphicsState.ClearFrameBufferBinding();
        }

        public static void Resize(int width, int height)
        {
            CompositeBufferTexture.TexImage2D(width, height, PixelInternalFormat.Rgba16f, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            CompositeBufferTexture.GenerateMipmap();
            Projection = Graphics.CreateOrthographic2D(width, height, 1f, -1f);
        }

        public static Framebuffer GetComposite2D()
        {
            return CompositeBuffer2D;
        }


    }
}
