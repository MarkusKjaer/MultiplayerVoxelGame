using CubeEngine.Engine.Window;

namespace CubeEngine.Engine.MeshObject
{
    public class Material(string vertShaderFileLocation, string fragShaderFileLocation, string textureLocation)
    {
        public string VertShaderFileLocation { get; private set; } = vertShaderFileLocation;
        public string FragShaderFileLocation { get; private set; } = fragShaderFileLocation;

        public string TextureLocation { get; private set; } = textureLocation;
    }
}
