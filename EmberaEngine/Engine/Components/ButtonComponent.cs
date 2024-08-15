using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Core;

namespace EmberaEngine.Engine.Components
{
    public class ButtonComponent : Component
    {
        public override string Type => nameof(ButtonComponent);

        public UIButton button;

        private void UpdateCoordinates()
        {
            button.top = gameObject.transform.position.Y - gameObject.transform.scale.Y / 2;
            button.bottom = gameObject.transform.position.Y + gameObject.transform.scale.Y / 2;
            button.left = gameObject.transform.position.X - gameObject.transform.scale.X / 2;
            button.right = gameObject.transform.position.X + gameObject.transform.scale.X / 2;

            //Console.WriteLine(button.top + " " + button.bottom);
            //Console.WriteLine(button.left +  " " + button.right);
        }

        public ButtonComponent()
        {

        }

        public override void OnStart()
        {
            UIManager.AddButton(this);
            UpdateCoordinates();
        }

        public override void OnUpdate(float dt)
        {
            if (button.IsPressed)
            {
                Console.WriteLine("Pressed");
            }
            UpdateCoordinates();
        }
    }
}
