using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ElementalEditor.Editor.AssetHandling
{
    public class AssetMetadataDatabase
    {
        static string filePath;

        public static void CreateDatabase(string projectRoot)
        {
            filePath = Path.Combine(projectRoot, Project.PROJECT_ENGINE_DIRECTORY_NAME, Project.PROJECT_METADATA_DIRECTORY,  Project.PROJECT_REGISTRY_FILE_NAME);

            if (File.Exists(filePath))
            {
                byte[] data = File.ReadAllBytes(filePath);
                var metadataFile = MessagePackSerializer.Deserialize<AssetMetadataFile>(data);

                AssetLookup.guidToPath = metadataFile.Metadata ?? new Dictionary<Guid, string>();
                AssetLookup.pathToGuid = AssetLookup.guidToPath.ToDictionary(pair => pair.Value, pair => pair.Key);
            }
            else
            {
                AssetLookup.guidToPath = new Dictionary<Guid, string>();
                AssetLookup.pathToGuid = new Dictionary<string, Guid>();
            }
            string assetsPath = Path.Combine(projectRoot, Project.PROJECT_GAME_FILES_DIRECTORY);
            if (Directory.Exists(assetsPath))
            {
                var allFiles = Directory.GetFiles(assetsPath, "*.*", SearchOption.AllDirectories);

                foreach (string path in allFiles)
                {
                    string normalizedPath = Path.GetRelativePath(Path.Combine(projectRoot, Project.PROJECT_GAME_FILES_DIRECTORY),Path.GetFullPath(path));
                    if (!AssetLookup.pathToGuid.ContainsKey(normalizedPath))
                    {
                        Guid newGuid = Guid.NewGuid();
                        AssetLookup.pathToGuid[normalizedPath] = newGuid;
                        AssetLookup.guidToPath[newGuid] = normalizedPath;
                        Console.Write(newGuid);
                    }
                }
                var missingFiles = AssetLookup.pathToGuid
                    .Where(kvp => !File.Exists(VirtualFileSystem.ResolvePath(kvp.Key)))
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var missingPath in missingFiles)
                {
                    var guid = AssetLookup.pathToGuid[missingPath];
                    AssetLookup.pathToGuid.Remove(missingPath);
                    AssetLookup.guidToPath.Remove(guid);
                }
            }

            SaveFile();
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
