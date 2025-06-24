using Assimp;
using EmberaEngine.Engine.AssetHandling;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Resources;
using System.Text;
using System.Text.Encodings;
using static System.Net.Mime.MediaTypeNames;

namespace EmberaEngine.Engine.Utilities
{
    public class ModelImporter
    {
        public struct ModelData
        {
            public GameObject rootObject; // This contains everything.
            public List<GameObject> meshObjects;
            public List<GameObject> cameras;
            public List<GameObject> lights;
        }

        static Dictionary<string, Core.Texture> Textures = new Dictionary<string, Core.Texture>();
        static Dictionary<int, uint> processedMaterialIndices = new Dictionary<int, uint>();

        public static ModelData LoadModel(string path)
        {
            string virtualPath = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            string physicalPath = Path.GetDirectoryName(VirtualFileSystem.ResolvePath(virtualPath));

            var importer = new AssimpContext();
            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(VirtualFileSystem.ResolvePath(path),
                    PostProcessSteps.Triangulate |
                    PostProcessSteps.GenerateNormals |
                    PostProcessSteps.CalculateTangentSpace |
                    PostProcessSteps.GenerateSmoothNormals |
                    PostProcessSteps.FlipUVs |
                    PostProcessSteps.GenerateUVCoords |
                    //PostProcessSteps.OptimizeGraph |
                    PostProcessSteps.OptimizeMeshes
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ModelImporter] Failed to load model: {virtualPath}\nError: {e.Message}");
                return default;
            }

            if (scene?.RootNode == null) return default;

            List<GameObject> meshObjects = new List<GameObject>();
            List<GameObject> cameras = new List<GameObject>();
            List<GameObject> lights = new List<GameObject>();

            GameObject rootGO = new GameObject();
            rootGO.Name = Path.GetFileNameWithoutExtension(virtualPath);
            int totalMeshCount = scene.MeshCount;
            int meshProcessed = 0;

            // 1. Process materials first
            for (int i = 0; i < scene.MaterialCount; i++)
            {
                if (!processedMaterialIndices.ContainsKey(i))
                {
                    var material = SetupMaterial(i, scene, virtualPath);
                    var materialID = MaterialManager.AddMaterial(material);
                    processedMaterialIndices[i] = materialID;
                }
            }

            // 2. Now process meshes (via node traversal)
            void ProcessNode(Node node, GameObject parentGO)
            {
                // If the node has meshes and no children, attach meshes directly to the parentGO
                bool hasMeshes = node.MeshIndices.Count > 0;
                bool hasChildren = node.Children.Count > 0;

                GameObject currentGO = parentGO;

                node.Transform.DecomposeNoScaling(out Assimp.Quaternion rot, out Vector3D assimpPos);

                // Only create an intermediate node if it has children (for transform hierarchy) or non-mesh purpose
                if (hasChildren && node.Name != parentGO.Name)
                {
                    currentGO = new GameObject();
                    currentGO.Name = node.Name;
                    parentGO.AddChild(currentGO);
                }

                foreach (int meshIdx in node.MeshIndices)
                {
                    var assimpMesh = scene.Meshes[meshIdx];
                    var processedMesh = ProcessMesh(assimpMesh, scene, node.Transform, virtualPath);

                    var meshGO = new GameObject();
                    meshGO.Initialize();
                    meshGO.Name = assimpMesh.Name != "" ? assimpMesh.Name.Substring(0, Math.Min(10, assimpMesh.Name.Length)) : $"Mesh_{meshIdx}";

                    var meshRenderer = meshGO.AddComponent<MeshRenderer>();
                    meshRenderer.SetMesh(processedMesh);

                    currentGO.AddChild(meshGO);
                    meshObjects.Add(meshGO);

                    meshProcessed++;
                }

                foreach (var child in node.Children)
                {
                    ProcessNode(child, currentGO);
                }
            }

            foreach (var child in scene.RootNode.Children)
            {
                ProcessNode(child, rootGO);
            }


