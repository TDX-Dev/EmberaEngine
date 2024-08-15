using EmberaEngine.Engine.Utilities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public class Layer
    {
        // This method is called when the layer is attached to the application
        public virtual void OnAttach() { }
        // This method is called when the layer is removed from the application
        public virtual void OnDetach() { }
        // This method is called every frame to update the logic of the application
        public virtual void OnUpdate(float deltaTime) { }
        // This method is called before the renderer commences rendering, this is for submitting the meshes to be rendered to the renderer
        public virtual void OnRender() { }
        // This method is called once all the rendering is done
        public virtual void OnLateRender() { }
        // This method is called whenever there is a key down event, this includes all keys including control keys
        public virtual void OnKeyDown(KeyboardEvent keyboardEvent) { }
        // This method only contains the text input keys
        public virtual void OnTextInput(TextInputEvent textInputEvent) { }
        // This method is called whenever any mouse button is pressed or released
        public virtual void OnMouseButton(MouseButtonEvent buttonEvent) { }
        // This method is called when any mouse movement is made
        public virtual void OnMouseMove(MouseMoveEvent moveEvent) { }
        // This method is called whenever the application window is resized
        public virtual void OnResize(int width, int height) { }
        // This method is called before the imgui renderer commences rendering
        public virtual void OnGUIRender() { }
    }
}
