using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Serializing;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.AssetHandling
{
    public class MaterialLoader : IAssetLoader<Material>
    {
        public IEnumerable<string> SupportedExtensions => new[] { "dmat" };

        public Material LoadSync(string virtualPath)
        {
            var resolver = CompositeResolver.Create(
                new IMessagePackFormatter[]
                {
                new PBRMaterialFormatter(),
                new TextureFormatter(),
                new Color4Formatter()
                },
                new IFormatterResolver[]
                {
                StandardResolver.Instance
                }
            );

            var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

            var binary = VirtualFileSystem.Open(virtualPath);

            PBRMaterial material = MessagePackSerializer.Deserialize<PBRMaterial>(binary, options);
            material.Id = AssetLookup.GetFileGuidByPath(virtualPath);

            return material;
        }

        public IAssetReference<Material> Load(string virtualPath)
        {
            var materialReference = new MaterialReference();

            Task.Run(() =>
            {
                var resolver = CompositeResolver.Create(
                    new IMessagePackFormatter[]
                    {
                    new PBRMaterialFormatter(),
                    new TextureFormatter(),
                    new Color4Formatter()
                    },
                    new IFormatterResolver[]
                    {
                    StandardResolver.Instance
                    }
                );

                var options = MessagePackSerializerOptions.Standard.WithResolver(resolver);

                var binary = VirtualFileSystem.Open(virtualPath);

                MainThreadDispatcher.Queue(() =>
                {
                    PBRMaterial material = MessagePackSerializer.Deserialize<PBRMaterial>(binary, options);
                    material.Id = AssetLookup.GetFileGuidByPath(virtualPath);
                    materialReference.SetValue(material);
                });
            });

            return materialReference;
        }
    }

}
