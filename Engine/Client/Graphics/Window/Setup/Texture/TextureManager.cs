using OpenTK.Graphics.OpenGL;
using StbImageSharp;


namespace CubeEngine.Engine.Client.Graphics.Window.Setup.Texture
{
    public class TextureManager : ITexture, IDisposable
    {
        private bool disposed;

        public int TextureID { get; set; }

        public TextureManager(string textureFilePath)
        {
            //Texture
            TextureID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, TextureID);

            // texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult image = ImageResult.FromStream(File.OpenRead(textureFilePath), ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        ~TextureManager() 
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                return;
            }

            GL.BindTexture(TextureTarget.Texture2D, TextureID);
            GL.DeleteTexture(TextureID);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
