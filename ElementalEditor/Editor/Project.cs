using ElementalEditor.Editor.AssetHandling;
using EmberaEngine.Engine.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using YamlDotNet;
using YamlDotNet.Serialization;

namespace ElementalEditor.Editor
{
    struct ProjectFile
    {
        public string ProjectName;
        public string ProjectAuthor;
        public string EngineVersion;
    }

    public class Project
    {
        public static string PROJECT_DEFAULT_FILE_NAME = "project";
        public static string PROJECT_DEFAULT_FILE_EXTENSION = "dproj";
        public static string PROJECT_ENGINE_DIRECTORY_NAME = ".devoid";
        public static string PROJECT_REGISTRY_FILE_NAME = "assetRegistry";
        public static string PROJECT_ASSET_THUMBNAIL_DIRECTORY = "Asset Thumbnails";
        public static string PROJECT_METADATA_DIRECTORY = "Metadata";
        public static string PROJECT_GAME_FILES_DIRECTORY = "GameFiles";

        public static FileSystemWatcher ProjectDirectoryWatcher;

        public static void SetupProject(string projectPath)
        {
            string file = Path.Combine(projectPath, PROJECT_DEFAULT_FILE_NAME + "." +  PROJECT_DEFAULT_FILE_EXTENSION);
            string engineDirectory = Path.Combine(projectPath, PROJECT_ENGINE_DIRECTORY_NAME);
            string thumbnailDirectory = Path.Combine(engineDirectory, PROJECT_ASSET_THUMBNAIL_DIRECTORY);
            string metadataDirectory = Path.Combine(engineDirectory, PROJECT_METADATA_DIRECTORY);
            string gameFilesDirectory = Path.Combine(projectPath, PROJECT_GAME_FILES_DIRECTORY);

            if (!File.Exists(file)) { throw new Exception("Project file not found."); }

            string fileSource;
            using (var reader = new StreamReader(file))
            {
                fileSource = reader.ReadToEnd();
            }
            Deserializer deserializer = new Deserializer();



            ProjectFile project = deserializer.Deserialize<ProjectFile>(fileSource);

            CreateDirectoryIfNotExist(engineDirectory);
            CreateDirectoryIfNotExist(thumbnailDirectory);
            bool metadataCreated = CreateDirectoryIfNotExist(metadataDirectory);
            CreateDirectoryIfNotExist(gameFilesDirectory);

            VirtualFileSystem.Mount(new DirectoryAssetSource(gameFilesDirectory));

            AssetMetadataDatabase.CreateDatabase(projectPath);

            AssetWatcher.SetupWatcher(gameFilesDirectory);

            //LoadProjectGameFiles(projectPath, metadataCreated);

        }

        public static bool CreateDirectoryIfNotExist(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            return false;
        }

        public static void CreateEngineDirectory(string basePath)
        {
            string fullPath = Path.Combine(basePath, PROJECT_ENGINE_DIRECTORY_NAME);
            Directory.CreateDirectory(fullPath);
        }

        public static void CreateAssetMetadataStorage()
        {

        }

    }


    public class DirectoryAssetSource : IAssetSource
    {
        private readonly string _rootPath;

        public DirectoryAssetSource(string rootPath)
        {
            _rootPath = rootPath;
        }

        public bool Exists(string virtualPath)
            => File.Exists(Path.Combine(_rootPath, virtualPath));

        public byte[] Open(string virtualPath)
            => File.ReadAllBytes(Path.Combine(_rootPath, virtualPath));

        public Stream OpenStream(string virtualPath)
            => File.OpenRead(Path.Combine(_rootPath, virtualPath));

        public string ResolvePath(string virtualPath) => Path.Combine(_rootPath, virtualPath);

        public IEnumerable<string> EnumerateCurrentLevel(string path) => Directory.EnumerateFiles(Path.Combine(_rootPath, path), "*")
            .Select(p => Path.GetRelativePath(_rootPath, p).Replace("\\", "/"));

        public IEnumerable<string> EnumerateFiles(string path)
            => Directory.EnumerateFiles(Path.Combine(_rootPath, path), "*", SearchOption.AllDirectories)
                        .Select(p => Path.GetRelativePath(_rootPath, p).Replace("\\", "/"));
    }
}
