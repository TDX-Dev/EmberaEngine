using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Editor.Utils
{
    public static class UIDragBehavior
    {

        public static unsafe void ResolveDragTarget(Type type)
        {
            //ImGuiPayloadPtr imGuiPayloadPtr = ImGui.AcceptDragDropPayload(type.Name);

            //Console.WriteLine(Marshal.PtrToStringAnsi(imGuiPayloadPtr.Data));
        }
    }
}