            foreach (var cam in scene.Cameras)
            {
                if (scene.RootNode.FindNode(cam.Name) is Node node)
                {
                    var parentGO = FindGOByName(rootGO, cam.Name);
                    if (parentGO != null)
                    {
                        var cameraComponent = parentGO.AddComponent<CameraComponent3D>();
                        cameraComponent.FarPlane = cam.ClipPlaneFar;
                        cameraComponent.NearPlane = cam.ClipPlaneNear;
                        cameraComponent.Fov = MathHelper.RadiansToDegrees(cam.FieldOfview);
                        parentGO.transform.Position = new OpenTK.Mathematics.Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z);
                        parentGO.transform.Rotation = new OpenTK.Mathematics.Vector3(cam.Direction.X, cam.Direction.Y, cam.Direction.Z);
                        cameras.Add(parentGO);
                        continue;
                    }
                }
                var gameObject = new GameObject();
                gameObject.Initialize();
                gameObject.Name = cam.Name;
                var fallbackCamera = gameObject.AddComponent<CameraComponent3D>();
                fallbackCamera.FarPlane = cam.ClipPlaneFar;
                fallbackCamera.NearPlane = cam.ClipPlaneNear;
                fallbackCamera.Fov = MathHelper.RadiansToDegrees(cam.FieldOfview);
                gameObject.transform.Position = new OpenTK.Mathematics.Vector3(cam.Position.X, cam.Position.Y, cam.Position.Z);
                gameObject.transform.Rotation = new OpenTK.Mathematics.Vector3(cam.Direction.X, cam.Direction.Y, cam.Direction.Z);
                rootGO.AddChild(gameObject);
                cameras.Add(gameObject);
            }



            foreach (var light in scene.Lights)
            {
                if (!TryGetLightType(light.LightType, out var lightType))
                    continue;

                GameObject targetGO = null;

                if (scene.RootNode.FindNode(light.Name) is Node node)
                {
                    targetGO = FindGOByName(rootGO, light.Name);
                    if (targetGO != null)
                    {
                        
                        var worldTransform = ToOpenTKMatrix(GetNodeWorldTransform(node));
                        var direction = worldTransform * new OpenTK.Mathematics.Vector4(light.Direction.X, light.Direction.Y, light.Direction.Z, 0f);
                        var position = worldTransform * new OpenTK.Mathematics.Vector4(light.Position.X, light.Position.Y, light.Position.Z, 1f);

                        targetGO.transform.Rotation = direction.Xyz.Normalized();

                        node.Transform.DecomposeNoScaling(out Assimp.Quaternion rot, out Vector3D assimpPos);
                        targetGO.transform.Position = new OpenTK.Mathematics.Vector3(assimpPos.X, assimpPos.Y, assimpPos.Z) * 0.02f;
                    }


                    // Fallback: create a new GO if not found
                    if (targetGO == null)
                    {
                        targetGO = new GameObject();
                        targetGO.Initialize();
                        targetGO.Name = light.Name;
                        node.Transform.DecomposeNoScaling(out Assimp.Quaternion rot, out Vector3D assimpPos);
                        targetGO.transform.Position = new OpenTK.Mathematics.Vector3(assimpPos.X, assimpPos.Y, assimpPos.Z) * 0.02f;
                        targetGO.transform.Rotation = new OpenTK.Mathematics.Vector3(light.Direction.X, light.Direction.Y, light.Direction.Z);
                        rootGO.AddChild(targetGO);
                    }

                }

                var lightComponent = targetGO.AddComponent<LightComponent>();
                lightComponent.LightType = lightType;
                lightComponent.Enabled = true;
                lightComponent.Radius = ComputeLightRange(light) * 0.02f;

                float intensity = 0.2126f * light.ColorDiffuse.R + 0.7152f * light.ColorDiffuse.G + 0.0722f * light.ColorDiffuse.B;
                lightComponent.Color = new Color4((float)Math.Clamp(light.ColorDiffuse.R, 0, 1), (float)Math.Clamp(light.ColorDiffuse.G, 0, 1), (float)Math.Clamp(light.ColorDiffuse.B, 0, 1), 1f);
                lightComponent.Intensity = Math.Max(Math.Max(light.ColorDiffuse.R, light.ColorDiffuse.G), light.ColorDiffuse.B) / 100;
                lightComponent.OuterCutoff = MathHelper.RadiansToDegrees(light.AngleOuterCone) / 2;
                lightComponent.InnerCutoff = MathHelper.RadiansToDegrees(light.AngleInnerCone) / 2;
                lightComponent.LinearFactor = light.AttenuationLinear;
                lightComponent.QuadraticFactor = light.AttenuationQuadratic;

                lights.Add(targetGO);
            }


