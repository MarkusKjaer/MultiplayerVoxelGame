using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Util;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.Mesh
{
    public class WaterChunkMesh : BaseMesh<WaterChunkMeshInfo>
    {
        public WaterChunkMesh(WaterChunkMeshInfo meshInfo, Material material)
            : base(meshInfo, material)
        {
            string vert = File.ReadAllText(material.VertShaderFileLocation);
            string frag = File.ReadAllText(material.FragShaderFileLocation);
            shaderProgram = new(vert, frag);

            UpdateMesh(meshInfo);
        }

        public void UpdateMesh(WaterChunkMeshInfo meshInfo)
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();

            _meshInfo = meshInfo;

            if (_meshInfo.IndexCount == 0)
                return;

            vertexBuffer = new VertexBuffer(
                VertexPositionNormalTexture.vertexInfo,
                _meshInfo.VertexCount,
                true);

            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);
        }

        public override void Render()
        {
            if (_meshInfo.IndexCount == 0)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.DepthMask(false);
            GL.DepthFunc(DepthFunction.Less); 

            base.Render(); 

            GL.BindTexture(TextureTarget.Texture2D, _material.TextureManager.TextureID);
            GL.BindVertexArray(vertexArray.VertexArrayHandle);
            GL.DrawElements(PrimitiveType.Triangles, _meshInfo.IndexCount, DrawElementsType.UnsignedInt, 0);

            GL.DepthMask(true);
            GL.DepthFunc(DepthFunction.Less); 
            GL.Disable(EnableCap.Blend);
        }

        public override void Update(Camera camera, int windowWidth, int windowHeight)
        {
            // Lighting & Camera Uniforms
            shaderProgram.SetUniform("lightPos", new Vector3(1000f, 500f, 2000f));
            shaderProgram.SetUniform("viewPos", camera.Position);
            shaderProgram.SetUniform("time", (float)Time.GlobalTime);

            // Color Uniforms
            shaderProgram.SetUniform("lightColor", new Vector3(1.0f, 1.0f, 1.0f)); // White light
            shaderProgram.SetUniform("ambient", new Vector3(0.2f, 0.2f, 0.3f));    // Deep blue ambient
            shaderProgram.SetUniform("waterColor", new Vector3(0.0f, 0.5f, 0.7f));  // Tropical blue tint

            base.Update(camera, windowWidth, windowHeight);
        }
    }

    public readonly struct WaterChunkMeshInfo
    {
        public static readonly WaterChunkMeshInfo Empty =
            new WaterChunkMeshInfo(
                Array.Empty<VertexPositionNormalTexture>(),
                Array.Empty<int>()
            );

        public readonly VertexPositionNormalTexture[] Vertices;
        public readonly int[] Indices;
        public readonly int VertexCount;
        public readonly int IndexCount;

        public WaterChunkMeshInfo(
            VertexPositionNormalTexture[] vertices,
            int[] indices)
        {
            Vertices = vertices;
            Indices = indices;
            VertexCount = vertices.Length;
            IndexCount = indices.Length;
        }
    }
}
