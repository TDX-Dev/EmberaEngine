﻿using ElementalEditor.Editor;
using EmberaEngine.Engine.Core;

namespace ElementalEditor
{



    public class Elemental
    {
        public Elemental()
        {

        }

        public void Run(string projectFilePath)
        {
            Application app = new Application();

            ApplicationSpecification specification = new ApplicationSpecification()
            {
                Name = "Devoid Engine - Elemental Editor",
                Height = (int)(1080 * 1.5f),
                Width = (int)(1920 * 1.5f),
                forceVsync = false,
                useImGui = true,
                useImGuiDock = true,
                useCustomTitlebar = false
            };

            EditorLayer layer = new EditorLayer();
            layer.projectPath = projectFilePath;
            layer.app = app;

            app.Create(specification);


            app.AddLayer(layer);

            app.Run();
        }
    }
}