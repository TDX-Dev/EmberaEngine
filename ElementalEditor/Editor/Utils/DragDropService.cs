using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ElementalEditor.Editor.Utils
{
    public class DragDropService
    {
        public DragFileItem DragFile;

        public void AddDragFile(string pathToFile)
        {
            DragFile = new DragFileItem() { fileextension = Path.GetExtension(pathToFile), path = pathToFile, fileName = Path.GetFileName(pathToFile) };
        }

        public DragFileItem GetDragFile()
        {
            return DragFile;
        }

    }

    public struct DragFileItem
    {
        public string fileName;
        public string path;
        public string fileextension;
    }
}