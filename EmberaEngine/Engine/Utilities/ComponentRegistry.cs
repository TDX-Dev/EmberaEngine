using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Serializing;
using MessagePack;
using MessagePack.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{
    public static class ComponentRegistry
    {
        private static readonly Dictionary<ushort, Func<IInjectableFormatter<Component>>> _idToFormatter = new();
        private static readonly Dictionary<Type, ushort> _typeToId = new();
        private static readonly Dictionary<ushort, Func<Component>> _idToInstanceFactory = new();

        private static ushort _nextId = 0;

        public static void Register<T>() where T : Component, new()
        {
            var id = _nextId++;
            _typeToId[typeof(T)] = id;

            // Wrap the generic formatter to base type
            var formatter = new ComponentFormatter<T>();
            _idToFormatter[id] = () => new Adapter<T>(formatter);

            _idToInstanceFactory[id] = () => new T();
        }

        public static ushort GetId(Type type) => _typeToId[type];

        public static IInjectableFormatter<Component> GetInjectableFormatter(ushort id)
        {
            return _idToFormatter[id]();
        }

        public static Component CreateInstance(ushort id)
        {
            if (_idToInstanceFactory.TryGetValue(id, out var factory))
                return factory();
            throw new ArgumentException($"No component registered with ID {id}");
        }

        private class Adapter<T> : IInjectableFormatter<Component> where T : Component, new()
        {
            private readonly ComponentFormatter<T> _inner;

            public Adapter(ComponentFormatter<T> inner)
            {
                _inner = inner;
            }

            public Component DeserializeInto(ref MessagePackReader reader, MessagePackSerializerOptions options, Component existingInstance)
            {
                return _inner.DeserializeInto(ref reader, options, (T)existingInstance);
            }

            public void Serialize(ref MessagePackWriter writer, Component value, MessagePackSerializerOptions options)
            {
                if (value is T typedValue)
                {
                    _inner.Serialize(ref writer, typedValue, options);
                }
                else
                {
                    throw new InvalidCastException($"Component is not of expected type {typeof(T)}, but was {value.GetType()}.");
                }
            }
        }

    }



}
