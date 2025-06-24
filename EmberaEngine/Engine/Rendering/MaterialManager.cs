using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace EmberaEngine.Engine.Rendering
{
    public static class MaterialManager
    {
        private static readonly Dictionary<uint, Material> _materialsByHandle = new();
        private static readonly Dictionary<Guid, uint> _guidToHandle = new();
        private static uint _nextHandle = 1;

        private static PBRMaterial _nullMaterial;
        private const uint NullHandle = 0;

        public static void Initialize()
        {
            _materialsByHandle.Clear();
            _guidToHandle.Clear();
            _nextHandle = 1;

            _nullMaterial = new PBRMaterial();
            _nullMaterial.DiffuseTexture = Helper.loadImageAsTex("Engine/Content/Textures/Placeholders/null.png");

            _materialsByHandle[NullHandle] = _nullMaterial;
        }

        /// <summary>
        /// Retrieves a material by its runtime handle.
        /// </summary>
        public static Material GetMaterial(uint handle)
        {
            return _materialsByHandle.TryGetValue(handle, out var mat) ? mat : _nullMaterial;
        }

        /// <summary>
        /// Adds a material and returns its runtime handle.
        /// </summary>
        public static uint AddMaterial(Material material)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            if (_guidToHandle.TryGetValue(material.Id, out var existingHandle))
                return existingHandle;

            uint newHandle = _nextHandle++;
            _materialsByHandle[newHandle] = material;
            _guidToHandle[material.Id] = newHandle;
            return newHandle;
        }

        /// <summary>
        /// Gets the runtime handle for a material by asset GUID.
        /// </summary>
        public static uint GetHandle(Guid materialId)
        {
            return _guidToHandle.TryGetValue(materialId, out var handle) ? handle : NullHandle;
        }

        /// <summary>
        /// Removes a material by its handle.
        /// </summary>
        public static bool RemoveMaterial(uint handle)
        {
            if (!_materialsByHandle.TryGetValue(handle, out var mat))
                return false;

            _materialsByHandle.Remove(handle);
            _guidToHandle.Remove(mat.Id);
            return true;
        }

        /// <summary>
        /// Gets the fallback null material handle.
        /// </summary>
        public static uint GetFallbackHandle() => NullHandle;

        /// <summary>
        /// Gets the fallback null material.
        /// </summary>
        public static Material GetFallbackMaterial() => _nullMaterial;
    }
}
