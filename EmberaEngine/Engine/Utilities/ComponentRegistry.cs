using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Serializing;
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
        private static readonly Dictionary<ushort, Func<IMessagePackFormatter<Component>>> _idToFormatter = new();
        private static readonly Dictionary<Type, ushort> _typeToId = new();
        private static readonly Dictionary<ushort, Func<Component>> _idToInstanceFactory = new();

        private static ushort _nextId = 0;

        public static void Register<T>(Func<IMessagePackFormatter<T>> formatterFactory) where T : Component, new()
        {
            var id = _nextId++;
            _typeToId[typeof(T)] = id;

            // Register adapter around generated formatter
            _idToFormatter[id] = () => new Adapter<T>(formatterFactory());

            // Register constructor factory
            _idToInstanceFactory[id] = () => new T();
        }

        public static ushort GetId(Type type)
            => _typeToId.TryGetValue(type, out var id)
                ? id
                : throw new KeyNotFoundException($"Component type not registered: {type}");

        public static IMessagePackFormatter<Component> GetFormatter(ushort id)
            => _idToFormatter.TryGetValue(id, out var factory)
                ? factory()
                : throw new KeyNotFoundException($"Formatter not registered for ID: {id}");

        public static Component CreateInstance(ushort id)
            => _idToInstanceFactory.TryGetValue(id, out var factory)
                ? factory()
                : throw new ArgumentException($"No component registered with ID {id}");
    }



}
