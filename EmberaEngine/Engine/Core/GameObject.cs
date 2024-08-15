using System;
using System.Collections.Generic;

using EmberaEngine.Engine.Components;
using static System.Formats.Asn1.AsnWriter;

namespace EmberaEngine.Engine.Core
{
    public class GameObject
    {

        public string Name { get; set; }

        public Transform transform { get; private set; }

        public Scene scene;

        public GameObject parentObject;
        public List<GameObject> children;

        public List<Component> Components;

        private bool DestroyOnLoad = true;

        public GameObject()
        {
            children = new List<GameObject>();
            Components = new List<Component>();

            // This is because the scene object doesnt get assigned to the game object on initialization (obviously!)
            // So to prevent errors on calling ComponentAdded from scene, this bandaid solution has been implemented.
            transform = new Transform();
            transform.gameObject = this;
            Components.Add(transform);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T _component = new();
            _component.gameObject = this;
            Components.Add(_component);
            scene.ComponentAdded(_component);
            return _component;
        }

        public Component AddComponent(Component component)
        {
            Component _component = component;
            _component.gameObject = this;
            Components.Add(_component);
            scene.ComponentAdded(_component);
            return _component;
        }

        public T GetComponent<T>() where T : Component, new()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                if (typeof(T) == Components[i].GetType())
                {
                    return (T)Components[i];
                }
            }
            return null;
        }

        public void OnStart()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnStart();
            }
        }

        public void OnUpdate(float dt)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnUpdate(dt);
            }
        }


        // This method is called when the objects are destroyed when another scene is loaded
        public bool OnDestroyLoad()
        {
            if (DestroyOnLoad)
            {
                for (int i = 0; i < Components.Count; i++)
                {
                    Components[i].OnDestroy();
                }
            }
            return DestroyOnLoad;
        }

        // This is called when the object is requested to be removed
        public void OnDestroy()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].OnDestroy();
            }
        }

    }
}
