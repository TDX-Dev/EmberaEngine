using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Components
{
    public class SpriteRenderer : Component
    {
        public override string Type => nameof(SpriteRenderer);

        public Texture Sprite = Texture.GetWhite2D();
        public Vector4 SolidColor = Vector4.Zero;
        public float Rotation;
        public int sortingOrder = 0;

        private RenderSprite renderSprite;

        public SpriteRenderer()
        {

        }

        CanvasComponent IterateGetParentComponent(GameObject gameObject)
        {
            CanvasComponent component = gameObject.GetComponent<CanvasComponent>();

            if (component != null)
            {
                return component;
            } else if (gameObject.parentObject != null)
            {
                return IterateGetParentComponent(gameObject.parentObject);
            }

            return null;
        }

        public override void OnStart()
        {
            renderSprite = new RenderSprite()
            {
                transform = gameObject.transform.position.Xy,
                scale = gameObject.transform.scale.Xy,
                rotationAngle = Rotation,
                Sprite = Sprite,
                SolidColor = SolidColor
            };
            
            CanvasComponent component = IterateGetParentComponent(gameObject) ?? gameObject.scene.GetComponent<CanvasComponent>();

            if (component != null)
            {
                CanvasManager.AddRenderSprite(component.canvas.id, renderSprite);
            }
        }

        public override void OnUpdate(float dt)
        {
            renderSprite.transform = gameObject.transform.position.Xy;
            renderSprite.scale = gameObject.transform.scale.Xy / 2;
            renderSprite.rotationAngle = gameObject.transform.rotation.X;
            renderSprite.Sprite = Sprite;
            renderSprite.order = sortingOrder;
        }

        public override void OnDestroy()
        {
            CanvasManager.RemoveRenderSprite(renderSprite);
        }


    }
}
