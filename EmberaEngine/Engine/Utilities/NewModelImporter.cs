using EmberaEngine.Core;
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
            public Vector3 scale;
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
                    Assimp.PostProcessSteps.GenerateSmoothNormals |
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
                var node = ProcessNode(child, scene, spec, ref meshNodes, ref cameraNodes, ref lightNodes, scene.RootNode.Transform);
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

        static ModelNode ProcessNode(
            Assimp.Node node,
            Assimp.Scene scene,
            ModelLoaderSpecification spec,
            ref List<MeshNode> meshNodes,
            ref List<CameraNode> cameras,
            ref List<LightNode> lights,
            Assimp.Matrix4x4 parentTransform // Accumulated from root
        )
        {
            // Accumulate transformation
            Assimp.Matrix4x4 worldTransform = node.Transform * parentTransform;
            worldTransform.Decompose(out var scale, out var rot, out var pos);
            var rotation = ToEulerAngles(rot);

            ModelNode resultNode;

            // LIGHT NODE
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

            // CAMERA NODE
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

            // MESH NODE
            else if (node.MeshIndices.Count > 0)
            {
                // Only use first mesh per node for now — extendable if needed
                int meshIdx = node.MeshIndices[0];
                var assimpMesh = scene.Meshes[meshIdx];

                Guid materialId = Guid.Empty;
                if (assimpMesh.MaterialIndex >= 0 && assimpMesh.MaterialIndex < materials.Count)
                    materialId = materials[assimpMesh.MaterialIndex].Id;

                var meshNode = ProcessMesh(assimpMesh, materialId, spec.importScale);
                meshNode.name = node.Name;
                meshNode.position = new Vector3(pos.X, pos.Y, pos.Z) / 100;
                meshNode.rotation = new Vector3(rotation.X, rotation.Y, rotation.Z);
                meshNode.scale = new Vector3(scale.X, scale.Y, scale.Z);
                meshNode.nodeType = ModelNodeType.Mesh;
                meshNode.materialIndex = assimpMesh.MaterialIndex;

                meshNodes.Add(meshNode);
                resultNode = meshNode;
            }

            // EMPTY NODE
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

            // Recurse into children
            foreach (var child in node.Children)
            {
                var childNode = ProcessNode(child, scene, spec, ref meshNodes, ref cameras, ref lights, worldTransform);
                resultNode.children.Add(childNode);
            }

            return resultNode;
        }


        static MeshNode ProcessMesh(
            Assimp.Mesh mesh,
            Guid materialId,
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
                Vector3 position = Vector3.TransformPosition(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z), scaleMatrix);
                Vector3 normal = Vector3.Normalize(Vector3.TransformNormal(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z), Matrix4.Transpose(Matrix4.Invert(scaleMatrix))));

                Vector3 tangent = Vector3.Normalize(TransformNormal(normalMatrix, new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z)));

                Vector3 bitangent = Vector3.Normalize(TransformNormal(normalMatrix, new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z)));

                Vector2 uv = Vector2.Zero;
                if (mesh.TextureCoordinateChannels[0].Count > 0)
                {
                    uv = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);
                }

                vertices.Add(new Vertex(position, normal, uv, tangent, bitangent));
            }

            Mesh devoidMesh = new Mesh
            {
                Name = mesh.Name,
                Id = Guid.NewGuid(),
                MaterialReference = materialId
            };
            devoidMesh.SetVertices(vertices.ToArray());
            if (indices.Length > 0)
                devoidMesh.SetIndices(indices);

            return new MeshNode() { mesh = devoidMesh };
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
            string relativePath = Path.GetRelativePath(AppContext.BaseDirectory, fullPath);
            Guid textureId = AssetLookup.GetFileGuidByPath(relativePath);

            var textureRef = (TextureReference)AssetLoader.Load<Texture>(fullPath);
            if (textureRef == null)
                return;

            Action<Texture> setupAndAssign = tex =>
            {
                tex.Id = textureId;
                SetupTexture(tex, texSlot, setTexture);
            };

            if (textureRef.isLoaded)
            {
                setupAndAssign(textureRef.value);
            }
            else
            {
                textureRef.OnLoad += setupAndAssign;

                var nullTex = Helper.loadImageAsTex("Engine/Content/Textures/Placeholders/null.png");
                nullTex.Id = textureId;
                SetupTexture(nullTex, texSlot, setTexture);
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
