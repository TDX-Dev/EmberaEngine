using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.AssetHandling
{
    public static class AssetType
    {
        public static string SCENE_FILE = "SCENE";
        public static string MATERIAL_FILE = "MATERIAL";
        public static string MODEL_FILE = "MODEL";
        public static string MESH_FILE = "MESH";
        public static string TEXTURE_FILE = "TEXTURE";
        public static string UNKNOWN_FILE = "UNKNOWN";

        static Dictionary<string, string> knownFileTypes = new Dictionary<string, string>();

        static AssetType()
        {
            knownFileTypes["dscn"] = SCENE_FILE;
            knownFileTypes["dmat"] = MATERIAL_FILE;
            knownFileTypes["dmsh"] = MESH_FILE;


            knownFileTypes["tga"] = TEXTURE_FILE;
            knownFileTypes["png"] = TEXTURE_FILE;
            knownFileTypes["exr"] = TEXTURE_FILE;
            knownFileTypes["jpg"] = TEXTURE_FILE;
            knownFileTypes["jpeg"] = TEXTURE_FILE;

            knownFileTypes["fbx"] = MODEL_FILE;
            knownFileTypes["obj"] = MODEL_FILE;
            knownFileTypes["gltf"] = MODEL_FILE;
        }


        public static string ResolveAssetType(string type)
        {
            if (knownFileTypes.TryGetValue(type, out var result))
            {
                return result;
            }

            return UNKNOWN_FILE;
        }

    }
}
