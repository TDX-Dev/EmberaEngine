using EmberaEngine.Engine.Attributes;
using EmberaEngine.Engine.Core;
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

        private Matrix4 localMatrix;
        private Matrix4 worldMatrix;

        private Vector3 prev_position;
        private Vector3 prev_rotation;
        private Vector3 prev_scale;
        private Vector3 prev_globalPosition;
        private Vector3 prev_globalRotation;
        private Vector3 prev_globalScale;

        public bool hasMoved = false;

        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                hasMoved = true;
                UpdateTransform();
            }
        }

        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                hasMoved = true;
                UpdateTransform();
            }
        }

        public Vector3 Scale
        {
            get => scale;
            set
            {
                scale = value;
                hasMoved = true;
                UpdateTransform();
            }
        }

        public Vector3 GlobalPosition
        {
            get => globalPosition;
            set
            {
                if (gameObject.parentObject != null)
                {
                    var parentTransform = gameObject.parentObject.transform;
                    if (parentTransform != null)
                    {
                        Matrix4 parentWorld = parentTransform.GetWorldMatrix();
                        Matrix4 parentInv = parentWorld.Inverted();
                        position = Vector3.TransformPosition(value, parentInv);
                    }
                    else
                    {
                        position = value;
                    }
                }
                else
                {
                    position = value;
                }

                hasMoved = true;
                UpdateTransform();
            }
        }

        public Vector3 GlobalRotation => globalRotation;
        public Vector3 GlobalScale => globalScale;

        public Matrix4 GetLocalMatrix() => localMatrix;
        public Matrix4 GetWorldMatrix() => worldMatrix;

        public void UpdateTransform()
        {
            if (gameObject == null) return;

            // Correct TRS order: Translate * Rotate * Scale
            localMatrix = CreateTRS(position, rotation, scale);

            // Apply parent world matrix
            if (gameObject.parentObject != null)
            {
                var parentTransform = gameObject.parentObject.transform;
                if (parentTransform != null)
                {
                    worldMatrix = localMatrix * parentTransform.GetWorldMatrix();
                }
                else
                {
                    worldMatrix = localMatrix;
                }
            }
            else
            {
                worldMatrix = localMatrix;
            }

            // Extract global components
            globalPosition = worldMatrix.ExtractTranslation();
            globalRotation = worldMatrix.ExtractRotation().ToEulerAngles() * MathHelper.RadiansToDegrees(1.0f);
            globalScale = worldMatrix.ExtractScale();

            // Recursively update children
            foreach (var child in gameObject.children)
            {
                var childTransform = child.GetComponent<Transform>();
                childTransform?.UpdateTransform();
            }
        }

        private Matrix4 CreateTRS(Vector3 pos, Vector3 rotDeg, Vector3 scl)
        {
            Matrix4 translation = Matrix4.CreateTranslation(pos);

            Quaternion rotationQuat = Quaternion.FromEulerAngles(
                MathHelper.DegreesToRadians(rotDeg.X),
                MathHelper.DegreesToRadians(rotDeg.Y),
                MathHelper.DegreesToRadians(rotDeg.Z)
            );
            Matrix4 rotation = Matrix4.CreateFromQuaternion(rotationQuat);

            Matrix4 scaling = Matrix4.CreateScale(scl);

            return translation * rotation * scaling; // T * R * S
        }


        public override void OnStart()
        {
            UpdateTransform();

            prev_position = position;
            prev_rotation = rotation;
            prev_scale = scale;

            prev_globalPosition = globalPosition;
            prev_globalRotation = globalRotation;
            prev_globalScale = globalScale;
        }

        public override void OnUpdate(float dt)
        {
            if (
                prev_position != position ||
                prev_rotation != rotation ||
                prev_scale != scale ||
                prev_globalPosition != globalPosition ||
                prev_globalRotation != globalRotation ||
                prev_globalScale != globalScale
            )
            {
                prev_position = position;
                prev_rotation = rotation;
                prev_scale = scale;

                prev_globalPosition = globalPosition;
                prev_globalRotation = globalRotation;
                prev_globalScale = globalScale;

                hasMoved = true;
            }
            else
            {
                hasMoved = false;
            }
        }
    }
}
