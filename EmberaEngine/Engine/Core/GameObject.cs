using System;
using System.Collections.Generic;

using EmberaEngine.Engine.Components;
using static System.Formats.Asn1.AsnWriter;

namespace EmberaEngine.Engine.Core
{
    public class GameObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name;

        public Transform transform { get; set; }

        private Scene scene;

        public GameObject parentObject;
        public List<GameObject> children;

        public List<Component> Components;

        private bool DestroyOnLoad = true;

        public Scene Scene
        {
            get { return scene; }
            set
            {
                scene = value;
                SetScene(value);
            }
        }

        public GameObject()
        {
            children = new List<GameObject>();
            Components = new List<Component>();

            // This is because the scene object doesnt get assigned to the game object on initialization (obviously!)
            // So to prevent errors on calling ComponentAdded from scene, this bandaid solution has been implemented.
        }

        public void Initialize()
        {
            transform = new Transform();
            transform.gameObject = this;
            Components.Add(transform);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            T _component = new();
            _component.gameObject = this;
            Components.Add(_component);
            scene?.ComponentAdded(_component);
            return _component;
        }

        public Component AddComponent(Component component)
        {
            Component _component = component;
            _component.gameObject = this;
            Components.Add(_component);
            scene?.ComponentAdded(_component);
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

        public void RemoveComponent<T>() where T : Component
        {
            for (int i = 0;i < Components.Count;i++)
            {
                if (typeof(T) == Components[i].GetType())
                {
                    scene?.ComponentRemoved(Components[i]);
                    Components[i].OnDestroy();
                    Components.RemoveAt(i);
                }
            }
        }

        public void RemoveComponent(Component component)
        {
            if (Components.Contains(component))
            {
                scene?.ComponentRemoved(component);
                component.OnDestroy();
                Components.Remove(component);
            }

        }

        public void SetScene(Scene scene)
        {
            if (children.Count == 0) { return; }

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Scene = scene;
            }
        }

        public List<Component> GetComponents()
        {
            return Components;
        }

        public List<Component> GetComponentsRecursive()
        {
            List<Component> result = new List<Component>();
            CollectComponentsRecursive(this, result);
            return result;
        }

        private void CollectComponentsRecursive(GameObject obj, List<Component> list)
        {
            list.AddRange(obj.Components);

            foreach (var child in obj.children)
            {
                CollectComponentsRecursive(child, list);
            }
        }


        public void AddChild(GameObject gameObject)
        {
            gameObject.SetParent(this);
            this.children.Add(gameObject);
        }

        public void SetParent(GameObject gameObject)
        {
            this.parentObject = gameObject;
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


            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].OnUpdate(dt);
                }
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

            if (children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].OnDestroy();
                }
            }
        }

    }
}
