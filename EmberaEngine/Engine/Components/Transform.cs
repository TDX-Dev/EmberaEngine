using EmberaEngine.Engine.Attributes;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using OpenTK.Mathematics;
using System;

namespace EmberaEngine.Engine.Components
{
    [ExecuteInPauseMode]
    public class Transform : Component
    {
        public override string Type => nameof(Transform);

        private Vector3 position = Vector3.Zero;
        private Vector3 rotation = Vector3.Zero;
        private Vector3 scale = Vector3.One;

        private Vector3 globalPosition = Vector3.Zero;
        private Vector3 globalRotation = Vector3.Zero;
        private Vector3 globalScale = Vector3.One;

        private Matrix4 localMatrix = Matrix4.Identity;
        private Matrix4 worldMatrix = Matrix4.Identity;

        private Vector3 prev_position = Vector3.Zero;
        private Vector3 prev_rotation = Vector3.Zero;
        private Vector3 prev_scale = Vector3.One;

        public bool hasMoved = false;

        public Vector3 Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    hasMoved = true;
                    UpdateTransform();
                }
            }
        }

        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    hasMoved = true;
                    UpdateTransform();
                }
            }
        }

        public Vector3 Scale
        {
            get => scale;
            set
            {
                if (scale != value)
                {
                    scale = value;
                    hasMoved = true;
                    UpdateTransform();
                }
            }
        }

        [IgnoreMember]
        public Vector3 GlobalPosition 
        {
            get => globalPosition;
            set  {
                return;
            }
        }
        
        [IgnoreMember]
        public Vector3 GlobalRotation
        {
            get => globalRotation;
            set {
                globalRotation = value;
            }
        }
        [IgnoreMember]
        public Vector3 GlobalScale => globalScale;

        public Matrix4 GetLocalMatrix() => localMatrix;
        public Matrix4 GetWorldMatrix() => worldMatrix;

        public void SetGlobalTransform(Vector3 globalPosition, Quaternion globalRotation)
        {
            // Apply global position and rotation directly
            this.globalPosition = globalPosition;
            this.globalRotation = Helper.ToDegrees(Helper.ToEulerAngles(globalRotation));

            // Recalculate local matrix from world
            if (gameObject.parentObject != null)
            {
                var parentWorld = gameObject.parentObject.transform.GetWorldMatrix();
                this.localMatrix = worldMatrix * Matrix4.Invert(parentWorld);
            }
            else
            {
                this.localMatrix = worldMatrix;
            }

            // Update local TRS components
            this.position = localMatrix.ExtractTranslation();
            this.rotation = localMatrix.ExtractRotation().ToEulerAngles() * MathHelper.RadiansToDegrees(1f);
            this.scale = localMatrix.ExtractScale();

            UpdateTransform(true); // Recalculate world matrix from updated globals
        }


        public void UpdateTransform(bool forceUpdate = false)
        {
            if (gameObject == null) return;

            Vector3 prevGlobalPosition = globalPosition;
            Vector3 prevGlobalRotation = globalRotation;
            Vector3 prevGlobalScale = globalScale;

            localMatrix = CreateTRS(position, rotation, scale);

            if (gameObject.parentObject != null)
            {
                var parentTransform = gameObject.parentObject.transform;
                worldMatrix = localMatrix * parentTransform.GetWorldMatrix();


            }
            else
            {
                worldMatrix = localMatrix;
            }

            globalPosition = worldMatrix.ExtractTranslation();
            globalRotation = worldMatrix.ExtractRotation().ToEulerAngles() * MathHelper.RadiansToDegrees(1f);
            globalScale = worldMatrix.ExtractScale();

            hasMoved = forceUpdate ||
                       !globalPosition.Equals(prevGlobalPosition) ||
                       !globalRotation.Equals(prevGlobalRotation) ||
                       !globalScale.Equals(prevGlobalScale);

            // Only force child updates if this object moved
            bool childForceUpdate = hasMoved;

            foreach (var child in gameObject.children)
            {
                child.transform.UpdateTransform(childForceUpdate);
            }
        }


        private Matrix4 CreateTRS(Vector3 pos, Vector3 rotDeg, Vector3 scl)
        {
            Vector3 rotRad = Helper.ToRadians(rotDeg);

            Matrix4 scale = Matrix4.CreateScale(scl);

            // Rotation order ZYX
            Matrix4 rotX = Matrix4.CreateRotationX(rotRad.X);
            Matrix4 rotY = Matrix4.CreateRotationY(rotRad.Y);
            Matrix4 rotZ = Matrix4.CreateRotationZ(rotRad.Z);
            Matrix4 rotation = rotZ * rotY * rotX;

            Matrix4 translation = Matrix4.CreateTranslation(pos);

            // Correct order: scale, then rotate, then translate
            return scale * rotation * translation;
        }



        public override void OnStart()
        {
            UpdateTransform();

            prev_position = position;
            prev_rotation = rotation;
            prev_scale = scale;
        }

        public override void OnUpdate(float dt)
        {
            if (
                prev_position != position ||
                prev_rotation != rotation ||
                prev_scale != scale
            )
            {
                prev_position = position;
                prev_rotation = rotation;
                prev_scale = scale;
                UpdateTransform();
                hasMoved = true;
                Console.WriteLine($"[Transform] {gameObject?.Name}: Position={position}, Global={globalPosition}, hasMoved={hasMoved}");
            }
            else
            {
                hasMoved = false;
            }

            // Debugging position drift
           
        }
    }
}
