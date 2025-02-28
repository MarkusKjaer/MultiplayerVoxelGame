using CubeEngine.Engine.Window.Setup.Texture;

namespace CubeEngine.Engine.MeshObject
{
    public class Material(string vertShaderFileLocation, string fragShaderFileLocation, ITexture texture)
    {
        public string VertShaderFileLocation { get; private set; } = vertShaderFileLocation;
        public string FragShaderFileLocation { get; private set; } = fragShaderFileLocation;

        public ITexture TextureManager { get; private set; } = texture;
    }
}
