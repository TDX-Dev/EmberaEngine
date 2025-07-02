using EmberaEngine.Core;
using EmberaEngine.Engine.AssetHandling;
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
    public static class FormatterRegistry
    {
        public static readonly List<IMessagePackFormatter> Formatters = new();

        public static void Register(IMessagePackFormatter formatter)
        {
            Formatters.Add(formatter);
        }
    }

    public class FormatterRegistryResolver : IFormatterResolver
    {
        public static readonly FormatterRegistryResolver Instance = new();

        private readonly Dictionary<Type, object> _formatters = new();

        private FormatterRegistryResolver()
        {
            RegisterFormatter(new SceneFormatter());
            RegisterFormatter(new GameObjectFormatter());
            RegisterFormatter(new SceneMeshFormatter());
            RegisterFormatter(new Vector2Formatter());
            RegisterFormatter(new Vector3Formatter());
            RegisterFormatter(new Vector4Formatter());

            foreach (var formatter in FormatterRegistry.Formatters)
            {
                RegisterFormatter(formatter);
            }
        }

        private void RegisterFormatter(object formatter)
        {
            var formatterType = formatter.GetType();

            foreach (var iface in formatterType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IMessagePackFormatter<>))
                {
                    var targetType = iface.GetGenericArguments()[0];
                    _formatters[targetType] = formatter;
                    break;
                }
            }
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (_formatters.TryGetValue(typeof(T), out var formatter))
            {
                return (IMessagePackFormatter<T>)formatter;
            }
            return null;
        }
    }


    public class SceneSerializer
    {
        public static Dictionary<string, GameObject> gameObjectGUIDReference;


        public static byte[] Serialize(Scene scene)
        {

            var resolver = CompositeResolver.Create(
                new IFormatterResolver[]
                {
                    FormatterRegistryResolver.Instance,
                    ContractlessStandardResolver.Instance
                }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);//.WithCompression(MessagePackCompression.Lz4BlockArray);
            try
            {
                return MessagePackSerializer.Serialize(scene, options);
            } catch (Exception ex)
            {
                Console.WriteLine("[Scene Deserializer]: An error occurred while deserializing: " + ex.Message);
            }

            return new byte[0];
        }

        public static Scene DeSerialize(byte[] binary)
        {
            gameObjectGUIDReference = new Dictionary<string, GameObject>();

            var resolver = CompositeResolver.Create(
                new IFormatterResolver[]
                {
                                FormatterRegistryResolver.Instance,
                                ContractlessStandardResolver.Instance
                }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            try
            {
                return MessagePackSerializer.Deserialize<Scene>(binary, options);
            } catch (Exception ex)
            {
                Console.WriteLine("[Scene Deserializer]: An error occurred while deserializing: " + ex.Message);
            }

            return new Scene();
        }
    }

    public class SceneFormatter : IMessagePackFormatter<Scene>
    {
        public void Serialize(ref MessagePackWriter writer, Scene value, MessagePackSerializerOptions options)
        {
            writer.WriteMapHeader(3);

            writer.Write("GUID");
            writer.Write(value.Id.ToString());

            writer.Write("NAME");
            writer.Write(value.Name);

            writer.Write("GameObjects");
            options.Resolver.GetFormatterWithVerify<List<GameObject>>().Serialize(ref writer, value.GameObjects, options);
            
        }

        public Scene Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadMapHeader();

            List<GameObject> gameObjects = null;
            Guid guid = new Guid();

            var scene = new Scene();

            for (int i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                switch (key)
                {
                    case "GameObjects":
                        gameObjects = options.Resolver.GetFormatterWithVerify<List<GameObject>>().Deserialize(ref reader, options);
                        break;

                    case "GUID":
                        guid = Guid.Parse(reader.ReadString());
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            scene.GameObjects = gameObjects ?? new List<GameObject>();
            scene.Id = guid;

            scene.Initialize();

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
            var components = value.GetComponents()
                .Where(comp => comp != value.transform)
                .ToList();

            writer.WriteMapHeader(6);

            writer.Write("NAME");
            writer.Write(value.Name);

            writer.Write("GUID");
            writer.Write(value.Id.ToString());

            writer.Write("PARENT_GUID");
            writer.Write(value.parentObject?.Id.ToString() ?? "");

            writer.Write("CHILDREN");
            options.Resolver.GetFormatterWithVerify<List<GameObject>>().Serialize(ref writer, value.children, options);

            writer.Write("TRANSFORM");
            MessagePackSerializer.Serialize(ref writer, value.transform, options);

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
                var formatter = ComponentRegistry.GetFormatter(id);
                formatter.Serialize(ref writer, comp, options);
            }
        }

        public GameObject Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            try
            {
                //Console.WriteLine(reader.ReadSingle());
                var count = reader.ReadMapHeader();

                string name = null;
                Guid id = Guid.Empty;
                Guid? parentId = null;
                List<GameObject> children = null;
                Transform transform = null;
                List<(ushort, Component)> deserializedComponents = new();

                var go = new GameObject();

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
                            go.Id = id;
                            SceneSerializer.gameObjectGUIDReference.Add(id.ToString(), go);
                            break;
                        case "PARENT_GUID":
                            var parentStr = reader.ReadString();
                            parentId = string.IsNullOrEmpty(parentStr) ? null : Guid.Parse(parentStr);
                            break;
                        case "CHILDREN":
                            children = options.Resolver.GetFormatterWithVerify<List<GameObject>>().Deserialize(ref reader, options);
                            break;
                        case "TRANSFORM":
                            transform = MessagePackSerializer.Deserialize<Transform>(ref reader, options);
                            go.AddComponent(transform);
                            go.transform = transform;
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
                                                reader.Skip();
                                                break;
                                            case "DATA":
                                                var formatter = ComponentRegistry.GetFormatter(compId);
                                                comp = formatter.Deserialize(ref reader, options);
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
                go.Name = name;
                go.children = children ?? new List<GameObject>();
                if (transform != null)
                {
                    transform.gameObject = go;
                }

                go.Components = new List<Component>();
                if (transform != null)
                    go.Components.Add(transform);

                foreach (var (compId, comp) in deserializedComponents)
                {
                    comp.gameObject = go;
                    go.Components.Add(comp);
                }

                return go;
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                GameObject go = new GameObject();
                go.Initialize();
                go.Name = "Error";
                return go;
            }
        }
    }


    public class ComponentFormatter<T> : IMessagePackFormatter<T> where T : Component, new()
    {
        public void Serialize(ref MessagePackWriter writer, T value, MessagePackSerializerOptions options)
        {
            options.Resolver.GetFormatterWithVerify<T>().Serialize(ref writer, value, options);
        }

        public T Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var raw = options.Resolver.GetFormatterWithVerify<T>();
            var temp = raw.Deserialize(ref reader, options);
            return temp;
        }
    }

