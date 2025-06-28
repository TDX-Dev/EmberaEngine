using EmberaEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberaEngine.Engine.Utilities;
using static EmberaEngine.Engine.Utilities.NewModelImporter;
using EmberaEngine.Engine.Components;

namespace ElementalEditor.Editor.Utils
{
    public static class ImporterUtils
    {
        public static GameObject ConvertToGameObjectTree(ModelGraphData graph)
        {
            GameObject rootObject = new GameObject();
            rootObject.Initialize();
            rootObject.Name = "RootObject";

            foreach (var child in graph.rootNode.children)
            {
                GameObject childObj = ConvertModelNodeToGameObject(child);
                if (childObj != null)
                    rootObject.AddChild(childObj);
            }

            return rootObject;
        }

        private static GameObject ConvertModelNodeToGameObject(ModelNode node)
        {
            GameObject obj = new GameObject();
            obj.Initialize();
            obj.Name = node.name;
            obj.transform.Position = node.position;
            obj.transform.Rotation = node.rotation;
            obj.transform.Scale = node.scale;

            switch (node.nodeType)
            {
                case ModelNodeType.Empty:
                    // Already created the base object with transform
                    break;

                case ModelNodeType.Mesh:
                    if (node is MeshNode meshNode)
                    {
                        var meshRenderer = obj.AddComponent<MeshRenderer>();
                        meshRenderer.SetMesh(meshNode.mesh);
                        // Optionally assign material by meshNode.materialID here
                    }
                    break;

                case ModelNodeType.Light:
                    if (node is LightNode lightNode)
                    {
                        var light = obj.AddComponent<LightComponent>();
                        light.LightType = lightNode.lightType;
                        light.Color = lightNode.colorDiffuse;
                        light.Intensity = lightNode.intensity;
                        light.Radius = lightNode.radius;
                    }
                    break;

                case ModelNodeType.Camera:
                    if (node is CameraNode cameraNode)
                    {
                        var camera = obj.AddComponent<CameraComponent3D>();
                        camera.Fov = cameraNode.fovy;
                        camera.NearPlane = cameraNode.near;
                        camera.FarPlane = cameraNode.far;
                    }
                    break;
            }

            // Recurse into children
            foreach (var child in node.children)
            {
                GameObject childObj = ConvertModelNodeToGameObject(child);
                if (childObj != null)
                    obj.AddChild(childObj);
            }

            return obj;
        }




    }
}
