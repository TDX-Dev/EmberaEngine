using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Core;

namespace EmberaEngine.Engine.Components
{
    public class TextComponent : Component
    {
        public override string Type => nameof(TextComponent);

        public int charSpacing = 0;
        public string Content = "";
        public FontObject fontObject;

        private RenderText textObject;
        private List<Vertex> vertices = new List<Vertex>();
        private string prevContent = "";

        CanvasComponent IterateGetParentComponent(GameObject gameObject)
        {
            CanvasComponent component = gameObject.GetComponent<CanvasComponent>();

            if (component != null)
            {
                return component;
            }
            else if (gameObject.parentObject != null)
            {
                return IterateGetParentComponent(gameObject.parentObject);
            }

            return null;
        }

        public override void OnStart()
        {
            fontObject = FontUtilities.LoadFont("LD.ttf", 32f);

            textObject = new RenderText();
            textObject.textMesh = new Mesh();
            textObject.fontTexture = fontObject.fontTexture;

            CanvasComponent component = IterateGetParentComponent(gameObject) ?? gameObject.scene.GetComponent<CanvasComponent>();

            CanvasManager.AddRenderText(component.canvas.id, textObject);
        }

        public override void OnDestroy()
        {

        }

        public override void OnUpdate(float dt)
        {
            textObject.transform = gameObject.transform.position.Xy;
            textObject.scale = gameObject.transform.scale.Xy;
            textObject.rotationAngle = gameObject.transform.rotation.X;

            if (prevContent != Content)
            {
                prevContent = Content;
                ReconstructMesh();
                //Console.WriteLine("RECONSTRUCTING");
            }
        }

        void ReconstructMesh()
        {
            float totalLength = 0;
            float totalHeight = 0;
            textObject.textMesh?.Dispose();
            vertices.Clear();

            if (Content.Trim() == "") { textObject.textMesh = new Mesh(); Console.WriteLine("RETURNING"); return; }

            for (int y = 0; y < Content.Length; y++)
            {
                if (Content[y] == ' ')
                {
                    totalLength += 10;
                    continue;
                }
                if (Content[y] == "\n".ToCharArray()[0])
                {
                    totalHeight += 32;
                    totalLength = 0;
                    continue;
                }

                for (int i = 0; i < fontObject.fontChars.Count; i++)
                {
                    char character = Content[y];

                    FontChar glyph = fontObject.fontChars[i];

                    if (character == glyph.character)
                    {
                        float u = (float)(glyph.x) / (float)fontObject.fontTexture.Width;
                        float v = (float)(glyph.y) / (float)fontObject.fontTexture.Height;

                        float r = (float)(glyph.x + glyph.w) / (float)fontObject.fontTexture.Width;
                        float b = (float)(glyph.y + glyph.h) / (float)fontObject.fontTexture.Height;

                        float glyphWidth = (float)glyph.w;
                        float glyphHeight = (float)glyph.h;

                        float alignTopFactor = -glyph.bitmapTop;


                        vertices.Add(
                            new Vertex(new Vector3(totalLength + glyphWidth, totalHeight + alignTopFactor, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(r, v))
                        );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength, totalHeight + alignTopFactor, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u, v))
                         );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength, totalHeight + glyphHeight + alignTopFactor, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u, b))
                        );

                        //

                        vertices.Add(
                            new Vertex(new Vector3(totalLength + glyphWidth, totalHeight + glyphHeight + alignTopFactor, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(r, b))
                        );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength + glyphWidth, totalHeight + alignTopFactor, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(r, v))
                        );

                        vertices.Add(
                            new Vertex(new Vector3(totalLength, totalHeight + glyphHeight + alignTopFactor, 0), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(u, b))
                        );

                        totalLength += glyphWidth + charSpacing;
                    }
                }
            }
            textObject.textMesh = new Mesh();

            textObject.textMesh.SetVertices(vertices.ToArray());
        }


    }
}