#pragma warning disable MsgPack009
    public class SceneMeshFormatter : IMessagePackFormatter<Mesh>
    {
        public void Serialize(ref MessagePackWriter writer, Mesh value, MessagePackSerializerOptions options)
        {
            writer.WriteMapHeader(1);
            writer.Write("MESH_GUID");
            writer.Write(value.Id.ToString());
        }

        public Mesh Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            reader.ReadMapHeader();
            reader.Skip();
            Guid meshGuid = Guid.Parse(reader.ReadString());
            Mesh mesh = (Mesh)AssetLoader.LoadSync<Mesh>(meshGuid);
            return mesh;
        }
    }


    public class MeshFormatter : IMessagePackFormatter<Mesh>
    {
        public void Serialize(ref MessagePackWriter writer, Mesh value, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;

            writer.WriteMapHeader(4);

            // GUID
            writer.Write("GUID");
            writer.Write(value.Id.ToString());

            writer.Write("MATERIAL_GUID");
            writer.Write(value.MaterialReference.ToString());

            // Vertices
            writer.Write("VERTICES");
            var vertices = value.GetVertices();
            writer.WriteArrayHeader(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                writer.WriteMapHeader(5);

                writer.Write("P");
                resolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, vertices[i].Position, options);

                writer.Write("N");
                resolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, vertices[i].Normal, options);

                writer.Write("TC");
                resolver.GetFormatterWithVerify<Vector2>().Serialize(ref writer, vertices[i].TexCoord, options);

                writer.Write("T");
                resolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, vertices[i].Tangent, options);

                writer.Write("B");
                resolver.GetFormatterWithVerify<Vector3>().Serialize(ref writer, vertices[i].BiTangent, options);
            }

            // Indices
            writer.Write("INDICES");
            var indices = value.GetIndices();
            writer.WriteArrayHeader(indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                writer.Write(indices[i]);
            }
        }

        public Mesh Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;

            int mapCount = reader.ReadMapHeader();

            Guid guid = Guid.Empty;
            Guid materialGuid = Guid.Empty;
            List<Vertex> vertices = new List<Vertex>();
            List<int> indices = new List<int>();

            for (int i = 0; i < mapCount; i++)
            {
                string key = reader.ReadString();
                switch (key)
                {
                    case "GUID":
                        guid = Guid.Parse(reader.ReadString());
                        break;
                    case "MATERIAL_GUID":
                        materialGuid = Guid.Parse(reader.ReadString());
                        break;
                    case "VERTICES":
                        int vertexCount = reader.ReadArrayHeader();
                        for (int v = 0; v < vertexCount; v++)
                        {
                            reader.ReadMapHeader();

                            Vector3 position = default;
                            Vector3 normal = default;
                            Vector2 texCoord = default;
                            Vector3 tangent = default;
                            Vector3 bitangent = default;

                            for (int p = 0; p < 5; p++)
                            {
                                string propKey = reader.ReadString();
                                switch (propKey)
                                {
                                    case "P":
                                        position = resolver.GetFormatterWithVerify<Vector3>().Deserialize(ref reader, options);
                                        break;
                                    case "N":
                                        normal = resolver.GetFormatterWithVerify<Vector3>().Deserialize(ref reader, options);
                                        break;
                                    case "TC":
                                        texCoord = resolver.GetFormatterWithVerify<Vector2>().Deserialize(ref reader, options);
                                        break;
                                    case "T":
                                        tangent = resolver.GetFormatterWithVerify<Vector3>().Deserialize(ref reader, options);
                                        break;
                                    case "B":
                                        bitangent = resolver.GetFormatterWithVerify<Vector3>().Deserialize(ref reader, options);
                                        break;
                                    default:
                                        reader.Skip();
                                        break;
                                }
                            }

                            vertices.Add(new Vertex(position, normal, texCoord, tangent, bitangent));
                        }
                        break;

                    case "INDICES":
                        int indexCount = reader.ReadArrayHeader();
                        for (int idx = 0; idx < indexCount; idx++)
                        {
                            indices.Add(reader.ReadInt32());
                        }
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            Mesh mesh = new Mesh();
            mesh.Id = guid;
            mesh.MaterialReference = materialGuid;
            mesh.SetVertices(vertices.ToArray());
            if (indices.Count > 0)
            {
                mesh.SetIndices(indices.ToArray());
            }

            return mesh;
        }
    }

