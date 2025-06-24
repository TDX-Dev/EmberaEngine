using EmberaEngine.Engine.AssetHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Core
{
    public static class AssetCache
    {
        private static readonly Dictionary<Type, Dictionary<string, object>> _cache = new();

        public static bool TryGet<T>(string virtualPath, out IAssetReference<T> reference) where T : class
        {
            reference = null;

            if (_cache.TryGetValue(typeof(T), out var typeDict))
            {
                if (typeDict.TryGetValue(virtualPath, out var obj) && obj is IAssetReference<T> typedRef)
                {
                    reference = typedRef;
                    return true;
                }
            }

            return false;
        }

        public static void Add<T>(string virtualPath, IAssetReference<T> reference) where T : class
        {
            if (!_cache.TryGetValue(typeof(T), out var typeDict))
            {
                typeDict = new Dictionary<string, object>();
                _cache[typeof(T)] = typeDict;
            }

            typeDict[virtualPath] = reference;
        }

        public static void Remove<T>(string virtualPath) where T : class
        {
            if (_cache.TryGetValue(typeof(T), out var typeDict))
            {
                typeDict.Remove(virtualPath);
                if (typeDict.Count == 0)
                    _cache.Remove(typeof(T));
            }
        }

        public static void ClearAll()
        {
            _cache.Clear();
        }
    }

}
