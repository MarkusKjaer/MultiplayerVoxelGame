using MultiplayerVoxelGame.Game.Resources;
using StbImageSharp;

namespace CubeEngine.Util
{
    public static class Noise
    {
        private static ImageResult? _heightmap;

        public static void LoadHeightmap()
        {
            using var stream = File.OpenRead(AssetsManager.Instance.LoadedAssets[("Perlin", AssetType.PNG)].FilePath);
            _heightmap = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
        }

        public static float ImageHeight(float x, float y)
        {
            if (_heightmap == null)
                throw new InvalidOperationException("Heightmap not loaded. Call LoadHeightmap first.");

            int width = _heightmap.Width;
            int height = _heightmap.Height;

            float fx = (x % width + width) % width;
            float fy = (y % height + height) % height;

            int x0 = (int)MathF.Floor(fx) % width;
            int x1 = (x0 + 1) % width;
            int y0 = (int)MathF.Floor(fy) % height;
            int y1 = (y0 + 1) % height;

            float tx = fx - x0;
            float ty = fy - y0;

            float SamplePixel(int px, int py)
            {
                int index = (py * width + px) * 3; // RGB
                byte r = _heightmap.Data[index];
                byte g = _heightmap.Data[index + 1];
                byte b = _heightmap.Data[index + 2];
                return (r + g + b) / (3f * 255f);
            }

            float c00 = SamplePixel(x0, y0);
            float c10 = SamplePixel(x1, y0);
            float c01 = SamplePixel(x0, y1);
            float c11 = SamplePixel(x1, y1);

            float c0 = c00 * (1 - tx) + c10 * tx;
            float c1 = c01 * (1 - tx) + c11 * tx;
            float c = c0 * (1 - ty) + c1 * ty;

            return c;
        }

    }
}