using ElementalEditor.Editor.AssetHandling;
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

        int folderTabWidth = 400;
        int directoryTabHeight = 50;
        int assetTilePadding = 10;

        string rootPath;
        string currentPath;

        List<ProjectAssetPanelType> currentPathAssets;
        List<string> directoryContents;
        Dictionary<string, TextureReference> textureAssetCache;

        ProjectAssetPanelType currentSelectedFile;

        public void UpdatePaths()
        {
            Console.WriteLine("Updating Path");
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

            UpdatePaths();
        }

        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));

            if (ImGui.Begin("Project Assets"))
            {
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(5, 5));

                Vector2 cursorPos = ImGui.GetCursorPos();

                if (ImGui.BeginChild("foldersTab", new Vector2(folderTabWidth, -1), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    if (currentPath != rootPath)
                    {
                        if (DirectoryButton(".."))
                        {
                            currentPath = Path.GetFullPath(Path.Combine(currentPath, @".."));
                            UpdatePaths();
                        }
                    }

                    for (int i = 0; i < directoryContents.Count; i++)
                    {
                        if (DirectoryButton(directoryContents[i]))
                        {
                            currentPath = Path.GetFullPath(Path.Combine(currentPath, directoryContents[i]));
                            UpdatePaths();
                        }
                    }

                    ImGui.EndChild();
                }
                ImGui.PopStyleVar();

                ImGui.SetCursorPos(new Vector2(cursorPos.X + folderTabWidth, cursorPos.Y));
                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(20, 15));

                if (ImGui.BeginChild("directory_ind", new Vector2(-1, directoryTabHeight), false, ImGuiWindowFlags.AlwaysUseWindowPadding))
                {
                    ImGui.Text(Path.GetRelativePath(editor.projectPath, currentPath));

                    ImGui.EndChild();
                }

                ImGui.PopStyleVar();

                ImGui.SetCursorPos(new Vector2(cursorPos.X + folderTabWidth + assetTilePadding, cursorPos.Y + directoryTabHeight + assetTilePadding));

                // Set the size of the grid area (height - directoryTabHeight)
                Vector2 gridAreaSize = new Vector2(-1, ImGui.GetContentRegionAvail().Y); // fills remaining vertical space

                if (ImGui.BeginChild("assetGrid", gridAreaSize, false, ImGuiWindowFlags.AlwaysUseWindowPadding | ImGuiWindowFlags.HorizontalScrollbar))
                {
                    DrawAssetGrid(currentPathAssets);
                    ImGui.EndChild();
                }



                ImGui.End();
            }

            ImGui.PopStyleVar(2);
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


            ImGui.BeginChild(assetInfo.name, new Vector2(assetCardWidth, assetCardHeight), true, ImGuiWindowFlags.AlwaysUseWindowPadding);

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

            ImGui.BeginChild(assetInfo.name + "desc", Vector2.One * -1, false, ImGuiWindowFlags.AlwaysUseWindowPadding);

            ImGui.Text(assetInfo.name);

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

                // Custom drag widget (thumbnail + name)
                ImGui.BeginGroup();
                ImGui.Image(thumbnailTexture.GetRendererID(), new Vector2(40, 40)); // Small preview
                ImGui.SameLine();
                ImGui.Text(assetInfo.name);
                ImGui.TextColored(new Vector4(0.2f, 0.2f, 0.2f, 1f), assetInfo.type);
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

            ImGui.EndChild();

            ImGui.PopStyleVar(3);
            ImGui.PopStyleColor();
        }
    }
}
