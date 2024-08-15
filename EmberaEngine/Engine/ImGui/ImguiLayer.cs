using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Core;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;

using ImGuiNET;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Imgui
{
    public class ImguiLayer : Layer
    {

        IntPtr Context;
        public ImguiAPI imguiAPI;
        public static bool customTitlebar = false;
        public static bool UseDockspace = false;

        public void InitIMGUI(GameWindow window, int width, int height)
        {

            Context = ImGui.CreateContext();
            ImGui.SetCurrentContext(Context);

            imguiAPI = new ImguiAPI(window, width, height);
        }

        public override void OnGUIRender()
        {

        }

        public override void OnUpdate(float deltaTime)
        {
            
            base.OnUpdate(deltaTime);
        }

        public void SetFont(ImFontPtr ptr)
        {
            imguiAPI._CurrentFont = ptr;
        }

        public void SetIconFont(string path, int size, (ushort, ushort) range)
        {
            ImguiAPI.LoadIconFont(path, size, range);
        }

        public void RecreateFontDevice()
        {
            imguiAPI.DestroyDeviceObjects();
            imguiAPI.CreateDeviceResources();
        }

        public void Begin(float deltaTime)
        {
            imguiAPI.Update(deltaTime);
            if (ImguiLayer.UseDockspace)
            {
                imguiAPI.SetUpDockspace(customTitlebar);
            }
        }
        
        public void End()
        {
            ImGui.End();

            imguiAPI.Render();
        }
        public override void OnRender()
        {

        }

        public override void OnTextInput(TextInputEvent textInputEvent)
        {
            imguiAPI.PressChar((char)textInputEvent.Unicode);
        }

        public override void OnResize(int width, int height)
        {
            imguiAPI.OnResize(width, height);
            base.OnResize(width, height);
        }

        public override void OnDetach()
        {
            imguiAPI.Dispose();
        }

        public static string defaultINIFallback = @"
            [Window][Dockspace]
            Pos=0,0
            Size=1920,1061
            Collapsed=0

            [Window][Debug##Default]
            Pos=74,63
            Size=400,400
            Collapsed=0

            [Window][Game View]
            Pos=306,78
            Size=1298,605
            Collapsed=0
            DockId=0x0000000B,0

            [Window][Engine Details]
            Pos=1606,30
            Size=306,653
            Collapsed=0
            DockId=0x00000002,2

            [Window][Scene Hierarchy]
            Pos=8,30
            Size=296,653
            Collapsed=0
            DockId=0x00000008,0

            [Window][Object Properties]
            Pos=1606,30
            Size=306,653
            Collapsed=0
            DockId=0x00000002,0

            [Window][Content Browser]
            Pos=306,685
            Size=1606,368
            Collapsed=0
            DockId=0x00000007,0

            [Window][PostProcess Profile]
            Pos=1606,30
            Size=306,653
            Collapsed=0
            DockId=0x00000002,1

            [Window][ViewportTools]
            Pos=306,30
            Size=1298,46
            Collapsed=0
            DockId=0x0000000A,0

            [Window][Scene Details]
            Pos=8,30
            Size=296,653
            Collapsed=0
            DockId=0x00000008,1

            [Window][Folder Hierarchy]
            Pos=8,685
            Size=296,368
            Collapsed=0
            DockId=0x00000005,0

            [Window][ss]
            Pos=60,60
            Size=108,60
            Collapsed=0

            [Docking][Data]
            DockSpace         ID=0x33675C32 Window=0x5B816B74 Pos=8,30 Size=1904,1023 Split=Y
              DockNode        ID=0x00000003 Parent=0x33675C32 SizeRef=1904,609 Split=X
                DockNode      ID=0x00000001 Parent=0x00000003 SizeRef=1596,979 Split=X Selected=0x642CEEBB
                  DockNode    ID=0x00000008 Parent=0x00000001 SizeRef=296,651 Selected=0x2E9237F7
                  DockNode    ID=0x00000009 Parent=0x00000001 SizeRef=1298,651 Split=Y Selected=0x642CEEBB
                    DockNode  ID=0x0000000A Parent=0x00000009 SizeRef=1341,46 HiddenTabBar=1 Selected=0xB57E89BE
                    DockNode  ID=0x0000000B Parent=0x00000009 SizeRef=1341,561 CentralNode=1 Selected=0x642CEEBB
                DockNode      ID=0x00000002 Parent=0x00000003 SizeRef=306,979 Selected=0x625EA9E1
              DockNode        ID=0x00000004 Parent=0x33675C32 SizeRef=1904,368 Split=X Selected=0x6BFAD16C
                DockNode      ID=0x00000005 Parent=0x00000004 SizeRef=296,368 Selected=0x6BFAD16C
                DockNode      ID=0x00000007 Parent=0x00000004 SizeRef=1606,368 Selected=0xBF096F38

";
    }
}

