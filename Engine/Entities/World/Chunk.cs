using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;
using CubeEngine.Engine.Window.Setup;
using System.Collections.Generic;

namespace CubeEngine.Engine.Entities.World
{
    public class Chunk
    {
        private ChunkData _chunkData;
        private ChunkMesh _chunkMesh;

        private TextureArrayManager _textureArrayManager;

        public ChunkData ChunkData 
        { 
            get
            {
                return _chunkData;
            } 
            set
            {
                _chunkData = value;
                _chunkMesh.UpdateMesh(GenChunkMesh(_chunkData));
            }
        }
        
        public Chunk(ChunkData chunkData, TextureArrayManager textureArrayManager)
        {
            ChunkData = chunkData;
            _textureArrayManager = textureArrayManager;
        }

        private MeshInfo GenChunkMesh(ChunkData chunkData)
        {
            List<VertexPositionTexture> vertexPositions = new();
            List<int> indices = new();

            for (int i = 0; i < chunkData.Voxels.GetLength(0); i++)
            {
                for (int j = 0; j < chunkData.Voxels.GetLength(1); j++)
                {
                    for (int k = 0; k < chunkData.Voxels.GetLength(2); k++)
                    {
                        if (chunkData.Voxels[i, j, k].VoxelType == Enum.VoxelType.Empty) continue;

                        vertexPositions.Add(new VertexPositionTexture(

                    }
                }
            }


            return new MeshInfo(vertexPositions, indices);
        }
        
        public void Render()
        {

        }
    }
}
