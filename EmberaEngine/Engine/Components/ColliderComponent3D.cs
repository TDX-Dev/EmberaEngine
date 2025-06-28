using EmberaEngine.Engine.Attributes;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Components
{
    public enum ColliderShapeType
    {
        Box,
        Sphere,
        Capsule,
        ConvexHull,
        Mesh
    }

    [ExecuteInPauseMode]
    public class ColliderComponent3D : Component
    {
        public override string Type => nameof(ColliderComponent3D);

        private ColliderShapeType colliderShape = ColliderShapeType.Box;
        private Vector3 size = Vector3.One;
        private float radius = 0.5f;
        private float height = 2.0f; // For capsule
        private Action onColliderChanged = () => { };

        public ColliderShapeType ColliderShape
        {
            get => colliderShape;
            set
            {
                colliderShape = value;
                onColliderChanged.Invoke();
            }
        }

        public Vector3 Size
        {
            get => size;
            set
            {
                size = value;
                onColliderChanged.Invoke();
            }
        }

        public float Radius
        {
            get => radius;
            set
            {
                radius = value;
                onColliderChanged.Invoke();
            }
        }

        public float Height
        {
            get => height;
            set
            {
                height = value;
                onColliderChanged.Invoke();
            }
        }

        public Action OnColliderPropertyChanged
        {
            get => onColliderChanged;
            set => onColliderChanged = value;
        }
    }
}
