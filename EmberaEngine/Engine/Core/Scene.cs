using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Rendering;
using MessagePack;
using nkast.Aether.Physics2D.Common;

namespace EmberaEngine.Engine.Core
{
    public class Scene : IDisposable
    {
        private bool _disposed;

        public string Name;
        public Guid Id = Guid.NewGuid();

        public List<GameObject> GameObjects { get; set; }

        [IgnoreMember]
        public List<CameraComponent3D> Cameras;

        [IgnoreMember]
        public PhysicsManager2D PhysicsManager2D;
        [IgnoreMember]
        public PhysicsManager3D PhysicsManager3D;

        [IgnoreMember]
        public bool IsPlaying = false;

        public event Action<Component> OnComponentAdded = (c) => {};

        public event Action<Component> OnComponentRemoved = (c) => {};

        public Scene()
        {
            GameObjects = new List<GameObject>();
            PhysicsManager2D = new PhysicsManager2D();
            PhysicsManager3D = new PhysicsManager3D();
        }

        public void Initialize()
        {
            Cameras = new List<CameraComponent3D>();
            PhysicsManager2D.Initialize();
            PhysicsManager3D.Initialize();
        }

        public GameObject addGameObject(string name)
        {
            GameObject gameObject = new GameObject();
            gameObject.Name = name;
            gameObject.Scene = this;
            gameObject.Initialize();
            GameObjects.Add(gameObject);
            return gameObject;
        }

        public void addGameObject(GameObject gameObject)
        {
            gameObject.Scene = this;
            GameObjects.Add(gameObject);
        }

        public T GetComponent<T>() where T : Component, new()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                T component = GameObjects[i].GetComponent<T>();

                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }

        public List<Component> GetComponents()
        {
            List<Component> components = new List<Component>();

            for (int i = 0; i < GameObjects.Count; i++)
            {
                components.AddRange(GameObjects[i].GetComponentsRecursive());
            }
            return components;
        }

        public List<T> GetComponentsOfType<T>() where T : Component, new()
        {
            List<T> components = new List<T>();

            for (int i = 0; i < GameObjects.Count; i++)
            {
                T component = GameObjects[i].GetComponent<T>();

                if (component != null)
                {
                    components.Add(component);
                }
            }
            return components;
        }

        public List<Component> GetAllComponentsOfType(Type componentType)
        {
            List<Component> components = new();

            foreach (var go in GameObjects)
            {
                var component = go.GetComponent(componentType);
                if (component != null)
                {
                    components.Add(component);
                }
            }

            return components;
        }


        public void removeGameObject(GameObject gameObject)
        {
            gameObject.OnDestroy();
            GameObjects.Remove(gameObject);
        }

        public void SetMainCamera(CameraComponent3D camera)
        {
            int index = this.Cameras.IndexOf(camera);
            for (int i = 0; i < this.Cameras.Count; i++)
            {
                if (i == index) { continue; }

                this.Cameras[i].isDefault = false;
            }

            Renderer3D.SetRenderCamera(camera.camera);
        }

        public void AddCamera(CameraComponent3D camera)
        {
            this.Cameras.Add(camera);
        }
        
        public void RemoveCamera(CameraComponent3D camera)
        {
            this.Cameras.Remove(camera);
        }

        public void Destroy()
        {
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.OnDestroy();
            }
            PhysicsManager3D.Dispose();
        }

        public void Play()
        {
            IsPlaying = true;
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnStart();
            }
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void OnUpdate(float dt)
        {
            if (!IsPlaying) { return; }
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(dt);
            }
            PhysicsManager2D.Update(dt);
            PhysicsManager3D.Update(dt);
        }

        public void ComponentAdded(Component component)
        {
            OnComponentAdded.Invoke(component);

            if (IsPlaying)
            {
                component.OnStart();
            }
        }

        public void ComponentRemoved(Component component)
        {
            OnComponentRemoved.Invoke(component);
        }

        public void OnResize(float width, float height)
        {
            //for (int i = 0; i < Cameras.Count; i++)
            //{
            //    Cameras[i].
            //}
        }
        protected virtual void Dispose(bool disposing)
        {
            // check if already disposed
            if (!_disposed)
            {
                if (disposing)
                {
                    for (int i = 0; i < GameObjects.Count; i++)
                    {
                        GameObjects[i].OnDestroy();
                    }

                    PhysicsManager3D.Dispose();
                    GameObjects.Clear();
                }
                // set the bool value to true
                _disposed = true;
            }
        }

        // The consumer object can call
        // the below dispose method
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Scene()
        {

            Dispose(false);
        }
    }
}
