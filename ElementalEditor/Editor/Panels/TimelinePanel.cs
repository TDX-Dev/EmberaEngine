using ImGuiNET;
using MaterialIconFont;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Panels
{
    class TimelinePanel : Panel
    {
        const float TIMELINE_TOOLBAR_HEADER_HEIGHT = 50;
        const float TIMELINE_TIME_HEADER_HEIGHT = 70;
        const float TIMELINE_PROPERTY_TAB_WIDTH = 400;
        Vector2 buttonSizing = new Vector2(50, 50);

        int currentFrame = 0;
        int startFrame = 0;
        int endFrame = 100;

        int subdivisionsTimeframe = 10;
        int bigLineSubdivisionAt = 5;
        int highlightTimelineLineEvery = 5;
        float timeFrameLineSpacing = 20.0f;

        float currentScrollPosition = 0;

        float zoomScaleFactor = 1;

        bool isDraggingFrame = false;


        public override void OnAttach()
        {

        }

        public override void OnGUI()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            if (ImGui.Begin("Timeline"))
            {

                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 0f));
                ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 0);

                if (ImGui.BeginChild("TIMELINE_TOOLBAR_HEADER", new Vector2(-1, TIMELINE_TOOLBAR_HEADER_HEIGHT)))
                {
                    if (ImGui.Button(MaterialDesign.Pause_circle_outline, buttonSizing))
                    {

                    }

                    ImGui.PushItemWidth(100);

                    ImGui.SameLine();
                    ImGui.Text("Frame:");

                    ImGui.SameLine();
                    ImGui.DragInt("##currentFrame", ref currentFrame, 1, 5);

                    ImGui.SameLine();
                    ImGui.Text("Start: ");
                    ImGui.SameLine();
                    ImGui.DragInt("##startFrame", ref startFrame, 1, 0);
                    startFrame = Math.Clamp(startFrame, 0, int.MaxValue);

                    ImGui.SameLine();
                    ImGui.Text("End: ");
                    ImGui.SameLine();
                    ImGui.DragInt("##endFrame", ref endFrame, 1, 5);

                    ImGui.PopItemWidth();
                }
                ImGui.EndChild();

                ImGui.PopStyleColor();
                ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0.2f, 0.2f, 0.2f, 1f));

                ImGui.SetCursorPosX(TIMELINE_PROPERTY_TAB_WIDTH);
                if (ImGui.BeginChild("TIMELINE_TIME_HEADER", new Vector2(-1, TIMELINE_TIME_HEADER_HEIGHT)))
                {
                    DrawTimeHeader();
                    HandleUserInput();
                }
                ImGui.EndChild();

                ImGui.SetCursorPosX(TIMELINE_PROPERTY_TAB_WIDTH);
                if (ImGui.BeginChild("TIMELINE_EDITOR", new Vector2(-1, -1)))
                {
                    HandleUserInput();
                    DrawTimeLine();
                }
                ImGui.EndChild();

                ImGui.PopStyleColor();
                ImGui.PopStyleVar();
            }
            ImGui.End();

            ImGui.PopStyleVar();
        }

        void DrawTimeLine()
        {
            Vector2 windowPos = ImGui.GetWindowPos();

            Vector2 clipMin = ImGui.GetCursorScreenPos();
            Vector2 clipMax = ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail();
            float scrollX = ImGui.GetScrollX();

            Vector2 basePosition = windowPos;
            basePosition.X += 10 - scrollX + currentScrollPosition;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(clipMin, clipMax, true);

            float visibleStartX = clipMin.X;
            float visibleEndX = clipMax.X;

            int firstVisibleFrame = (int)Math.Floor((visibleStartX - basePosition.X) / timeFrameLineSpacing);
            int lastVisibleFrame = (int)Math.Ceiling((visibleEndX - basePosition.X) / timeFrameLineSpacing);


            for (int i = firstVisibleFrame; i <= lastVisibleFrame; i++)
            {
                if (i % highlightTimelineLineEvery != 0) continue;
                Vector2 linePos = basePosition;
                linePos.X += i * timeFrameLineSpacing;

                Vector2 lineStart = linePos;
                Vector2 lineEnd = linePos;

                uint uiColor = (i % subdivisionsTimeframe == 0) ? ImGui.ColorConvertFloat4ToU32(new Vector4(0.7f, 0.7f, 0.7f, 1)) : ImGui.ColorConvertFloat4ToU32(new Vector4(0.3f, 0.3f, 0.3f, 1));

                lineEnd.Y += ImGui.GetWindowSize().Y;

                drawList.AddLine(lineStart, lineEnd, uiColor);
            }

            float lineTop = basePosition.Y;
            float lineBottom = clipMax.Y;

            Vector4 cutoutColor = new Vector4(0.9f, 0.2f, 0f, 1f);
            uint ucutoutColor = ImGui.ColorConvertFloat4ToU32(cutoutColor);

            Vector2 startLinePos = new Vector2(basePosition.X + startFrame * timeFrameLineSpacing, lineTop);
            Vector2 endLinePos = new Vector2(basePosition.X + endFrame * timeFrameLineSpacing, lineTop);

            drawList.AddLine(startLinePos, new Vector2(startLinePos.X, lineBottom), ucutoutColor);
            drawList.AddLine(endLinePos, new Vector2(endLinePos.X, lineBottom), ucutoutColor);

            DrawOpaqueBoundaryRect(basePosition, clipMin, clipMax, drawList);

            startLinePos = new Vector2(basePosition.X + currentFrame * timeFrameLineSpacing, lineTop);

            Vector4 frameIndicatorLineColor = new Vector4(0.1f, 0.1f, 0.9f, 1);
            uint uframeIndicatorLineColor = ImGui.ColorConvertFloat4ToU32(frameIndicatorLineColor);

            drawList.AddLine(startLinePos, new Vector2(startLinePos.X, lineBottom), uframeIndicatorLineColor);


            drawList.PopClipRect();

        }

        void DrawOpaqueBoundaryRect(Vector2 basePosition, Vector2 clipMin, Vector2 clipMax, ImDrawListPtr drawList)
        {
            Vector4 overlayColor = new Vector4(0f, 0f, 0f, 0.3f);
            uint uOverlayColor = ImGui.ColorConvertFloat4ToU32(overlayColor);

            float startFrameX = basePosition.X + startFrame * timeFrameLineSpacing;
            float endFrameX = basePosition.X + endFrame * timeFrameLineSpacing;

            float shadedLeftX = Math.Max(clipMin.X, clipMin.X);
            float shadedRightX = Math.Min(clipMax.X, clipMax.X);

            if (startFrameX > clipMin.X)
            {
                drawList.AddRectFilled(
                    new Vector2(shadedLeftX, clipMin.Y),
                    new Vector2(Math.Min(startFrameX, clipMax.X), clipMax.Y),
                    uOverlayColor
                );
            }

            if (endFrameX < clipMax.X)
            {
                drawList.AddRectFilled(
                    new Vector2(Math.Max(endFrameX, clipMin.X), clipMin.Y),
                    new Vector2(shadedRightX, clipMax.Y),
                    uOverlayColor
                );
            }

        }

        void DrawTimeHeader()
        {
            Vector2 windowPos = ImGui.GetWindowPos();
            Vector2 clipMin = ImGui.GetCursorScreenPos();
            Vector2 clipMax = ImGui.GetCursorScreenPos() + ImGui.GetContentRegionAvail();


            float scrollX = ImGui.GetScrollX();

            Vector2 basePosition = windowPos;
            basePosition.X += 10 - scrollX + currentScrollPosition;
            basePosition.Y += TIMELINE_TIME_HEADER_HEIGHT - 20;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();
            drawList.PushClipRect(clipMin, clipMax, true);

            float visibleStartX = clipMin.X;
            float visibleEndX = clipMax.X;

            int firstVisibleFrame = (int)Math.Floor((visibleStartX - basePosition.X) / timeFrameLineSpacing);
            int lastVisibleFrame = (int)Math.Ceiling((visibleEndX - basePosition.X) / timeFrameLineSpacing);

            for (int i = firstVisibleFrame; i <= lastVisibleFrame; i++)
            {
                Vector2 linePos = basePosition;
                linePos.X += i * timeFrameLineSpacing;

                Vector2 lineStart = linePos;
                Vector2 lineEnd = linePos;

                Vector4 lineColor = (i >= startFrame && i <= endFrame) ? new Vector4(0.7f, 0.7f, 0.7f, 1) : new Vector4(0.3f, 0.3f, 0.3f, 1);
                uint uiColor = ImGui.ColorConvertFloat4ToU32(lineColor);

                int labelFrequency = 1;

                if (timeFrameLineSpacing < 10f)
                    labelFrequency = 10;
                else if (timeFrameLineSpacing < 20f)
                    labelFrequency = 5;
                else if (timeFrameLineSpacing < 50f)
                    labelFrequency = 2;
                else
                    labelFrequency = 1;

                if (i % labelFrequency == 0)
                {
                    lineEnd.Y += 20;

                    Vector2 textSize = ImGui.CalcTextSize(i.ToString());
                    Vector2 textPos = lineStart - new Vector2(textSize.X / 2, 20);


                    drawList.AddText(textPos, uiColor, i.ToString());
                }
                else
                {
                    lineStart.Y += 10;
                    lineEnd.Y += 20;
                }



                //uint uiColor = ImGui.ColorConvertFloat4ToU32(lineColor);


                drawList.AddLine(lineStart, lineEnd, uiColor);
            }

            // THIS IS WHERE I WANT THE DRAGGING

            float lineTop = basePosition.Y + 20;
            float lineBottom = clipMax.Y;

            Vector2 startLinePos = new Vector2(basePosition.X + currentFrame * timeFrameLineSpacing, lineTop);

            Vector4 frameIndicatorLineColor = new Vector4(0.1f, 0.1f, 0.9f, 1);
            uint uframeIndicatorLineColor = ImGui.ColorConvertFloat4ToU32(frameIndicatorLineColor);

            float currentFrameX = basePosition.X + currentFrame * timeFrameLineSpacing;

            string frameLabel = currentFrame.ToString();
            Vector2 textSize2 = ImGui.CalcTextSize(frameLabel);

            float paddingX = 15f;
            float paddingY = 8f;
            float radius = 5f;

            Vector2 rectCenter = new Vector2(currentFrameX, lineTop - 10 - textSize2.Y / 2 - paddingY);
            Vector2 rectMin = new Vector2(rectCenter.X - textSize2.X / 2 - paddingX, rectCenter.Y - textSize2.Y / 2 - paddingY);
            Vector2 rectMax = new Vector2(rectCenter.X + textSize2.X / 2 + paddingX, rectCenter.Y + textSize2.Y / 2 + paddingY);

            uint backgroundColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.8f, 1f));
            uint textColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f));

            drawList.AddRectFilled(rectMin, rectMax, backgroundColor, radius);

            Vector2 textPos2 = new Vector2(
                rectCenter.X - textSize2.X / 2,
                rectCenter.Y - textSize2.Y / 2
            );
            drawList.AddText(textPos2, textColor, frameLabel);
            drawList.AddLine(startLinePos, new Vector2(startLinePos.X, lineBottom), uframeIndicatorLineColor);


            // Add this above `drawList.PopClipRect();` in DrawTimeHeader()

            Vector2 mousePos = ImGui.GetMousePos();

            bool isHoveringFrameLabel =
                mousePos.X >= rectMin.X && mousePos.X <= rectMax.X &&
                mousePos.Y >= rectMin.Y && mousePos.Y <= rectMax.Y;

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left) && isHoveringFrameLabel)
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                ImGui.SetNextFrameWantCaptureMouse(true); // Optional, for full capture
                isDraggingFrame = true;
            }

            if (isDraggingFrame && ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                float timelineX = mousePos.X - basePosition.X;
                int newFrame = (int)Math.Round(timelineX / timeFrameLineSpacing);
                currentFrame = Math.Clamp(newFrame, startFrame, endFrame);
            }

            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
            {
                isDraggingFrame = false;
            }


            drawList.PopClipRect();
        }


        void HandleUserInput()
        {
            if (ImGui.IsWindowHovered() && ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
            {
                currentScrollPosition += ImGui.GetIO().MouseDelta.X;
            }

            if (ImGui.IsWindowHovered())
            {
                float mouseWheel = ImGui.GetIO().MouseWheel;
                if (mouseWheel != 0)
                {
                    float zoomSpeed = 1.1f;
                    float oldSpacing = timeFrameLineSpacing;

                    if (mouseWheel > 0)
                        timeFrameLineSpacing *= zoomSpeed;
                    else
                        timeFrameLineSpacing /= zoomSpeed;

                    timeFrameLineSpacing = Math.Clamp(timeFrameLineSpacing, 5f, 200f);

                    float mouseX = ImGui.GetMousePos().X;
                    float scrollDelta = (mouseX - (ImGui.GetWindowPos().X + currentScrollPosition)) / oldSpacing;

                    currentScrollPosition += (scrollDelta * (oldSpacing - timeFrameLineSpacing));
                }
            }
        }


    }
}
