using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeEngine.Engine.Mesh
{
    public class Mesh
    {
        public void Load()
        {

        }

        public void Unload()
        {

        }
    }

    public readonly struct MeshInfo
    {
        public readonly int vertexCount;
        public readonly int indexCount;
        public readonly VertexPositionTexture[] vertices;
        public readonly int[] indices;

        public MeshInfo(VertexPositionTexture[] vertexPositions, int[] indices)
        {
            vertices = vertexPositions;
            vertexCount = vertices.Length;

            this.indices = indices;
            indexCount = indices.Length;

        }
    }
}
