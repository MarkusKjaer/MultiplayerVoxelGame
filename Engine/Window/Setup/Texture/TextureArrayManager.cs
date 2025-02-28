using OpenTK.Graphics.OpenGL;
using StbImageSharp;
using System.IO;

namespace CubeEngine.Engine.Window.Setup.Texture
{
    public class TextureArrayManager : ITexture
    {
        private bool disposed;

        public int TextureID { get; set; }

        public TextureArrayManager(string[] filepaths, int width, int height)
        {
            int texture;
            GL.GenTextures(1, out texture);
            TextureID = texture;

            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, 1, 1, filepaths.Length);

            for (int i = 0; i < filepaths.Length; i++)
            {
                using (FileStream stream = File.OpenRead(filepaths[i]))
                {
                    ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                    GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, i, width, height, 1,
                        PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
                }
            }
            
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        ~TextureArrayManager()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                return;
            }

            GL.BindTexture(TextureTarget.Texture2DArray, TextureID);
            GL.DeleteTexture(TextureID);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

    }
}
