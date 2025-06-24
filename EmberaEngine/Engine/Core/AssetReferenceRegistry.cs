using EmberaEngine.Engine.AssetHandling;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public static class AssetReferenceRegistry
    {
        private static Dictionary<string, Action> reloaders = new();

        public static void Register<T>(string path, IAssetReference<T> reference) where T : class
        {
            path = PathUtils.NormalizeVirtualPath(path);

            // Create the reload action once and store it
            reloaders[path] = () =>
            {
                var loader = AssetLoader.GetLoader<T>();
                var newReference = loader.Load(path);

                newReference.OnLoad += (T value) => { reference.SetValue(value); };
            };
        }

        public static void Reload(string path)
        {
            path = PathUtils.NormalizeVirtualPath(path);

            if (reloaders.TryGetValue(path, out var reloadAction))
            {
                reloadAction();
            }
        }
    }

}
