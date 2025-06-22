using System;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Serializing;
using MessagePack;

namespace EmberaEngine.Engine.Components
{
    public abstract partial class Component
    {
        public abstract string Type { get; }
        public Component() { }

        public GameObject gameObject;
        public virtual void OnStart() { }
        public virtual void OnUpdate(float dt) { }

        public virtual void OnDestroy() { }
    }
}

