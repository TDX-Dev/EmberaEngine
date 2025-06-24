using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.AssetHandling
{
    public interface IAssetLoader
    {
        IEnumerable<string> SupportedExtensions { get; } // [".png", ".dds"]
    }

    public interface IAssetLoader<T> : IAssetLoader where T : class
    {
        public IAssetReference<T> Load(string virtualPath);
        public T LoadSync(string virtualPath);
    }


    public static class AssetLoader
    {
        private static readonly Dictionary<Type, IAssetLoader> _loaders = new();


        static AssetLoader()
        {
            Register(new TextureLoader());
            Register(new MeshLoader());
            Register(new MaterialLoader());
        }

        public static void Register<T>(IAssetLoader<T> loader) where T : class
        {
            _loaders[typeof(T)] = loader;
        }

        public static IAssetReference<T> Load<T>(string virtualPath) where T : class
        {
            if (AssetCache.TryGet<T>(virtualPath, out IAssetReference<T> reference))
                return reference;

            if (!_loaders.TryGetValue(typeof(T), out var loaderObj))
                throw new Exception($"No loader registered for type {typeof(T)}");

            var loader = (IAssetLoader<T>)loaderObj;
            var newRef = loader.Load(virtualPath);

            AssetCache.Add(virtualPath, newRef);
            AssetReferenceRegistry.Register(PathUtils.NormalizeVirtualPath(virtualPath), newRef);

            return newRef;
        }

        public static IAssetReference<T> Load<T>(Guid guid) where T : class
        {
            string virtualPath = AssetLookup.GetFilePathByGuid(guid);

            return Load<T>(virtualPath);
        }

        public static T LoadSync<T>(string virtualPath) where T : class
        {
            if (AssetCache.TryGet<T>(virtualPath, out T reference))
                return reference;

            if (!_loaders.TryGetValue(typeof(T), out var loaderObj))
                throw new Exception($"No loader registered for type {typeof(T)}");

            var loader = (IAssetLoader<T>)loaderObj;
            T newRef = (T)loader.LoadSync(virtualPath);

            AssetCache.Add(virtualPath, newRef);

            return newRef;
        }

        public static T LoadSync<T>(Guid guid) where T : class
        {
            string virtualPath = AssetLookup.GetFilePathByGuid(guid);

            return LoadSync<T>(virtualPath);
        }

        public static IAssetLoader<T> GetLoader<T>() where T : class
        {
            if (!_loaders.TryGetValue(typeof(T), out var loaderObj))
                throw new Exception($"No loader registered for type {typeof(T).FullName}");

            return (IAssetLoader<T>)loaderObj;
        }

        public static Type? GuessAssetType(string assetPath)
        {
            string ext = Path.GetExtension(assetPath).ToLowerInvariant();

            foreach (var kv in _loaders)
            {
                if (kv.Value.SupportedExtensions.Contains(ext))
                    return kv.Key;
            }

            return null;
        }
    }
}