#pragma warning restore MsgPack009


    public class TextureFormatter : IMessagePackFormatter<Texture>
    {
        public void Serialize(ref MessagePackWriter writer, Texture value, MessagePackSerializerOptions options)
        {
            writer.WriteMapHeader(4);
            writer.Write("GUID");
            writer.Write(value.Id.ToString());

            writer.Write("WRAP_MODE_S");
            writer.WriteInt32((int)value.WrapS);
            writer.Write("WRAP_MODE_T");
            writer.WriteInt32((int)value.WrapT);
            writer.Write("WRAP_MODE_R");
            writer.WriteInt32((int)value.WrapR);
        }
        public Texture Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            reader.ReadMapHeader();

            reader.Skip();

            string guidStr = reader.ReadString();

            reader.Skip();

            TextureWrapMode wrapS = (TextureWrapMode)reader.ReadUInt32();

            reader.Skip();

            TextureWrapMode wrapT = (TextureWrapMode)reader.ReadUInt32();

            reader.Skip();

            TextureWrapMode wrapR = (TextureWrapMode)reader.ReadUInt32();

            Guid textureGuid = Guid.Parse(guidStr);
            Texture texture = (Texture)AssetLoader.LoadSync<Texture>(textureGuid);
            texture.SetWrapMode(wrapS, wrapT, wrapR);
            return texture;
        }
    }

    public class PBRMaterialFormatter : IMessagePackFormatter<PBRMaterial>
    {
        public void Serialize(ref MessagePackWriter writer, PBRMaterial value, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;

            writer.WriteArrayHeader(14);

            writer.Write(value.Id.ToString());

            MessagePackSerializer.Serialize(ref writer, value.Albedo, options);
            MessagePackSerializer.Serialize(ref writer, value.Emission, options);

            writer.Write(value.EmissionStrength);
            writer.Write(value.Metallic);
            writer.Write(value.Roughness);

            writer.Write(value.isDiffuseSet);
            writer.Write(value.isNormalSet);
            writer.Write(value.isRoughnessSet);
            writer.Write(value.isEmissionSet);

            if (value.isDiffuseSet)
            {
                resolver.GetFormatterWithVerify<Texture>().Serialize(ref writer, value.DiffuseTexture, options);
            }
            if (value.isNormalSet)
            {
                resolver.GetFormatterWithVerify<Texture>().Serialize(ref writer, value.NormalTexture, options);
            }
            if (value.isRoughnessSet)
            {
                resolver.GetFormatterWithVerify<Texture>().Serialize(ref writer, value.RoughnessTexture, options);
            }
            if (value.isEmissionSet)
            {
                resolver.GetFormatterWithVerify<Texture>().Serialize(ref writer, value.EmissionTexture, options);
            }
        }

        public PBRMaterial Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var resolver = options.Resolver;

            var count = reader.ReadArrayHeader();
            if (count != 14)
                throw new InvalidOperationException("Invalid PBRMaterial format");

            var material = new PBRMaterial();
            material.SetDefaults();

            material.Id = Guid.Parse(reader.ReadString());
            material.Albedo = MessagePackSerializer.Deserialize<OpenTK.Mathematics.Color4>(ref reader, options);
            material.Emission = MessagePackSerializer.Deserialize<OpenTK.Mathematics.Color4>(ref reader, options);
            material.EmissionStrength = reader.ReadSingle();
            material.Metallic = reader.ReadSingle();
            material.Roughness = reader.ReadSingle();

            var useDiffuse = reader.ReadBoolean();
            var useNormal = reader.ReadBoolean();
            var useRoughness = reader.ReadBoolean();
            var useEmission = reader.ReadBoolean();

            Console.WriteLine(useDiffuse);

            if (useDiffuse)
            {
                var diffuse = resolver.GetFormatterWithVerify<Texture>().Deserialize(ref reader, options);
                material.DiffuseTexture = diffuse;
            }
            if (useNormal)
            {
                var normal = resolver.GetFormatterWithVerify<Texture>().Deserialize(ref reader, options);
                material.NormalTexture = normal;
            }
            if (useRoughness)
            {
                var roughness = resolver.GetFormatterWithVerify<Texture>().Deserialize(ref reader, options);
                material.RoughnessTexture = roughness;
            }
            if (useEmission)
            {
                var emission = resolver.GetFormatterWithVerify<Texture>().Deserialize(ref reader, options);
                material.EmissionTexture = emission;
            }
            
            material.OnChangeValue();
            return material;
        }

    }


    public class Adapter<T> : IMessagePackFormatter<Component> where T : Component
    {
        private readonly IMessagePackFormatter<T> inner;

        public Adapter(IMessagePackFormatter<T> inner) => this.inner = inner;

        public void Serialize(ref MessagePackWriter writer, Component value, MessagePackSerializerOptions options)
            => inner.Serialize(ref writer, (T)value, options);

        public Component Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
            => inner.Deserialize(ref reader, options);
    }

    public class Color4Formatter : IMessagePackFormatter<Color4>
    {
        public void Serialize(ref MessagePackWriter writer, Color4 value, MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.R);
            writer.Write(value.G);
            writer.Write(value.B);
            writer.Write(value.A);
        }

        public Color4 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var count = reader.ReadArrayHeader();
            if (count != 4)
                throw new InvalidOperationException("Invalid Color4 format");

            float r = reader.ReadSingle();
            float g = reader.ReadSingle();
            float b = reader.ReadSingle();
            float a = reader.ReadSingle();

            return new Color4(r, g, b, a);
        }
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
