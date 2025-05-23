﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Rendering
{
    public static class RenderGraph
    {


        // Above this limit, it is best to instance the meshes.
        public static int MAX_MESH_COUNT = 1000000;

        public static int CURRENT_MESH_COUNT = 0;

        public static int MAX_POINT_LIGHTS = 1000000;
        public static int MAX_DIR_LIGHTS = 1000000;
        public static int MAX_SPOT_LIGHTS = 1000000;


        public static bool isGraphicsContextInitialized = false;



    }
}
