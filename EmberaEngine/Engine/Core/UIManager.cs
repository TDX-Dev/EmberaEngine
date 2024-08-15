using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{

    public enum UIElementType
    {
        Button
    }

    public abstract class UIElement
    {
        public int UICanvasID { get; set; }
        public abstract UIElementType ElementType { get; }
        public float left, right, top, bottom;
    }

    public class UIButton : UIElement
    {
        public override UIElementType ElementType => UIElementType.Button;
        public bool IsHovering;
        public bool IsPressed;
    }

    public class UIManager
    {
        static List<CanvasComponent> UICanvases = new List<CanvasComponent>();
        static List<UIButton> UIButtons = new List<UIButton>();

        public static void AddCanvas(CanvasComponent obj)
        {
            UICanvases.Add(obj);
        }

        static CanvasComponent GetCanvasById(int id)
        {
            foreach (CanvasComponent c in UICanvases)
            {
                if (c.canvas.id == id)
                {
                    return c;
                }
            }
            return null;
        }

        public static void AddButton(ButtonComponent button)
        {
            button.button = new UIButton();

            CanvasComponent canvas = GetCanvas(button.gameObject);

            if (canvas != null)
            {
                button.button.UICanvasID = canvas.canvas.id;
            } else
            {
                Console.WriteLine("Added UI Element Without Canvas");
                return;
            }

            UIButtons.Add(button.button);
        }

        public static void Update()
        {
            foreach (UIButton button in UIButtons)
            {
                CanvasComponent canvas = GetCanvasById(button.UICanvasID);
                Vector2 mousePosition = canvas.GetLocalMousePos(Input.mousePosition);

                //Console.WriteLine(mousePosition);

                if (mousePosition.X >  button.left && mousePosition.X < button.right)
                {
                    if (mousePosition.Y > button.top && mousePosition.Y <  button.bottom)
                    {
                        //Console.WriteLine("IS INSIDE");
                        if (Input.GetMouseDown(Utilities.MouseButton.Left))
                        {
                            button.IsPressed = true;
                        }
                        else
                        {
                            button.IsPressed = false;
                        }
                        button.IsHovering = true;

                        continue;
                    }
                }
                button.IsHovering = false;
            }
        }

        static CanvasComponent GetCanvas(GameObject obj)
        {
            return IterateGetParentComponent(obj) ?? obj.scene.GetComponent<CanvasComponent>(); 
        }

        static CanvasComponent IterateGetParentComponent(GameObject gameObject)
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

    }
}
