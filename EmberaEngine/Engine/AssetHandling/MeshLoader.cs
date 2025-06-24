using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.AssetHandling
{
    public class MeshLoader : IAssetLoader<Mesh>
    {
        public IEnumerable<string> SupportedExtensions = [
            "png", "jpg", "jpeg", "tga", "exr"
        ];

        IEnumerable<string> IAssetLoader.SupportedExtensions => SupportedExtensions;

        IAssetReference<Mesh> IAssetLoader<Mesh>.Load(string virtualPath)
        {
            return new MeshReference();
        }

    }
}
