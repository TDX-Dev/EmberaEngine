using EmberaEngine.Engine;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;

namespace EmberaEngine.Game.Scripts
{
    public class ChunkSpawner : Component
    {
        public override string Type => nameof(ChunkSpawner);

        public int chunksX = 30;
        public int chunksZ = 30;

        public override void OnStart()
        {
            for (int x = 0; x < chunksX; x++)
            {
                for (int z = 0; z < chunksZ; z++)
                {
                    SpawnChunk(x, z);
                }
            }
        }

        private void SpawnChunk(int chunkX, int chunkZ)
        {
            GameObject chunk = new GameObject();
            chunk.Name = "Chunk_" + chunkX + "_" + chunkZ;
            chunk.Scene = this.gameObject.Scene;

            // Position it based on chunk coordinates and chunk size
            chunk.transform.Position = new Vector3(chunkX * 16, 0, chunkZ * 16);

            // Add voxel component
            chunk.AddComponent<VoxelComponent>().OnStart();

            // Optionally, add the chunk to the scene
            this.gameObject.AddChild(chunk);
        }
    }
}
