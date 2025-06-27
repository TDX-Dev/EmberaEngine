using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class PhysicsSpawnerComponent : Component
    {
        public override string Type => nameof(PhysicsSpawnerComponent);

        public float objectCount = 10;

        public override void OnStart()
        {
            for (int i = 0; i < objectCount; i++)
            {
                for (int  j = 0; j < objectCount; j++)
                {
                    for (int k = 0; k < objectCount; k++)
                    {
                        GameObject go = gameObject.Scene.addGameObject("PhysicsObject #" + i);
                        go.transform.Position = new OpenTK.Mathematics.Vector3(k * 2, i * 2 + 50, j * 2);
                        RigidBody3D r = go.AddComponent<RigidBody3D>();
                        go.AddComponent<ColliderComponent3D>();

                        PhysicsMaterial physics = PhysicsMaterial.Default;
                        
                        physics.Friction = 2;
                        physics.MaxRecoveryVelocity = 10000000000000000;
                        physics.SpringDampingRatio = j * j / 10000f;
                        physics.SpringFrequency = 5 + 0.25f * i;

                        r.Mass = 1;
                        r.PhysicsMaterial = physics;
                    }
                }
            }
        }

        public override void OnUpdate(float dt)
        {
            
        }

        public override void OnDestroy()
        {
            
        }
    }
}
