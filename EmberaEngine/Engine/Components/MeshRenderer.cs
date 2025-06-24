using EmberaEngine.Engine.Core;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class MeshRenderer : Component
    {
        public override string Type => nameof(MeshRenderer);

        public Mesh[] meshes;

        public MeshRenderer()
        {
            meshes = new Mesh[0];
        }

        public override void OnStart()
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];
                Renderer3D.RegisterMesh(mesh);
            }
        }

        public override void OnUpdate(float dt)
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];
                mesh.WorldMatrix = gameObject.transform.GetWorldMatrix();
            }
        }

        public void SetMesh(Mesh mesh)
        {
            this.meshes = new Mesh[] { mesh };

            Renderer3D.RegisterMesh(mesh);
        }

        public void SetMeshes(Mesh[] meshes)
        {
            this.meshes = meshes;
            for (int i = 0;i < meshes.Length;i++)
            {
                Mesh mesh = meshes[i];
                Renderer3D.RegisterMesh(mesh);
            }
        }

        public void RemoveMeshes()
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];
                Renderer3D.RemoveMesh(mesh);
            }
        }

        public override void OnDestroy()
        {
            Console.WriteLine("Help!");
            if (meshes.Length != 0)
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    Console.WriteLine("Removing!");
                    Mesh mesh = meshes[i];
                    Renderer3D.RemoveMesh(mesh);
                }
            }
        }

    }
}
