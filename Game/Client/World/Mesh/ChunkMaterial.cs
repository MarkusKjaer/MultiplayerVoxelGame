using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;

namespace CubeEngine.Engine.Client.World.Mesh
{
    public class ChunkMaterial(string vertShaderFileLocation, string fragShaderFileLocation, ITexture textureArrayManager)
    {
        public string VertShaderFileLocation { get; private set; } = vertShaderFileLocation;
        public string FragShaderFileLocation { get; private set; } = fragShaderFileLocation;

        public ITexture TextureArrayManager { get; private set; } = textureArrayManager;
    }
}
