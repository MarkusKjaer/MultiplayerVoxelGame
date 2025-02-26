using OpenTK.Graphics.OpenGL;
using StbImageSharp;

namespace CubeEngine.Engine.Window.Setup
{
    public class TextureArrayManager
    {
        public int TextureID { get; private set; }

        public TextureArrayManager(string[] filepaths, int width, int height)
        {
            // Generate texture
            TextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);

            // Allocate storage (Assume all textures have the same width/height)
            int layers = filepaths.Length;
            if(layers == 0)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, 0); // Unbind
                return;
            }
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, layers);

            // Load textures into array

            // 0 is empty
            for (int i = 1; i < layers; i++)
            {
                ImageResult image = ImageResult.FromStream(File.OpenRead(filepaths[i]), ColorComponents.RedGreenBlueAlpha);

                GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, width, height, 1,
                    PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            // Set texture parameters
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.BindTexture(TextureTarget.Texture2DArray, 0); // Unbind
        }

    }
}
