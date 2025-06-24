using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;
using MaterialIconFont;

public static class FolderDropdownWidget
{
    private static Dictionary<string, bool> _treeNodeStates = new();

    public static void FolderDropdown(string label, ref string currentPath, string rootPath)
    {
        if (ImGui.BeginCombo(label, currentPath))
        {
            RenderFolderTree(rootPath, ref currentPath);
            ImGui.EndCombo();
        }
    }
    private static void RenderFolderTree(string directoryPath, ref string selectedPath)
    {
        if (!Directory.Exists(directoryPath))
            return;

        var directories = Directory.GetDirectories(directoryPath);
        foreach (var dir in directories)
        {
            string dirName = Path.GetFileName(dir);
            bool hasChildren = Directory.GetDirectories(dir).Length > 0;
            bool isSelected = dir == selectedPath;

            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.SpanAvailWidth;
            if (!hasChildren)
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

            if (isSelected)
                flags |= ImGuiTreeNodeFlags.Selected;

            // Push custom selected color if this node is selected
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Header, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 1.0f));  // Customize your color here
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.0f, 0.4f, 0.7f, 1.0f));
            }

            bool nodeOpen = ImGui.TreeNodeEx(dir, flags, MaterialDesign.Folder + " " + dirName);

            if (isSelected)
                ImGui.PopStyleColor(2);

            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                selectedPath = dir;

            if (hasChildren && nodeOpen)
                RenderFolderTree(dir, ref selectedPath);

            if (hasChildren && nodeOpen)
                ImGui.TreePop();
        }
    }







}
