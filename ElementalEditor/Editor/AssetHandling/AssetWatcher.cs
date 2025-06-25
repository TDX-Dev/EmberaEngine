using EmberaEngine.Core;
using EmberaEngine.Engine.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.AssetHandling
{
    class AssetWatcher
    {
        private static FileSystemWatcher watcher;
        private static string watcherRootDirectory;
        private static readonly ConcurrentDictionary<string, DateTime> debounceMap = new();
        private static readonly TimeSpan debounceTime = TimeSpan.FromMilliseconds(5000);

        public static void SetupWatcher(string path)
        {
            watcherRootDirectory = path;

            watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
                Filter = "*.*",
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Renamed += OnRenamed;
        }

        private static void OnChanged(object sender, FileSystemEventArgs e) => Debounce(e.FullPath);

        static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Debounce(e.OldFullPath, e.FullPath);
        }


        static void Debounce(string path)
        {
            if (Directory.Exists(path))
                return;

            Task.Run(async () =>
            {
                try
                {
                    await WaitUntilFileIsReady(path);

                    //Console.WriteLine($"Hot reloading asset at: {path}");

                    AssetLookup.AssetChange(path, path); // Same path — no rename
                    AssetMetadataDatabase.SaveFile();

                    MainThreadDispatcher.Queue(() =>
                    {
                        string relativePath = Path.GetRelativePath(watcherRootDirectory, path);
                        AssetReferenceRegistry.Reload(relativePath);
                    });
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[Watcher] Error on change: {ex.Message}");
                }
            });
        }

        static void Debounce(string oldPath, string newPath)
        {
            if (Directory.Exists(newPath))
                return;

            Task.Run(async () =>
            {
                try
                {
                    await WaitUntilFileIsReady(newPath);

                    //Console.WriteLine($"Asset moved or renamed: {oldPath} -> {newPath}");

                    AssetLookup.AssetChange(oldPath, newPath);
                    
                    lock (AssetMetadataDatabase._assetMetadataLock) {
                        AssetMetadataDatabase.SaveFile();
                    }

                    MainThreadDispatcher.Queue(() =>
                    {
                        string relativePath = Path.GetRelativePath(watcherRootDirectory, newPath);
                        AssetReferenceRegistry.Reload(relativePath);
                    });
                }
                catch (Exception ex)
                {
                    //Console.WriteLine($"[Watcher] Error on rename: {ex.Message}");
                }
            });
        }


        private static async Task WaitUntilFileIsReady(string path, int timeoutMs = 5000)
        {
            var sw = Stopwatch.StartNew();
            while (!IsFileReady(path))
            {
                if (sw.ElapsedMilliseconds > timeoutMs)
                    throw new TimeoutException($"File not ready after {timeoutMs}ms: {path}");

                await Task.Delay(100);
            }
        }

        private static bool IsFileReady(string path)
        {
            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }
    }

}
