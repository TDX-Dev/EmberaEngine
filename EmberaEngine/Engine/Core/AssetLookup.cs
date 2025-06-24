using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MessagePack;

namespace EmberaEngine.Core
{
    [MessagePackObject]
    public class AssetMetadataFile
    {
        [Key(0)]
        public DateTime LastWrite { get; set; }

        [Key(1)]
        public Dictionary<Guid, string> Metadata { get; set; } = new();
    }



    [MessagePackObject]
    public class AssetLookup
    {
        public static Dictionary<Guid, string> guidToPath = new();
        public static Dictionary<string, Guid> pathToGuid = new();

        public static string GetFilePathByGuid(Guid guid)
        {
            return guidToPath.TryGetValue(guid, out var value) ? value : "";
        }

        public static void RegisterFile(Guid guid, string path)
        {
            if (pathToGuid.TryGetValue(path, out var existingGuid))
            {
                pathToGuid[path] = existingGuid;
                guidToPath[existingGuid] = path;
            }
            else
            {
                pathToGuid[path] = guid;
                guidToPath[guid] = path;
            }
        }

        public static void AssetChange(string oldPath, string newPath)
        {
            if (pathToGuid.TryGetValue(oldPath, out var guid))
            {
                pathToGuid.Remove(oldPath);
                pathToGuid[newPath] = guid;
                guidToPath[guid] = newPath;
            }
        }

        //public static void CreateDatabase(string projectRoot)
        //{
        //    filePath = Path.Combine(projectRoot, Project.PROJECT_REGISTRY_FILE_NAME);

        //    if (File.Exists(filePath))
        //    {
        //        byte[] data = File.ReadAllBytes(filePath);
        //        var metadataFile = MessagePackSerializer.Deserialize<AssetMetadataFile>(data);

        //        guidToPath = metadataFile.Metadata ?? new Dictionary<Guid, string>();
        //        pathToGuid = guidToPath.ToDictionary(pair => pair.Value, pair => pair.Key);

        //        var keysToRemove = pathToGuid
        //            .Where(kvp => !File.Exists(kvp.Key))
        //            .Select(kvp => kvp.Key)
        //            .ToList();

        //        foreach (var key in keysToRemove)
        //        {
        //            var guid = pathToGuid[key];
        //            pathToGuid.Remove(key);
        //            guidToPath.Remove(guid);
        //        }
        //    }

        //    SaveFile(); // Persist to ensure registry is always valid
        //}

        //public static void SaveFile()
        //{
        //    var metadataFile = new AssetMetadataFile
        //    {
        //        LastWrite = DateTime.Now,
        //        Metadata = guidToPath
        //    };

        //    byte[] bytes = MessagePackSerializer.Serialize(metadataFile);
        //    File.WriteAllBytes(filePath, bytes);
        //}
    }
}
