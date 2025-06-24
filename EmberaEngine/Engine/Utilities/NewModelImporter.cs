using EmberaEngine.Engine.AssetHandling;
using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using OpenTK.Mathematics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Utilities
{

    public struct ModelLoaderSpecification
    {
        public string resourcePath;

        public float importScale;

    }

    public class NewModelImporter
    {
        public enum ModelNodeType
        {
            Empty,
            Mesh,
            Light,
            Camera
        }

        public struct ModelGraphData
        {
            public ModelNode rootNode;
            public List<MeshNode> meshNodes;
            public List<CameraNode> cameraNodes;
            public List<LightNode> lightNodes;
            public List<Material> materials;
        }

        public abstract class ModelNode
        {
            public string name;
            public Vector3 position;
            public Vector3 rotation;
            public ModelNodeType nodeType;
            public List<ModelNode> children = new();
        }

        public class EmptyNode : ModelNode
        {
            
        }

        public class LightNode : ModelNode
        {
            public LightType lightType;
            public Color4 colorDiffuse;
            public float intensity;
            public float radius;
        }

        public class CameraNode : ModelNode
        {
            public float fovy;
            public float near;
            public float far;
            
        }

        public class MeshNode : ModelNode
        {
            public Mesh mesh;
            public int materialIndex;
        }

        static List<Material> materials = new List<Material>();

        public static Action<Mesh, Material> OnMeshLinkCreatedCallback;


        public static ModelGraphData Load(ModelLoaderSpecification spec)
        {
            if (!VirtualFileSystem.Exists(spec.resourcePath))
            {
                Console.WriteLine("[MODEL IMPORTER]: The model file does not exist at the specified location!");
                return default;
            }

            string diskPath = VirtualFileSystem.ResolvePath(spec.resourcePath);

            Assimp.AssimpContext importer = new Assimp.AssimpContext();
            Assimp.Scene scene;

            try
            {
                scene = importer.ImportFile(diskPath,
                    Assimp.PostProcessSteps.Triangulate |
                    Assimp.PostProcessSteps.GenerateNormals |
                    Assimp.PostProcessSteps.CalculateTangentSpace |
                    Assimp.PostProcessSteps.FlipUVs |
                    Assimp.PostProcessSteps.GenerateUVCoords |
                    Assimp.PostProcessSteps.OptimizeMeshes
                );
            }
            catch ( Exception ex )
            {
                Console.WriteLine($"[Model Importer]: Failed to load model at \"{spec.resourcePath}\"\nError: {ex}");
                return default;
            }


            if (scene?.RootNode == null) 
                return default;

            var meshNodes = new List<MeshNode>();
            var cameraNodes = new List<CameraNode>();
            var lightNodes = new List<LightNode>();
            materials = ProcessMaterials(scene, Path.GetDirectoryName(spec.resourcePath));

            EmptyNode RootNode = new EmptyNode() { nodeType = ModelNodeType.Empty };
            foreach (var child in scene.RootNode.Children)
            {
                var node = ProcessNode(child, scene, spec, ref meshNodes, ref cameraNodes, ref lightNodes);
                RootNode.children.Add(node);
            }

            foreach (MeshNode meshnode in meshNodes)
            {
                
            }


            return new ModelGraphData()
            {
                cameraNodes = cameraNodes,
                lightNodes = lightNodes,
                materials = materials,
                meshNodes = meshNodes,
                rootNode = RootNode
            };
        }

        static ModelNode ProcessNode(Assimp.Node node, Assimp.Scene scene, ModelLoaderSpecification spec,
                                  ref List<MeshNode> meshNodes, ref List<CameraNode> cameras, ref List<LightNode> lights)
        {
            node.Transform.Decompose(out var scale, out var rot, out var pos);
            var rotation = ToEulerAngles(rot);

            ModelNode resultNode;

            if (scene.Lights.FirstOrDefault(l => l.Name == node.Name) is Assimp.Light light)
            {
                var lightNode = new LightNode()
                {
                    name = light.Name,
                    position = new Vector3(pos.X, pos.Y, pos.Z),
                    rotation = new Vector3(rotation.X, rotation.Y, rotation.Z),
                    nodeType = ModelNodeType.Light,
                    colorDiffuse = new Color4(light.ColorDiffuse.R, light.ColorDiffuse.G, light.ColorDiffuse.B, 1f),
                    intensity = light.AttenuationConstant,
                    radius = 10f,
                    lightType = light.LightType switch
                    {
                        Assimp.LightSourceType.Point => LightType.PointLight,
                        Assimp.LightSourceType.Spot => LightType.SpotLight,
                        Assimp.LightSourceType.Directional => LightType.DirectionalLight,
                        _ => LightType.PointLight
                    }
                };
                lights.Add(lightNode);
                resultNode = lightNode;
            }
            else if (scene.Cameras.FirstOrDefault(c => c.Name == node.Name) is Assimp.Camera camera)
            {
                var camNode = new CameraNode()
                {
                    name = camera.Name,
                    position = new Vector3(pos.X, pos.Y, pos.Z),
                    rotation = new Vector3(rotation.X, rotation.Y, rotation.Z),
                    nodeType = ModelNodeType.Camera,
                    fovy = camera.FieldOfview,
                    near = camera.ClipPlaneNear,
                    far = camera.ClipPlaneFar
                };
                cameras.Add(camNode);
                resultNode = camNode;
            }
            else if (node.MeshIndices.Count > 0)
            {
                int meshIdx = node.MeshIndices[0];
                var assimpMesh = scene.Meshes[meshIdx];
                var mesh = ProcessMesh(assimpMesh, spec.importScale);

                mesh.name = node.Name;
                mesh.position = new Vector3(pos.X, pos.Y, pos.Z);
                mesh.rotation = new Vector3(rotation.X, rotation.Y, rotation.Z);
                mesh.nodeType = ModelNodeType.Mesh;

                meshNodes.Add(mesh);
                resultNode = mesh;
            }
            else
            {
                resultNode = new EmptyNode()
                {
                    name = node.Name,
                    position = new Vector3(pos.X, pos.Y, pos.Z),
                    rotation = new Vector3(rotation.X, rotation.Y, rotation.Z),
                    nodeType = ModelNodeType.Empty
                };
            }

            foreach (var child in node.Children)
            {
                resultNode.children.Add(ProcessNode(child, scene, spec, ref meshNodes, ref cameras, ref lights));
            }

            return resultNode;
        }


        static MeshNode ProcessMesh(
            Assimp.Mesh mesh,
            float importScale = 1.0f,
            bool generateLightMapUVs = false
        )
        {
            List<Vertex> vertices = new List<Vertex>();
            int[] indices = mesh.GetIndices();

            Matrix4 scaleMatrix = Matrix4.CreateScale(importScale);

            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(scaleMatrix)));

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vertex;

                Vector3 vertexPosition = (new Vector4(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, 1) * scaleMatrix).Xyz;

                Vector3 originalNormal = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                Vector3 modifiedNormals = Vector3.Normalize(TransformNormal(normalMatrix, originalNormal));

                Vector3 modifiedTangents = Vector3.One;
                Vector3 modifiedBiTangents = Vector3.One;

                if (mesh.Tangents.Count > 0)
                {
                    // Transform tangent (w=0, same treatment as normal)
                    Vector3 originalTangent = new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);
                    modifiedTangents = Vector3.Normalize(TransformNormal(normalMatrix, originalTangent));
                }

                if (mesh.BiTangents.Count > 0)
                {
                    // Copy bitangent directly, or recompute in shader if using handedness
                    Vector3 originalBiTangent = new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z);
                    modifiedBiTangents = Vector3.Normalize(TransformNormal(normalMatrix, originalBiTangent));
                }

                Vector2 textureCoordinates = Vector2.Zero;

                if (mesh.TextureCoordinateChannels[0].Count > 0)
                {
                    textureCoordinates = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                }

                vertex = new Vertex(vertexPosition, modifiedNormals, textureCoordinates, modifiedTangents, modifiedBiTangents);
                vertices.Add(vertex);
            }

            Mesh devoidMesh = new Mesh();
            devoidMesh.SetVertices(vertices.ToArray());
            devoidMesh.Name = mesh.Name;
            devoidMesh.Id = Guid.NewGuid();
            devoidMesh.MaterialReference = materials[mesh.MaterialIndex].Id;
            if (indices.Length != 0)
                devoidMesh.SetIndices(indices);

            return new MeshNode()
            {
                mesh = devoidMesh
            };
        }


        static List<Material> ProcessMaterials(Assimp.Scene scene, string baseDir)
        {
            var list = new List<Material>();
            foreach (var assimpMat in scene.Materials)
            {
                var mat = SetupMaterial(assimpMat, baseDir);
                list.Add(mat);
            }
            return list;
        }

        static PBRMaterial SetupMaterial(Assimp.Material assimpMat, string baseDir)
        {
            var mat = new PBRMaterial();
            mat.SetDefaults();
            mat.Id = Guid.NewGuid();

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

            TrySetTexture(assimpMat, Assimp.TextureType.Diffuse, tex => mat.DiffuseTexture = tex, baseDir);
            TrySetTexture(assimpMat, Assimp.TextureType.Normals, tex => mat.NormalTexture = tex, baseDir);
            TrySetTexture(assimpMat, Assimp.TextureType.Shininess, tex => mat.RoughnessTexture = tex, baseDir);
            TrySetTexture(assimpMat, Assimp.TextureType.Emissive, tex => mat.EmissionTexture = tex, baseDir);

            return mat;
        }

        static void TrySetTexture(
            Assimp.Material mat,
            Assimp.TextureType type,
            Action<Texture> setTexture,
            string baseDir)
        {
            if (!mat.GetMaterialTexture(type, 0, out Assimp.TextureSlot texSlot))
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

        static void SetupTexture(Texture texture, Assimp.TextureSlot texSlot, Action<Texture> setTexture)
        {
            texture.SetFilter(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            texture.SetAnisotropy(8f);
            texture.GenerateMipmap();
            SetWrap(texSlot.WrapModeU, texSlot.WrapModeV, texture);

            setTexture(texture);
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

        public static Assimp.Vector3D ToEulerAngles(Assimp.Quaternion q)
        {
            // Convert quaternion to rotation matrix
            Assimp.Matrix3x3 mat = q.GetMatrix();

            float sy = MathF.Sqrt(mat.A1 * mat.A1 + mat.B1 * mat.B1);

            bool singular = sy < 1e-6f;

            float x, y, z; // Euler angles

            if (!singular)
            {
                x = MathF.Atan2(mat.C2, mat.C3); // Pitch
                y = MathF.Atan2(-mat.C1, sy);    // Yaw
                z = MathF.Atan2(mat.B1, mat.A1); // Roll
            }
            else
            {
                x = MathF.Atan2(-mat.B3, mat.B2);
                y = MathF.Atan2(-mat.C1, sy);
                z = 0;
            }

            return new Assimp.Vector3D(
                MathHelper.RadiansToDegrees(x),
                MathHelper.RadiansToDegrees(y),
                MathHelper.RadiansToDegrees(z)
            );
        }

        public static Vector3 TransformNormal(Matrix3 matrix, Vector3 vector)
        {
            return new Vector3(
                matrix.M11 * vector.X + matrix.M12 * vector.Y + matrix.M13 * vector.Z,
                matrix.M21 * vector.X + matrix.M22 * vector.Y + matrix.M23 * vector.Z,
                matrix.M31 * vector.X + matrix.M32 * vector.Y + matrix.M33 * vector.Z
            );
        }

        public static Matrix4 ToOpenTKMatrix(Assimp.Matrix4x4 matrix)
        {
            return new Matrix4(matrix.A1, matrix.A2, matrix.A3, matrix.A4,
                               matrix.B1, matrix.B2, matrix.B3, matrix.B4,
                               matrix.C1, matrix.C2, matrix.C3, matrix.C4,
                               matrix.D1, matrix.D2, matrix.D3, matrix.D4);
        }
    }
}
