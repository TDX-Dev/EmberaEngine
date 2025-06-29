using ElementalEditor.Editor.AssetHandling;
using ElementalEditor.Editor.Utils;
using EmberaEngine.Engine.AssetHandling;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Utilities;
using ImGuiNET;
using MaterialIconFont;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    public class ProjectAssetPanelType
    {
        public string file;
        public string name;
        public string type;
    }

    public class ProjectAssetPanel : Panel
    {

        Texture materialTexture;
        Texture checkerTexture;
        Texture unknownFileTexture;

        int assetCardWidth = 165;
        int assetCardHeight = 220;
        int assetCardPadding = 5;
        int assetCardThumbnailSize = 120;

        int directoryButtonHeight = 40;

        int folderTabWidth = 300;
        int directoryTabHeight = 55;
        int assetTilePadding = 10;

        string rootPath;
        string currentPath;

        List<ProjectAssetPanelType> currentPathAssets;
        List<string> directoryContents;
        Dictionary<string, TextureReference> textureAssetCache;

        ProjectAssetPanelType currentSelectedFile;

        private string searchText = "";
        private string pathInput = "";

        private Stack<string> backHistory = new();
        private Stack<string> forwardHistory = new();

        private void NavigateTo(string newPath)
        {
            if (newPath == currentPath) return;

            if (Directory.Exists(newPath))
            {
                backHistory.Push(currentPath);
                forwardHistory.Clear();
                currentPath = newPath;
                UpdatePaths();
            }
        }

        private void NavigateBack()
        {
            if (backHistory.Count > 0)
            {
                forwardHistory.Push(currentPath);
                currentPath = backHistory.Pop();
                UpdatePaths();
            }
        }

        private void NavigateForward()
        {
            if (forwardHistory.Count > 0)
            {
                backHistory.Push(currentPath);
                currentPath = forwardHistory.Pop();
                UpdatePaths();
            }
        }

        private void NavigateUp()
        {
            var parent = Path.GetFullPath(Path.Combine(currentPath, ".."));
            if (Directory.Exists(parent) && parent != currentPath)
            {
                NavigateTo(parent);
            }
        }

        private void CreateNewFolder()
        {
            string newFolderPath = Path.Combine(currentPath, "New Folder");
            int i = 1;
            while (Directory.Exists(newFolderPath))
            {
                newFolderPath = Path.Combine(currentPath, $"New Folder {i++}");
            }

            Directory.CreateDirectory(newFolderPath);
            UpdatePaths();
        }

        private void OpenInFileExplorer()
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", currentPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to open folder: " + ex.Message);
            }
        }



        public void UpdatePaths()
        {
            Console.WriteLine("Updating Path");
            currentPathAssets ??= new();
            directoryContents ??= new();
            textureAssetCache ??= new();

            currentPathAssets.Clear();
            directoryContents.Clear();
            textureAssetCache.Clear();

            foreach (string file in VirtualFileSystem.EnumerateCurrentLevel(currentPath))
            {
                currentPathAssets.Add(new ProjectAssetPanelType()
                {
                    file = file,
                    name = Path.GetFileName(file),
                    type = AssetType.ResolveAssetType(Path.GetExtension(file).Replace(".", ""))
                });
            }

            foreach (string dir in Directory.GetDirectories(currentPath))
            {
                directoryContents.Add(Path.GetRelativePath(currentPath, dir));
            }

            pathInput = Path.GetRelativePath(editor.projectPath, currentPath);
        }

        public override void OnAttach()
        {
            currentPathAssets = new List<ProjectAssetPanelType>();
            materialTexture = Helper.loadImageAsTex("Editor/Assets/Textures/FileTypeTextures/material.png");
            checkerTexture = Helper.loadImageAsTex("Editor/Assets/Textures/FileTypeTextures/assetCheckerBG.png");
            unknownFileTexture = Helper.loadImageAsTex("Editor/Assets/Textures/FileTypeTextures/unkfile.png");

            currentPath = Path.GetFullPath(Path.Combine(editor.projectPath, Project.PROJECT_GAME_FILES_DIRECTORY));
            rootPath = currentPath;

            textureAssetCache = new Dictionary<string, TextureReference>();
            directoryContents = new List<string>();

            pathInput = Path.GetRelativePath(editor.projectPath, currentPath);

            UpdatePaths();
        }

        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            ImGui.PushStyleColor(ImGuiCol.ChildBg, Vector4.Zero);

            if (ImGui.Begin("Project Assets"))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));

                Vector2 cursorPos = ImGui.GetCursorPos();

                // Folder Tab
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.247f, 0.247f, 0.247f, 1));
                ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                if (ImGui.BeginChild("foldersTab", new Vector2(folderTabWidth, -1), ImGuiChildFlags.AlwaysUseWindowPadding))
                {
                    if (currentPath != rootPath && DirectoryButton(".."))
                    {
                        NavigateTo(Path.GetFullPath(Path.Combine(currentPath, "..")));
                    }

                    for (int i = 0; i < directoryContents.Count; i++)
                    {
                        string dir = directoryContents[i];
                        if (DirectoryButton(dir))
                        {
                            NavigateTo(Path.GetFullPath(Path.Combine(currentPath, dir)));
                        }
                    }
                }
                ImGui.EndChild();
                ImGui.PopStyleColor(2);

                ImGui.PopStyleVar();
                ImGui.SetCursorPos(new Vector2(cursorPos.X + folderTabWidth, cursorPos.Y));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(20, 10));
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.118f, 0.118f, 0.118f, 1f));

                // Directory Header
                if (ImGui.BeginChild("directory_ind", new Vector2(-1, directoryTabHeight), ImGuiChildFlags.AlwaysUseWindowPadding))
                {
                    var btnGroup = new ButtonGroup("DirectoryControls", ButtonGroup.RenderMode.CustomDraw);
                    btnGroup.Add(MaterialDesign.Arrow_back, NavigateBack);
                    btnGroup.Add(MaterialDesign.Arrow_forward, NavigateForward);
                    btnGroup.Add(MaterialDesign.Arrow_upward, NavigateUp);
                    btnGroup.Add(MaterialDesign.Create_new_folder, CreateNewFolder);
                    btnGroup.Render();

                    ImGui.SameLine();
                    if (ImGui.Button(MaterialDesign.Folder_special))
                    {
                        OpenInFileExplorer();
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(700);
                    if (EditorUI.DrawTextInput("##directory_path_input", ref pathInput) && Directory.Exists(Path.Combine(editor.projectPath, pathInput)))
                    {
                        string newPath = Path.GetFullPath(Path.Combine(editor.projectPath, pathInput));
                        NavigateTo(newPath);
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(400);
                    EditorUI.DrawTextInput("##directory_search_input", ref searchText);
                }
                ImGui.EndChild();
                ImGui.PopStyleColor();
                ImGui.PopStyleVar();

                // Grid Region
                ImGui.SetCursorPos(new Vector2(cursorPos.X + folderTabWidth + assetTilePadding, cursorPos.Y + directoryTabHeight + assetTilePadding));
                Vector2 gridAreaSize = new Vector2(-1, ImGui.GetContentRegionAvail().Y);

                if (ImGui.BeginChild("assetGrid", gridAreaSize, ImGuiChildFlags.AlwaysUseWindowPadding, ImGuiWindowFlags.HorizontalScrollbar))
                {
                    DrawAssetGrid(currentPathAssets);
                }
                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleVar(2);
            ImGui.PopStyleColor();
        }


        public bool DirectoryButton(string name)
        {
            // Align text to the left vertically centered
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.0f, 0.5f));

            // Get available width and use that as button width
            float width = ImGui.GetContentRegionAvail().X;
            bool clicked = ImGui.Button(MaterialDesign.Folder + " " + name, new Vector2(width, directoryButtonHeight));

            ImGui.PopStyleVar();

            return clicked;
        }

        public void DrawAssetGrid(List<ProjectAssetPanelType> assets)
        {
            float contentWidth = ImGui.GetContentRegionAvail().X;
            float itemWidth = assetCardWidth;
            float itemSpacing = ImGui.GetStyle().ItemSpacing.X;

            int columnCount = Math.Max(1, (int)((contentWidth + itemSpacing) / (itemWidth + itemSpacing)));

            int i = 0;
            foreach (var asset in assets)
            {
                DrawAsset(asset);

                i++;
                if (i % columnCount != 0)
                {
                    ImGui.SameLine();
                }
            }
        }



        public void DrawAsset(ProjectAssetPanelType asset)
        {
            if (asset.type == AssetType.TEXTURE_FILE)
            {
                if (!textureAssetCache.TryGetValue(asset.file, out TextureReference value))
                {
                    value = (TextureReference)AssetLoader.Load<Texture>(asset.file);
                    textureAssetCache.Add(asset.file, value);
                }

                if (value.isLoaded)
                {
                    //Console.WriteLine("Loaded");
                    DrawAssetTile(value.value, asset);
                } else
                {
                    //Console.WriteLine("Not loaded");
                    DrawAssetTile(materialTexture, asset);
                }


            } else
            {
                DrawAssetTile(unknownFileTexture, asset);
            }
        }

        public void DrawAssetTile(Texture thumbnailTexture, ProjectAssetPanelType assetInfo)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5f);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 2);

            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.3f, 0.3f, 0.3f, 1f));

            bool isHovered = false;
            bool isActive = false;


            if (ImGui.BeginChild(assetInfo.name, new Vector2(assetCardWidth, assetCardHeight), ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.Borders))
            {
                //ImGui.InvisibleButton(fileName + "_dragRegion", ImGui.GetContentRegionAvail());
                isHovered = ImGui.IsItemHovered();
                isActive = ImGui.IsItemActive();


                float windowWidth = ImGui.GetContentRegionAvail().X;
                float windowHeight = ImGui.GetContentRegionAvail().Y;

                Vector2 checkerPatternSize = new Vector2(windowWidth, assetCardThumbnailSize + (windowWidth - assetCardThumbnailSize) / 2);

                ImGui.Image(checkerTexture.GetRendererID(), checkerPatternSize, new Vector2(0, 0), new Vector2(1, 0.8f));

                ImGui.SetCursorPosX((windowWidth - assetCardThumbnailSize) / 2);
                ImGui.SetCursorPosY((windowWidth - assetCardThumbnailSize) / 2);


                ImGui.Image(thumbnailTexture.GetRendererID(), new Vector2(assetCardThumbnailSize, assetCardThumbnailSize));

                if (currentSelectedFile == assetInfo)
                {
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.1f, 0.1f, 0.3f, 1f));
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.3f, 0.3f, 0.3f, 1f));
                }
                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));
                //ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.1f, 0.1f, 0.1f, 1f));

                if (ImGui.BeginChild(assetInfo.name + "desc", Vector2.One * -1, ImGuiChildFlags.AlwaysUseWindowPadding))
                {
                    ImGui.Text(assetInfo.name);
                }
                ImGui.EndChild();

                ImGui.PopStyleColor();
                ImGui.PopStyleVar(2);


                // Place this right before checking for drag
                ImGui.SetCursorPos(Vector2.Zero);
                if (ImGui.InvisibleButton(assetInfo.name + "_dragRegion", new Vector2(assetCardWidth, assetCardHeight)))
                {
                    currentSelectedFile = assetInfo;
                }

                isHovered = ImGui.IsItemHovered();
                isActive = ImGui.IsItemActive();

                if (isActive && ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceNoHoldToOpenOthers))
                {
                    string payload = assetInfo.file;
                    IntPtr payloadPtr = Marshal.StringToHGlobalAnsi(payload);
                    ImGui.SetDragDropPayload("ASSET_DRAG", payloadPtr, (uint)payload.Length + 1);

                    // Custom drag preview (without using BeginChild!)
                    ImGui.BeginGroup();

                    ImGui.Image(thumbnailTexture.GetRendererID(), new Vector2(40, 40)); // Small preview
                    ImGui.SameLine();
                    ImGui.BeginGroup();
                    ImGui.Text(assetInfo.name);
                    ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1f), assetInfo.type);
                    ImGui.EndGroup();

                    ImGui.EndGroup();

                    ImGui.EndDragDropSource();
                    Marshal.FreeHGlobal(payloadPtr);
                }


                // Draw "Heya" as overlay text
                var drawList = ImGui.GetWindowDrawList();

                // Absolute screen position of the window
                Vector2 windowPos = ImGui.GetWindowPos();

                // Offset within the window (tweak as needed)
                Vector2 overlayPos = new Vector2(windowPos.X + 5, windowPos.Y + 5);

                ImGui.PushFont(editor.interBoldFont);

                // Draw overlay text
                drawList.AddText(overlayPos, ImGui.GetColorU32(new Vector4(0.5f, 0.5f, 0.5f, 1)), assetInfo.type);

                ImGui.PopFont();
            }
            ImGui.EndChild();

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor();
        }
    }
}
