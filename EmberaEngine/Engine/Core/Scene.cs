using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Components;
using nkast.Aether.Physics2D.Common;

namespace EmberaEngine.Engine.Core
{
    public class Scene
    {

        public List<GameObject> GameObjects;
        public PhysicsManager2D PhysicsManager2D;
        public bool IsPlaying = false;

        public event Action<Component> OnComponentAdded = (c) => {};

        public Scene()
        {
            GameObjects = new List<GameObject>();
            PhysicsManager2D = new PhysicsManager2D();
        }

        public void Initialize()
        {
            PhysicsManager2D.Initialize();
        }

        public GameObject addGameObject(string name)
        {
            GameObject gameObject = new GameObject();
            gameObject.Name = name;
            gameObject.scene = this;
            GameObjects.Add(gameObject);
            return gameObject;
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

        public void removeGameObject(GameObject gameObject)
        {
            gameObject.OnDestroy();
            GameObjects.Remove(gameObject);
        }

        public void Play()
        {
            IsPlaying = true;
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.OnStart();
            }
        }

        public void OnUpdate(float dt)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(dt);
            }

            PhysicsManager2D.Update(dt);
        }

        public void ComponentAdded(Component component)
        {
            OnComponentAdded.Invoke(component);

            if (IsPlaying)
            {
                component.OnStart();
            }
        } 
    }
}
