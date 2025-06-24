using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Serializing;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    class DiskUtilities
    {
        public static void SaveMesh(string path, Mesh mesh)
        {
            var resolver = CompositeResolver.Create(
                new IMessagePackFormatter[]
                {
                    new MeshFormatter(),
                    new EmberaEngine.Engine.Serializing.Vector2Formatter(),
                    new EmberaEngine.Engine.Serializing.Vector3Formatter(),
                    new EmberaEngine.Engine.Serializing.Vector4Formatter()
                },
                new IFormatterResolver[] { StandardResolver.Instance }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            var binary = MessagePackSerializer.Serialize(mesh, options);


            using (FileStream fs = File.Create(path))
            {
                fs.Write(binary);
            }
        }

        public static Mesh LoadMesh(string path)
        {
            var resolver = CompositeResolver.Create(
                new IMessagePackFormatter[]
                {
                    new MeshFormatter(),
                    new EmberaEngine.Engine.Serializing.Vector2Formatter(),
                    new EmberaEngine.Engine.Serializing.Vector3Formatter(),
                    new EmberaEngine.Engine.Serializing.Vector4Formatter()
                },
                new IFormatterResolver[] { StandardResolver.Instance }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            var binary = File.ReadAllBytes(path);

            return MessagePackSerializer.Deserialize<Mesh>(binary, options);
        }


        public static void SaveMaterial()
        {

        }
    }
}
