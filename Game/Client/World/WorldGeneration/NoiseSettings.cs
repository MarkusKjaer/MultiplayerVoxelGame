using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class NoiseSettings
    {
        public float noiseZoom;
        public int octaves;
        public Vector2i offest;
        public Vector2i worldOffset;
        public float persistance;
        public float redistributionModifier;
        public float exponent;
        public int seed;
    }
}
