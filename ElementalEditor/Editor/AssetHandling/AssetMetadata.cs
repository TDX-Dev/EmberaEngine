using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.AssetHandling
{
    public class AssetMetadataDatabase
    {

        static string filePath;

        public static void CreateDatabase(string projectRoot)
        {
            filePath = Path.Combine(projectRoot, Project.PROJECT_REGISTRY_FILE_NAME);

            if (File.Exists(filePath))
            {
                byte[] data = File.ReadAllBytes(filePath);
                var metadataFile = MessagePackSerializer.Deserialize<AssetMetadataFile>(data);

                AssetLookup.guidToPath = metadataFile.Metadata ?? new Dictionary<Guid, string>();
                AssetLookup.pathToGuid = AssetLookup.guidToPath.ToDictionary(pair => pair.Value, pair => pair.Key);

                var keysToRemove = AssetLookup.pathToGuid
                    .Where(kvp => !File.Exists(kvp.Key))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    var guid = AssetLookup.pathToGuid[key];
                    AssetLookup.pathToGuid.Remove(key);
                    AssetLookup.guidToPath.Remove(guid);
                }
            }

            SaveFile(); // Persist to ensure registry is always valid
        }

        public static void SaveFile()
        {
            var metadataFile = new AssetMetadataFile
            {
                LastWrite = DateTime.Now,
                Metadata = AssetLookup.guidToPath
            };

            byte[] bytes = MessagePackSerializer.Serialize(metadataFile);
            File.WriteAllBytes(filePath, bytes);
        }
    }
}