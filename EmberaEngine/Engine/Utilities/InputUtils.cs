using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public struct KeyboardEvent
    {
        public Keys Key;

        public int scanCode;
        public string modifiers;

        public bool Caps;
    }

    public struct MouseMoveEvent
    {
        public Vector2 position;
        public Vector2 delta;
    }

    public struct MouseButtonEvent
    {
        public MouseButton Button;
        public InputAction Action;
        public KeyModifiers Modifiers;
        public bool IsPressed => Action != InputAction.Release; 
    }

    public struct TextInputEvent
    {
        public int Unicode;
    }

    internal class InputUtils
    {
    }
}
