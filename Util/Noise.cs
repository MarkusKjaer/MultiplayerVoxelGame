using StbImageSharp;

namespace CubeEngine.Util
{
    public static class Noise
    {
        private static ImageResult? _heightmap;

        public static void LoadHeightmap(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Heightmap image not found: {path}");

            using var stream = File.OpenRead(path);
            _heightmap = ImageResult.FromStream(stream, ColorComponents.RedGreenBlue);
        }

        public static float ImageHeight(float x, float y)
        {
            if (_heightmap == null)
                throw new InvalidOperationException("Heightmap not loaded. Call LoadHeightmap first.");

            int px = ((int)x % _heightmap.Width + _heightmap.Width) % _heightmap.Width;
            int py = ((int)y % _heightmap.Height + _heightmap.Height) % _heightmap.Height;

            // Get pixel RGB
            int index = (py * _heightmap.Width + px) * 3; // 3 channels (RGB)
            byte r = _heightmap.Data[index];
            byte g = _heightmap.Data[index + 1];
            byte b = _heightmap.Data[index + 2];

            // Convert to grayscale (0..1)
            return (r + g + b) / (3f * 255f);
        }
    }
}