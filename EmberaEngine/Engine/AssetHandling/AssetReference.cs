using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.AssetHandling
{

    public interface IAssetReference<T> where T : class
    {
        public bool isLoaded { get; }
        public T value { get; }
        public event Action<T> OnLoad;

        public void SetValue(T value);
        public void Unload();
    
    }

    public class TextureReference : IAssetReference<Texture>
    {
        public bool isLoaded => _loaded;

        public Texture value => _value;

        bool _loaded;
        Texture _value;

        public event Action<Texture> OnLoad = (value) => { };

        public void SetValue(Texture value)
        {
            _value = value;
            _loaded = true;

            OnLoad.Invoke(_value);
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }

    public class MeshReference : IAssetReference<Mesh>
    {
        public bool isLoaded => _loaded;
        public Mesh value => _value;

        bool _loaded;
        Mesh _value;

        public event Action <Mesh> OnLoad = (value) => { };

        public void SetValue(Mesh value)
        {
            _value= value;
            _loaded = true;
            OnLoad.Invoke(_value);
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }

    public class MaterialReference : IAssetReference<Material>
    {
        public bool isLoaded => _loaded;
        public Material value => _value;

        bool _loaded;
        Material _value;

        public event Action<Material> OnLoad = (value) => { };

        public void SetValue(Material value)
        {
            _value = value;
            _loaded = true;
            OnLoad.Invoke(_value);
        }

        public void Unload()
        {
            throw new NotImplementedException();
        }
    }
}
