using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Serializing
{
    public class SceneSerializer
    {
        public static Scene currentDeserializingScene;
        public static GameObject currentDeserializingGameObject;
        public static bool isDeserializing = false;


        public static Scene Serialize(Scene scene)
        {
            var resolver = CompositeResolver.Create(
                new IMessagePackFormatter[]
                {
                    new SceneFormatter(),
                new GameObjectFormatter(),
                new Vector2Formatter(),
                new Vector3Formatter(),
                new Vector4Formatter()
                },
                new IFormatterResolver[]
                {
                ContractlessStandardResolver.Instance // Use Standard or Contractless if you have no attributes
                }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);//.WithCompression(MessagePackCompression.Lz4BlockArray);

            // Serialize scene
            var binary = MessagePackSerializer.Serialize(scene, options);

            // Convert to JSON
            var json = MessagePackSerializer.ConvertToJson(binary);
            //Console.WriteLine(json);
            var prettyJson = JToken.Parse(json).ToString(Formatting.Indented);
            Console.WriteLine(prettyJson);
            isDeserializing = true;
            Scene scene1 = MessagePackSerializer.Deserialize<Scene>(binary, options);
            //Console.WriteLine(scene1.GameObjects.Count);
            isDeserializing = false;

            return new Scene();// MessagePackSerializer.Deserialize<Scene>(binary, options);
        }
    }

    public class SceneFormatter : IMessagePackFormatter<Scene>
    {
        public void Serialize(ref MessagePackWriter writer, Scene value, MessagePackSerializerOptions options)
        {
            // We'll serialize GameObjects and IsPlaying
            writer.WriteMapHeader(2);

            writer.Write("GameObjects");
            options.Resolver.GetFormatterWithVerify<List<GameObject>>().Serialize(ref writer, value.GameObjects, options);

            writer.Write("IsPlaying");
            writer.Write(value.IsPlaying);
        }

        public Scene Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadMapHeader();

            List<GameObject> gameObjects = null;
            bool isPlaying = false;

            var scene = new Scene();

            SceneSerializer.currentDeserializingScene = scene;

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                switch (key)
                {
                    case "GameObjects":
                        gameObjects = options.Resolver.GetFormatterWithVerify<List<GameObject>>().Deserialize(ref reader, options);
                        break;

                    case "IsPlaying":
                        isPlaying = reader.ReadBoolean();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            scene.GameObjects = gameObjects ?? new List<GameObject>();
            scene.IsPlaying = isPlaying;

            // Initialize fields that are ignored in serialization
            scene.Initialize();

            // Fix parent references for GameObjects (if needed)
            foreach (var go in scene.GameObjects)
            {
                go.Scene = scene;
                FixParentReferences(go);
            }

            return scene;
        }

        private void FixParentReferences(GameObject gameObject)
        {
            if (gameObject.children != null)
            {
                foreach (var child in gameObject.children)
                {
                    child.parentObject = gameObject;
                    FixParentReferences(child);
                }
            }
        }
    }


    public class GameObjectFormatter : IMessagePackFormatter<GameObject>
    {
        public void Serialize(ref MessagePackWriter writer, GameObject value, MessagePackSerializerOptions options)
        {
            var components = value.GetComponents();

            writer.WriteMapHeader(5);

            writer.Write("NAME");
            writer.Write(value.Name);

            writer.Write("GUID");
            writer.Write(value.Id.ToString());

            writer.Write("PARENT_GUID");
            writer.Write(value.parentObject?.Id.ToString() ?? "");

            writer.Write("CHILDREN");
            options.Resolver.GetFormatterWithVerify<List<GameObject>>().Serialize(ref writer, value.children, options);

            writer.Write("COMPONENTS");
            writer.WriteArrayHeader(components.Count);
            foreach (var comp in components)
            {
                ushort id = ComponentRegistry.GetId(comp.GetType());

                writer.WriteMapHeader(3);
                writer.Write("ID");
                writer.Write(id);

                writer.Write("TYPE");
                writer.Write(comp.Type);

                writer.Write("DATA");
                var formatter = ComponentRegistry.GetInjectableFormatter(id);

                // cast to proper formatter type
                // this is safe because ComponentFormatter<T> implements IInjectableFormatter<T>
                formatter.Serialize(ref writer, comp, options); // ⚠️ won't compile — see below

            }
        }

        public GameObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {

            var count = reader.ReadMapHeader();

            string name = null;
            Guid id = Guid.Empty;
            Guid? parentId = null;
            List<GameObject> children = null;
            List<(ushort, Component)> deserializedComponents = new();

            var go = new GameObject(); // Create GameObject first

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();

                switch (key)
                {
                    case "NAME":
                        name = reader.ReadString();
                        break;
                    case "GUID":
                        id = Guid.Parse(reader.ReadString());
                        break;
                    case "PARENT_GUID":
                        var parentStr = reader.ReadString();
                        parentId = string.IsNullOrEmpty(parentStr) ? null : Guid.Parse(parentStr);
                        break;
                    case "CHILDREN":
                        children = options.Resolver.GetFormatterWithVerify<List<GameObject>>().Deserialize(ref reader, options);
                        break;
                    case "COMPONENTS":
                        {
                            int compCount = reader.ReadArrayHeader();
                            for (int j = 0; j < compCount; j++)
                            {
                                var mapCount = reader.ReadMapHeader();
                                ushort compId = 0;
                                string type = null;
                                Component comp = null;

                                for (int k = 0; k < mapCount; k++)
                                {
                                    var mapKey = reader.ReadString();
                                    switch (mapKey)
                                    {
                                        case "ID":
                                            compId = reader.ReadUInt16();
                                            break;
                                        case "TYPE":
                                            type = reader.ReadString(); // can be skipped
                                            break;
                                        case "DATA":
                                            Console.WriteLine("datapass");
                                            reader.Skip();
                                            comp = ComponentRegistry.CreateInstance(compId);
                                            comp.gameObject = go;

                                            var formatter = ComponentRegistry.GetInjectableFormatter(compId);
                                            //comp = formatter.DeserializeInto(ref reader, options, comp);
                                            break;
                                        default:
                                            reader.Skip();
                                            break;
                                    }
                                }

                                if (comp != null)
                                    deserializedComponents.Add((compId, comp));
                            }
                            break;
                        }



                }
            }
            go.RemoveComponent<Transform>();
            go.Name = name;
            go.Id = id;
            go.children = children ?? new List<GameObject>();

            // Add components *after* setting up GameObject
            foreach (var (compId, comp) in deserializedComponents)
            {
                comp.gameObject = go;

                // Deserialize into the component using correct formatter with injected GameObject
                //var formatter = ComponentRegistry.GetFormatter(compId, go);
                //formatter.Deserialize(ref reader, options);

                go.AddComponent(comp); // optional: if you're using an AddComponent API
            }


            return go;
        }
    }

    public class ComponentFormatter<T> : IInjectableFormatter<T> where T : Component, new()
    {
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, options);
        }

        public T DeserializeInto(ref MessagePackReader reader, MessagePackSerializerOptions options, T existingInstance)
        {
            Console.WriteLine("Deserializing into!");
            Console.WriteLine(existingInstance.gameObject);
            var raw = options.Resolver.GetFormatterWithVerify<T>();
            var temp = raw.Deserialize(ref reader, options);

            // Inject back the GameObject reference
            temp.gameObject = existingInstance.gameObject;
            return temp;
        }
    }



    public interface IInjectableFormatter<T>
    {
        void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options);

        T DeserializeInto(ref MessagePackReader reader, MessagePackSerializerOptions options, T existingInstance);
    }



    public class Vector2Formatter : IMessagePackFormatter<Vector2>
    {
        public void Serialize(ref MessagePackWriter writer, Vector2 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.X);
            writer.Write(value.Y);
        }

        public Vector2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            return new Vector2(x, y);
        }
    }

    public class Vector3Formatter : IMessagePackFormatter<Vector3>
    {
        public void Serialize(ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public Vector3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            return new Vector3(x, y, z);
        }
    }

    public class Vector4Formatter : IMessagePackFormatter<Vector4>
    {
        public void Serialize(ref MessagePackWriter writer, Vector4 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
            writer.Write(value.W);
        }

        public Vector4 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            var x = reader.ReadSingle();
            var y = reader.ReadSingle();
            var z = reader.ReadSingle();
            var w = reader.ReadSingle();
            return new Vector4(x, y, z, w);
        }
    }



}
