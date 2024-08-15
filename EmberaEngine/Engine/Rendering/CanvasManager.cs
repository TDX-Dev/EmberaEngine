using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Rendering
{
    public enum Element2DType
    {
        Sprite,
        Text
    }

    public class RenderCanvas
    {
        public Matrix4 Projection;
        public int id;
        public List<RenderSprite> sprites = new List<RenderSprite>();
        public List<RenderText> textObjects = new List<RenderText>();

        public int ScreenWidth, ScreenHeight;
    }

    public abstract class Element2D
    {
        public abstract Element2DType Type { get; }

        public Vector2 transform;
        public Vector2 scale;
        public float rotationAngle;
    }

    public class RenderSprite : Element2D
    {
        public override Element2DType Type => Element2DType.Sprite;
        public Texture Sprite;
        public Vector4 SolidColor;
        public int arrayIndex;
        public int order;
    }

    public class RenderText : Element2D
    {
        public override Element2DType Type => Element2DType.Text;
        public Mesh textMesh;
        public Texture fontTexture;
    }

    public class CanvasManager
    {
        public static List<RenderCanvas> Canvases = new List<RenderCanvas>();

        public static void ResizeAllCanvases(int width, int height)
        {
            foreach (RenderCanvas value in Canvases)
            {
                value.ScreenWidth = width;
                value.ScreenHeight = height;
            }
        }

        public static int AddRenderCanvas(RenderCanvas canvas)
        {
            canvas.id = Canvases.Count;
            Canvases.Add(canvas);
            return Canvases.Count - 1;
        }

        public static void AddRenderSprite(int canvasID, RenderSprite instance)
        {
            foreach (RenderCanvas value in Canvases)
            {
                if (value.id == canvasID)
                {
                    value.sprites.Add(instance);
                }
            }
        }

        public static void AddRenderText(int canvasID, RenderText instance)
        {
            foreach (RenderCanvas value in Canvases)
            {
                if (value.id == canvasID)
                {
                    value.textObjects.Add(instance);
                }
            }
        }

        public static void RemoveRenderSprite(RenderSprite renderSpriteID)
        {
            foreach (RenderCanvas value in Canvases)
            {
                if (value.sprites.Contains(renderSpriteID))
                {
                    value.sprites.Remove(renderSpriteID);
                }
            }
        }
        public static void RemoveRenderText(RenderText renderTextID)
        {
            foreach (RenderCanvas value in Canvases)
            {
                if (value.textObjects.Contains(renderTextID))
                {
                    value.textObjects.Remove(renderTextID);
                }
            }
        }
    }
}
