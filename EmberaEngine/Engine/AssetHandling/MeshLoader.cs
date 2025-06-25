using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Serializing;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.AssetHandling
{
    public class MeshLoader : IAssetLoader<Mesh>
    {
        public IEnumerable<string> SupportedExtensions = [
            "dmsh"
        ];

        IEnumerable<string> IAssetLoader.SupportedExtensions => SupportedExtensions;

        public Mesh LoadSync(string virtualPath)
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

            var binary = VirtualFileSystem.Open(virtualPath);
            Mesh mesh = MessagePackSerializer.Deserialize<Mesh>(binary, options);

            // Load the material synchronously
            try
            {
                var material = (PBRMaterial)AssetLoader.LoadSync<Material>(mesh.MaterialReference);
                mesh.MaterialRenderHandle = MaterialManager.AddMaterial(material);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load material");
                mesh.MaterialRenderHandle = MaterialManager.GetFallbackHandle();
            }

            mesh.Id = AssetLookup.GetFileGuidByPath(virtualPath);
            return mesh;
        }


        IAssetReference<Mesh> IAssetLoader<Mesh>.Load(string virtualPath)
        {
            var meshReference = new MeshReference();

            Task.Run(() =>
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

                var binary = VirtualFileSystem.Open(virtualPath);

                MainThreadDispatcher.Queue(() =>
                {
                    Mesh mesh = MessagePackSerializer.Deserialize<Mesh>(binary, options);

                    Console.WriteLine($"Material: {mesh.MaterialReference}");
                    foreach (var kv in AssetLookup.guidToPath)
                    {
                        Console.WriteLine($"{kv.Key} : {kv.Value}");
                    }

                    PBRMaterial material;
                    
                    try
                    {
                        material = (PBRMaterial)AssetLoader.LoadSync<Material>(mesh.MaterialReference);
                        mesh.MaterialRenderHandle = MaterialManager.AddMaterial(material);
                    } catch (Exception ex)
                    {
                        Console.WriteLine("Failed to load material");
                        mesh.MaterialRenderHandle = MaterialManager.GetFallbackHandle();
                    }

                    mesh.Id = AssetLookup.GetFileGuidByPath(virtualPath);
                    meshReference.SetValue(mesh);
                });


            });

            return meshReference;
        }

    }
}
