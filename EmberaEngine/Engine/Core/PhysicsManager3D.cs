using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace EmberaEngine.Engine.Core
{
    public struct PhysicsObjectHandle
    {
        public StaticHandle StaticHandle;
        public BodyHandle BodyHandle;
        public BodyReference BodyReference;
        public bool IsStatic;
    }

    public struct PhysicsShapeInfo
    {
        public TypedIndex Shape;
        public BodyInertia Inertia;
    }

    public struct PhysicsMaterial
    {
        public float Friction;
        public float Restitution;
        public float MaxRecoveryVelocity;
        public SpringSettings SpringSettings;

        public static readonly PhysicsMaterial Default = new PhysicsMaterial
        {
            Friction = 0.1f,
            Restitution = 0f,
            MaxRecoveryVelocity = 2f,
            SpringSettings = new SpringSettings(30, 1)
        };
    }


    public class PhysicsManager3D : IDisposable
    {
        public static readonly System.Numerics.Vector3 GlobalGravity = new(0, -9.81f, 0);
        private readonly Dictionary<string, TypedIndex> shapeCache = new();
        private readonly Dictionary<BodyHandle, Transform> dynamicBodies = new();

        private BufferPool bufferPool;
        private PhysicsPoseIntegratorCallback integratorCallback;
        private PhysicsNarrowPhaseCallback narrowCallback;
        private Simulation simulation;
        private ThreadDispatcher threadDispatcher;
        private SolveDescription solveDescription;
        private const float TimeStep = 1f / 60f;

        public void Initialize()
        {
            bufferPool = new BufferPool();
            integratorCallback = new PhysicsPoseIntegratorCallback(GlobalGravity);
            narrowCallback = new PhysicsNarrowPhaseCallback();
            solveDescription = new(8, 1);
            threadDispatcher = new(Environment.ProcessorCount);
            simulation = Simulation.Create(bufferPool, narrowCallback, integratorCallback, solveDescription);
        }

        public void Update(float dt)
        {
            simulation?.Timestep(TimeStep, threadDispatcher);

            foreach (var kv in dynamicBodies)
            {
                var handle = kv.Key;
                var transform = kv.Value;
                var body = simulation.Bodies.GetBodyReference(handle);
                if (body.Exists)
                {
                    var pose = body.Pose;
                    transform.GlobalPosition = new OpenTK.Mathematics.Vector3(pose.Position.X, pose.Position.Y, pose.Position.Z);
                    transform.Rotation = Helper.ToDegrees(Helper.ToEulerAngles(pose.Orientation)); // Still local!
                }
            }
        }


        public void ApplyForce(BodyHandle handle, Vector3 force)
        {
            if (!simulation.Bodies.BodyExists(handle)) return;

            var body = simulation.Bodies.GetBodyReference(handle);
            body.Awake = true; // Ensure the body is awake before applying forces
            body.Velocity.Linear += force * simulation.Bodies.GetDescription(handle).LocalInertia.InverseMass;
        }


        public void ApplyImpulse(BodyHandle handle, Vector3 impulse)
        {
            if (!simulation.Bodies.BodyExists(handle)) return;

            var body = simulation.Bodies.GetBodyReference(handle);
            body.Awake = true; // Wake up before applying
            body.Velocity.Linear += impulse * simulation.Bodies.GetDescription(handle).LocalInertia.InverseMass;
        }


        public void ApplyTorque(BodyHandle handle, Vector3 torque)
        {
            if (!simulation.Bodies.BodyExists(handle)) return;

            var body = simulation.Bodies.GetBodyReference(handle);
            body.Awake = true; // Wake up before applying
            body.Velocity.Angular += torque;
        }

        public void SetVelocity(BodyHandle handle, Vector3 velocity)
        {
            if (!simulation.Bodies.BodyExists(handle)) return;

            var body = simulation.Bodies.GetBodyReference(handle);
            body.Awake = true;
            body.Velocity.Linear = velocity;
        }

        public Vector3 GetVelocity(BodyHandle handle)
        {
            if (!simulation.Bodies.BodyExists(handle)) return Vector3.Zero;

            var body = simulation.Bodies.GetBodyReference(handle);
            return body.Velocity.Linear;
        }




        public bool StaticExists(StaticHandle h) => simulation.Statics.StaticExists(h);
        public bool DynamicExists(BodyHandle h) => simulation.Bodies.BodyExists(h);

        public PhysicsObjectHandle AddPhysicsObject(Transform tf, RigidBody3D rb, ColliderComponent3D col)
        {
            var shapeInfo = CreateShape(rb, col);
            var pos = Helper.ToNumerics3(tf.Position);
            var rot = Helper.ToQuaternion(Helper.ToNumerics3(Helper.ToRadians(tf.Rotation)));

            StaticHandle sHandle = default;
            BodyHandle bHandle = default;

            switch (rb.RigidbodyType)
            {
                case Rigidbody3DType.Static:
                    sHandle = simulation.Statics.Add(new StaticDescription(pos, rot, shapeInfo.Shape));
                    break;

                case Rigidbody3DType.Dynamic:
                    {
                        var dynamicDesc = BodyDescription.CreateDynamic(pos, shapeInfo.Inertia, shapeInfo.Shape, 0.01f);
                        bHandle = simulation.Bodies.Add(dynamicDesc);

                        var bodyRef = simulation.Bodies.GetBodyReference(bHandle);
                        bodyRef.Pose.Orientation = rot; // Apply rotation

                        dynamicBodies[bHandle] = tf;
                        break;
                    }

                case Rigidbody3DType.Kinematic:
                    var kinDesc = BodyDescription.CreateKinematic(pos, shapeInfo.Shape, 0.01f);
                    //.WithMaterial(rb.Friction, rb.Restitution);
                    bHandle = simulation.Bodies.Add(kinDesc);
                    break;
            }

            return new PhysicsObjectHandle
            {
                StaticHandle = sHandle,
                BodyHandle = bHandle,
                IsStatic = rb.RigidbodyType == Rigidbody3DType.Static,
                BodyReference = rb.RigidbodyType == Rigidbody3DType.Static
                    ? default
                    : simulation.Bodies.GetBodyReference(bHandle)
            };
        }

        public void RemovePhysicsObject(PhysicsObjectHandle handle)
        {
            if (handle.IsStatic)
                simulation.Statics.Remove(handle.StaticHandle);
            else
            {
                simulation.Bodies.Remove(handle.BodyHandle);
                dynamicBodies.Remove(handle.BodyHandle);
            }
        }

        private PhysicsShapeInfo CreateShape(RigidBody3D rb, ColliderComponent3D col)
        {
            var key = col.ColliderShape + ":" + col.Radius + ":" + col.Height + ":" + col.Size;
            if (!shapeCache.TryGetValue(key, out var shapeIndex))
            {
                switch (col.ColliderShape)
                {
                    case ColliderShapeType.Sphere:
                        shapeIndex = simulation.Shapes.Add(new Sphere(col.Radius));
                        break;
                    case ColliderShapeType.Box:
                        shapeIndex = simulation.Shapes.Add(new Box(col.Size.X, col.Size.Y, col.Size.Z));
                        break;
                    case ColliderShapeType.Capsule:
                        shapeIndex = simulation.Shapes.Add(new Capsule(col.Radius, col.Height));
                        break;
                    //case ColliderShapeType.ConvexHull:
                    //    // Provide mesh vertices array from collider or asset loader
                    //    var hull = new ConvexHull(col.GetConvexHullVertices());
                    //    shapeIndex = simulation.Shapes.Add(hull);
                    //    break;
                    //case ColliderShapeType.Mesh:
                    //    var mesh = new MeshCollider(col.GetMeshVertices(), col.GetMeshIndices());
                    //    shapeIndex = simulation.Shapes.Add(mesh);
                    //    break;
                    default:
                        throw new InvalidOperationException("Unknown collider shape");
                }
                shapeCache[key] = shapeIndex;
            }

            BodyInertia inertia = col.ColliderShape switch
            {
                ColliderShapeType.Sphere => new Sphere(col.Radius).ComputeInertia(rb.Mass),
                ColliderShapeType.Box => new Box(col.Size.X, col.Size.Y, col.Size.Z).ComputeInertia(rb.Mass),
                ColliderShapeType.Capsule => new Capsule(col.Radius, col.Height).ComputeInertia(rb.Mass),
                _ => new BodyInertia()
            };

            return new PhysicsShapeInfo { Shape = shapeIndex, Inertia = inertia };
        }

        public void Dispose()
        {
            simulation.Dispose();
            threadDispatcher.Dispose();
            bufferPool.Clear();
        }
    }

    public struct PhysicsPoseIntegratorCallback : IPoseIntegratorCallbacks
    {
        public Vector3 Gravity;
        private Vector3Wide gravityWideDt;

        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public bool AllowSubstepsForUnconstrainedBodies => false;
        public bool IntegrateVelocityForKinematics => false;

        public PhysicsPoseIntegratorCallback(Vector3 gravity)
        {
            Gravity = gravity;
            gravityWideDt = default;
        }

        public void Initialize(Simulation simulation) { }

        public void PrepareForIntegration(float dt)
        {
            // Multiply gravity by dt and broadcast into a wide vector
            gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        public void IntegrateVelocity(
            Vector<int> bodyIndices,
            Vector3Wide position,
            QuaternionWide orientation,
            BodyInertiaWide localInertia,
            Vector<int> integrationMask,
            int workerIndex,
            Vector<float> dt,
            ref BodyVelocityWide velocity)
        {
            // Apply gravity to the linear velocity
            velocity.Linear += gravityWideDt;
        }
    }

    public struct PhysicsNarrowPhaseCallback : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation sim) { }

        public bool AllowContactGeneration(int wi, CollidableReference a, CollidableReference b, ref float speculativeMargin)
            => a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) => true;

        public bool ConfigureContactManifold<TManifold>(
    int workerIndex,
    CollidablePair pair,
    ref TManifold manifold,
    out PairMaterialProperties pairMaterial)
    where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial.FrictionCoefficient = 0.1f; // default or from user-defined data
            pairMaterial.MaximumRecoveryVelocity = 2f;
            pairMaterial.SpringSettings = new SpringSettings(30, 1);
            return true;
        }


        public bool ConfigureContactManifold(int wi, CollidablePair pair, int idxA, int idxB, ref ConvexContactManifold manifold)
            => true;

        public void Dispose() { }

    }
}
