using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public class Input
    {

        public static Vector2 mousePosition = Vector2.Zero;
        public static Vector2 mouseDelta = Vector2.Zero;
        public static MouseButtonEvent mouseBtnEvent;
        public static MouseMoveEvent mouseMoveEvent;

        public static MouseButtonEvent previousBtnEvent;

        public static void OnMouseInput(MouseButtonEvent e)
        {
            mouseBtnEvent = e;
        }

        public static void OnMouseMove(MouseMoveEvent e)
        {
            mouseMoveEvent = e;
            mousePosition = e.position;
            mouseDelta = e.delta;
            
        }

        public static bool IsPressed(MouseButton btn)
        {
            if (btn == mouseBtnEvent.Button)
            {
                if (mouseBtnEvent.IsPressed)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GetMouseDown(MouseButton btn)
        {
            if (btn == mouseBtnEvent.Button)
            {
                if (previousBtnEvent.Button == mouseBtnEvent.Button)
                {
                    if (mouseBtnEvent.Action != InputAction.Release && previousBtnEvent.Action == InputAction.Press)
                    {
                        return false;
                    }
                }

                previousBtnEvent = mouseBtnEvent;
                return mouseBtnEvent.IsPressed;
            }
            return false;
        }


    }
}