            importer.Dispose();

            //foreach (var mesh in totalMeshes)
            //{
            //    mesh.SetPath(path);
            //    mesh.fileID = Path.GetFileName(path);
            //}

            return new ModelData { rootObject = rootGO, meshObjects = meshObjects, cameras = cameras, lights = lights };
        }

        static bool TryGetLightType(LightSourceType type, out LightType result)
        {
            result = type switch
            {
                LightSourceType.Point => LightType.PointLight,
                LightSourceType.Spot => LightType.SpotLight,
                LightSourceType.Directional => LightType.DirectionalLight,
                _ => default
            };
            return type == LightSourceType.Point || type == LightSourceType.Spot || type == LightSourceType.Directional;
        }


        public static Mesh ProcessMesh(Assimp.Mesh mesh, Assimp.Scene scene, Assimp.Matrix4x4 transform, string path = "")
        {
            List<Vertex> vertices = new List<Vertex>();
            int[] indices = mesh.GetIndices();

            OpenTK.Mathematics.Vector3 position = Matrix4.Transpose(ToOpenTKMatrix(transform)).ExtractTranslation();

            Matrix4 modelMatrix = ToOpenTKMatrix(transform) * Matrix4.CreateScale(0.02f);

            // Compute normal matrix (3x3 inverse-transpose of the upper-left model matrix)
            Matrix3 normalMatrix = new Matrix3(modelMatrix);
            normalMatrix = Matrix3.Transpose(Matrix3.Invert(normalMatrix));


            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex;
                OpenTK.Mathematics.Vector3 modifiedVertex = /*new OpenTK.Mathematics.Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);*/ (OpenTK.Mathematics.Vector4.TransformColumn(ToOpenTKMatrix(transform), new OpenTK.Mathematics.Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1)) * Matrix4.CreateScale(0.02f)).Xyz;
                //OpenTK.Mathematics.Vector3 modifiedNormals = /*new OpenTK.Mathematics.Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);*/ (OpenTK.Mathematics.Vector4.TransformColumn(ToOpenTKMatrix(transform), new OpenTK.Mathematics.Vector4((mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, 1))) * Matrix4.CreateScale(0.02f)).Xyz;
                //OpenTK.Mathematics.Vector3 modifiedTangents = new OpenTK.Mathematics.Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);
                //OpenTK.Mathematics.Vector3 modifiedBiTangents = new OpenTK.Mathematics.Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z);

                OpenTK.Mathematics.Vector3 originalNormal = new OpenTK.Mathematics.Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                OpenTK.Mathematics.Vector3 modifiedNormals = OpenTK.Mathematics.Vector3.Normalize(TransformNormal(normalMatrix, originalNormal));

                // Transform tangent (w=0, same treatment as normal)
                OpenTK.Mathematics.Vector3 originalTangent = new OpenTK.Mathematics.Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);
                OpenTK.Mathematics.Vector3 modifiedTangents = OpenTK.Mathematics.Vector3.Normalize(TransformNormal(normalMatrix, originalTangent));

                // Copy bitangent directly, or recompute in shader if using handedness
                OpenTK.Mathematics.Vector3 originalBiTangent = new OpenTK.Mathematics.Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z);
                OpenTK.Mathematics.Vector3 modifiedBiTangents = OpenTK.Mathematics.Vector3.Normalize(TransformNormal(normalMatrix, originalBiTangent));

                if (mesh.TextureCoordinateChannels[0].Count != 0)
                {
                    modifiedNormals.Normalize();
                    if (mesh.Tangents.Count > 0 && mesh.BiTangents.Count > 0)
                    {
                        vertex = new Vertex(modifiedVertex, modifiedNormals, new OpenTK.Mathematics.Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y), modifiedTangents, modifiedBiTangents);
                    }
                    else
                    {
                        vertex = new Vertex(modifiedVertex, modifiedNormals, new OpenTK.Mathematics.Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                    }
                }
                else
                {
                    vertex = new Vertex(modifiedVertex, modifiedNormals, OpenTK.Mathematics.Vector2.Zero, modifiedTangents, modifiedBiTangents);
                }
                vertices.Add(vertex);
            }
            Mesh mesh1 = new Mesh();
            mesh1.Name = mesh.Name;
            //mesh1.MeshID = mesh.Name.GetHashCode();
            mesh1.SetPath(path);
            mesh1.SetVertices(vertices.ToArray());
            if (indices.Length != 0)
            {
                mesh1.SetIndices(indices);
            }

            uint materialID;

            if (processedMaterialIndices.ContainsKey(mesh.MaterialIndex))
            {
                materialID = processedMaterialIndices[mesh.MaterialIndex];
            }
            else
            {
                Core.Material material = SetupMaterial(mesh.MaterialIndex, scene, path);

                materialID = MaterialManager.AddMaterial(material);
                processedMaterialIndices[mesh.MaterialIndex] = materialID;
            }

            //mesh1.MaterialIndex = m;
            return mesh1;
        }

        static PBRMaterial SetupMaterial(int matIndex, Assimp.Scene scene, string baseDir)
        {
            var assimpMat = scene.Materials[matIndex];
            var mat = new PBRMaterial();
            mat.SetDefaults();

            mat.Albedo = new Color4(
                assimpMat.ColorDiffuse.R,
                assimpMat.ColorDiffuse.G,
                assimpMat.ColorDiffuse.B,
                assimpMat.ColorDiffuse.A
            );

            mat.Metallic = 0f;
            mat.Roughness = 1f - assimpMat.Reflectivity;

            mat.Emission = new Color4(
                assimpMat.ColorEmissive.R,
                assimpMat.ColorEmissive.G,
                assimpMat.ColorEmissive.B,
                1.0f
            );
            mat.EmissionStrength = assimpMat.TextureEmissive.BlendFactor;

            TrySetTexture(assimpMat, TextureType.Diffuse, tex => mat.DiffuseTexture = tex, baseDir);
            TrySetTexture(assimpMat, TextureType.Normals, tex => mat.NormalTexture = tex, baseDir);
            TrySetTexture(assimpMat, TextureType.Shininess, tex => mat.RoughnessTexture = tex, baseDir);
            TrySetTexture(assimpMat, TextureType.Emissive, tex => mat.EmissionTexture = tex, baseDir);

            return mat;
        }

        static void TrySetTexture(
            Assimp.Material mat,
            TextureType type,
            Action<Texture> setTexture,
            string baseDir)
        {
            if (!mat.GetMaterialTexture(type, 0, out TextureSlot texSlot))
                return;

            string fullPath = Path.Combine(baseDir, texSlot.FilePath);

            var textureRef = (TextureReference)AssetLoader.Load<Texture>(fullPath);
            if (textureRef == null)
                return;

            if (textureRef.isLoaded)
            {
                SetupTexture(textureRef.value, texSlot, setTexture);
            }
            else
            {
                textureRef.OnLoad += tex => SetupTexture(tex, texSlot, setTexture);

                // Load null texture until texture loads.
                SetupTexture(Helper.loadImageAsTex("Engine/Content/Textures/Placeholders/null.png"), texSlot, setTexture);
            }
        }

        static void SetupTexture(Texture texture, TextureSlot texSlot, Action<Texture> setTexture)
        {
            texture.SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            texture.SetAnisotropy(8f);
            texture.GenerateMipmap();
            SetWrap(texSlot.WrapModeU, texSlot.WrapModeV, texture);

            setTexture(texture);
        }

        static Assimp.Matrix4x4 GetNodeWorldTransform(Node node)
        {
            Assimp.Matrix4x4 transform = node.Transform;

            Node current = node.Parent;
            while (current != null)
            {
                transform = current.Transform * transform;
                current = current.Parent;
            }

            return transform;
        }

        public static OpenTK.Mathematics.Vector3 TransformNormal(Matrix3 matrix, OpenTK.Mathematics.Vector3 vector)
        {
            return new OpenTK.Mathematics.Vector3(
                matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z,
                matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z,
                matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z
            );
        }




        static GameObject FindGOByName(GameObject root, string name)
        {
            if (root.Name == name)
                return root;

            foreach (var child in root.children)
            {
                var found = FindGOByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static float ComputeLightRange(Light light, float threshold = 0.01f)
        {
            float Kc = light.AttenuationConstant;
            float Kl = light.AttenuationLinear;
            float Kq = light.AttenuationQuadratic;

            float intensityAtDistance = 1.0f / threshold;
            float c = Kc - intensityAtDistance;

            // Solve quadratic: Kq * d^2 + Kl * d + c = 0
            float discriminant = Kl * Kl - 4 * Kq * c;

            if (Kq == 0 || discriminant < 0)
                return 100.0f; // fallback if invalid or directional light

            float sqrtD = MathF.Sqrt(discriminant);
            float d1 = (-Kl + sqrtD) / (2 * Kq);
            float d2 = (-Kl - sqrtD) / (2 * Kq);

            float range = MathF.Max(d1, d2);
            return range > 0 ? range : 100.0f;
        }

        static void SetWrap(Assimp.TextureWrapMode U, Assimp.TextureWrapMode V, Texture texture)
        {
            if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Repeat, Core.TextureWrapMode.Repeat);
            }
            else if (U == Assimp.TextureWrapMode.Wrap && V == Assimp.TextureWrapMode.Clamp)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Repeat, Core.TextureWrapMode.Clamp);
            }
            else if (U == Assimp.TextureWrapMode.Clamp && V == Assimp.TextureWrapMode.Wrap)
            {
                texture.SetWrapMode(Core.TextureWrapMode.Clamp, Core.TextureWrapMode.Repeat);
            }
        }

        static Core.Texture CheckTextureExists(string path)
        {
            return Textures.TryGetValue(path, out var tex) ? tex : null;
        }

        static void AddToTextureDict(string path, Core.Texture texture)
        {
            if (!Textures.ContainsKey(path))
            {
                Textures.Add(path, texture);
            }
        }

        public static Matrix4 ToOpenTKMatrix(Assimp.Matrix4x4 matrix)
        {
            return new Matrix4(matrix.A1, matrix.A2, matrix.A3, matrix.A4,
                               matrix.B1, matrix.B2, matrix.B3, matrix.B4,
                               matrix.C1, matrix.C2, matrix.C3, matrix.C4,
                               matrix.D1, matrix.D2, matrix.D3, matrix.D4);
        }


        static string CorrectFilePath(string path, string basePath = null)
        {
            if (IsFullPath(path))
            {
                return path;
            }
            return GetAbsolutePath(basePath, path);
        }

        public static bool IsFullPath(string path)
        {
            return !String.IsNullOrWhiteSpace(path)
                && path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1
                && Path.IsPathRooted(path)
                && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
        }

        public static String GetAbsolutePath(String basePath, String path)
        {
            if (path == null)
                return null;
            if (basePath == null)
                basePath = Path.GetFullPath("."); // quick way of getting current working directory
            else
                basePath = GetAbsolutePath(null, basePath); // to be REALLY sure ;)
            String finalPath;
            // specific for windows paths starting on \ - they need the drive added to them.
            // I constructed this piece like this for possible Mono support.
            if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
            {
                if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                    finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
                else
                    finalPath = Path.Combine(basePath, path);
            }
            else
                finalPath = path;
            // resolves any internal "..\" to get the true full path.
            return Path.GetFullPath(finalPath);
        }
    }
}