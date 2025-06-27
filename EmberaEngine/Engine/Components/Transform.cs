using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using System;

namespace EmberaEngine.Engine.Components
{
    public class Transform : Component
    {
        public override string Type => nameof(Transform);

        private Vector3 position = Vector3.Zero;
        private Vector3 rotation = Vector3.Zero; // Euler degrees
        private Vector3 scale = Vector3.One;

        private Vector3 globalPosition = Vector3.Zero;
        private Vector3 globalRotation = Vector3.Zero;
        private Vector3 globalScale = Vector3.One;

        private Matrix4 localMatrix = Matrix4.Identity;
        private Matrix4 worldMatrix = Matrix4.Identity;

        private Vector3 prevPosition;
        private Vector3 prevRotation;
        private Vector3 prevScale;

        public bool hasMoved = false;

        public Vector3 Position
        {
            get => position;
            set { position = value; UpdateTransform(); }
        }

        public Vector3 Rotation
        {
            get => rotation;
            set { rotation = value; UpdateTransform(); }
        }

        public Vector3 Scale
        {
            get => scale;
            set { scale = value; UpdateTransform(); }
        }

        public Vector3 GlobalPosition => globalPosition;
        public Vector3 GlobalRotation => globalRotation;
        public Vector3 GlobalScale => globalScale;

        public Matrix4 GetLocalMatrix() => localMatrix;
        public Matrix4 GetWorldMatrix() => worldMatrix;

        public Vector3 Forward => Vector3.TransformNormal(-Vector3.UnitZ, worldMatrix).Normalized();
        public Vector3 Up => Vector3.TransformNormal(Vector3.UnitY, worldMatrix).Normalized();
        public Vector3 Right => Vector3.TransformNormal(Vector3.UnitX, worldMatrix).Normalized();

        public Vector3 LocalEulerRadians => new Vector3(MathHelper.DegreesToRadians(rotation.X), MathHelper.DegreesToRadians(rotation.Y), MathHelper.DegreesToRadians(rotation.Z));

        public void UpdateTransform()
        {
            // Create local matrix from S * R * T
            var rotX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(rotation.X));
            var rotY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y));
            var rotZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));
            var rotationMatrix = rotZ * rotY * rotX;

            localMatrix =
                Matrix4.CreateScale(scale) *
                rotationMatrix *
                Matrix4.CreateTranslation(position);

            // Combine with parent world transform if any
            if (gameObject?.parentObject?.transform != null)
                worldMatrix = localMatrix * gameObject.parentObject.transform.worldMatrix;
            else
                worldMatrix = localMatrix;

            // Extract global transform values
            globalPosition = worldMatrix.ExtractTranslation();
            globalRotation = Helper.ToDegrees(worldMatrix.ExtractRotation().ToEulerAngles());
            globalScale = worldMatrix.ExtractScale();

            // Propagate transform update to children
            foreach (var child in gameObject.children)
            {
                var childTransform = child.GetComponent<Transform>();
                childTransform?.UpdateTransform();
            }

            hasMoved = true;
        }

        public void SetGlobalPosition(Vector3 newGlobalPos)
        {
            if (gameObject?.parentObject?.transform != null)
            {
                var parentInv = gameObject.parentObject.transform.GetWorldMatrix().Inverted();
                position = Vector3.TransformPosition(newGlobalPos, parentInv);
            }
            else
            {
                position = newGlobalPos;
            }

            UpdateTransform();
        }

        public override void OnStart()
        {
            prevPosition = position;
            prevRotation = rotation;
            prevScale = scale;
            UpdateTransform();
            hasMoved = true;
        }

        public override void OnUpdate(float dt)
        {
            bool changed =
                prevPosition != position ||
                prevRotation != rotation ||
                prevScale != scale;

            if (changed)
            {
                UpdateTransform();
                prevPosition = position;
                prevRotation = rotation;
                prevScale = scale;
            }
            else
            {
                hasMoved = false;
            }
        }
    }

}
