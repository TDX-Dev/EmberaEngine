using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class ProjectileComponent : Component
    {
        public override string Type => nameof(ProjectileComponent);

        public override void OnStart()
        {

        }

        public override void OnUpdate(float dt)
        {
            if (Input.GetKeyDown(Utilities.Keys.Space))
            {
                Console.WriteLine("heya");
            } 
        }

        public override void OnDestroy()
        {
            
        }

    }
}
