using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Components
{
    public class PlayerController3D : Component
    {
        public override string Type => nameof(PlayerController3D);

        CameraComponent3D camera;
        RigidBody3D rigidBody;

        float pitch = 0f;
        float yaw = 0f;
        float mouseSensitivity = 0.1f;

        public override void OnStart()
        {
            rigidBody = gameObject.GetComponent<RigidBody3D>();
            camera = FindCameraInChildren(gameObject);

            gameObject.Scene.OnComponentAdded += OnComponentAddedCallback;

            // Initialize yaw and pitch from current camera rotation
            if (camera != null)
            {
                Vector3 euler = gameObject.transform.Rotation;
                pitch = euler.X;
                yaw = euler.Y;
            }
        }

        private CameraComponent3D FindCameraInChildren(GameObject obj)
        {
            foreach (var child in obj.children)
            {
                var cam = child.GetComponent<CameraComponent3D>();
                if (cam != null)
                    return cam;

                var foundInDescendants = FindCameraInChildren(child);
                if (foundInDescendants != null)
                    return foundInDescendants;
            }

            return null;
        }



        public override void OnUpdate(float dt)
        {
            if (rigidBody == null) return;

            HandleCameraLook(dt);
            HandleMovement(dt);
        }

        private void HandleCameraLook(float dt)
        {
            if (camera == null) return;

            if (!Input.IsPressed(MouseButton.Left)) return;

            Vector2 mouseDelta = Input.mouseDelta;

            // Adjust pitch and yaw
            yaw -= mouseDelta.X * mouseSensitivity;
            pitch -= mouseDelta.Y * mouseSensitivity;

            pitch = Math.Clamp(pitch, -89f, 89f);

            // Apply rotation
            // Apply yaw to the player
            //gameObject.children[0].transform.Rotation = new Vector3(0f, yaw, 0f);

            // Apply pitch to the camera local transform
            camera.gameObject.transform.Rotation = new Vector3(0f, yaw, pitch);

        }


        private void HandleMovement(float dt)
        {
            Vector3 moveDir = Vector3.Zero;

            if (Input.GetKey(Keys.W))
                moveDir += camera.Front; // Forward
            if (Input.GetKey(Keys.S))
                moveDir += -camera.Front;  // Backward
            if (Input.GetKey(Keys.A))
                moveDir += -camera.Right;   // Left
            if (Input.GetKey(Keys.D))
                moveDir += camera.Right;    // Right

            moveDir.Y = 0; // Ensure only horizontal movement
            if (moveDir.LengthSquared > 0)
                moveDir = Vector3.Normalize(moveDir);

            float speed = 20f;
            Vector3 desiredHorizontalVelocity = moveDir * speed;

            Vector3 currentVelocity = rigidBody.GetVelocity();

            // Jump
            if (Input.GetKeyDown(Keys.Space))
            {
                currentVelocity.Y = 10f; // Adjust for jump strength
            }

            Vector3 newVelocity = new Vector3(
                desiredHorizontalVelocity.X,
                currentVelocity.Y,
                desiredHorizontalVelocity.Z
            );

            rigidBody.SetVelocity(newVelocity);
        }

        private void OnComponentAddedCallback(Component comp)
        {
            if (comp.gameObject != gameObject)
                return;

            if (comp is CameraComponent3D cam)
                camera = cam;
            else if (comp is RigidBody3D rb)
                rigidBody = rb;
        }
    }

}