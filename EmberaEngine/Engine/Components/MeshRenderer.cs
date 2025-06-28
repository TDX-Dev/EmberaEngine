using EmberaEngine.Engine.Attributes;
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
    [ExecuteInPauseMode]
    public class MeshRenderer : Component
    {
        public override string Type => nameof(MeshRenderer);

        public Mesh mesh;

        [IgnoreMember]
        private MeshEntry entry;

        public MeshRenderer()
        {
            mesh = null;
            entry = null;
        }

        public override void OnStart()
        {
            if (mesh != null && entry == null)
            {
                entry = Renderer3D.RegisterMesh(mesh);
            }
        }

        public override void OnUpdate(float dt)
        {
            Console.WriteLine("MR");
            if (entry != null)
            {
                entry.Transform = gameObject.transform.GetWorldMatrix();
            }
        }

        public void SetMesh(Mesh mesh)
        {
            this.mesh = mesh;

            // Re-register the mesh if already added
            if (entry != null)
            {
                Renderer3D.RemoveMesh(entry);
            }

            entry = Renderer3D.RegisterMesh(mesh);
        }

        public void RemoveMesh()
        {
            if (entry != null)
            {
                Renderer3D.RemoveMesh(entry);
                entry = null;
            }

            mesh = null;
        }

        public override void OnDestroy()
        {
            if (entry != null)
            {
                Renderer3D.RemoveMesh(entry);
                entry = null;
            }
        }
    }

}
