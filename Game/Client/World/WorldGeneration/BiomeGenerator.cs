using CubeEngine.Engine.Client.World;
using MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers;
using MultiplayerVoxelGame.Util;
using MultiplayerVoxelGame.Util.Settings;
using NoiseDotNet;
using OpenTK.Mathematics;
using System.Buffers;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class BiomeGenerator
    {
        public NoiseSettings biomeNoiseSettings;

        public DomainWarping domainWarping;

        public bool useDomainWarping = true;

        public VoxelHandler startLayerHandler;

        public List<VoxelHandler> additionalLayerHandlers;
        private int _groundLevel = 50;

        public BiomeGenerator(NoiseSettings biomeNoiseSettings, DomainWarping domainWarping, VoxelHandler startLayerHandler, List<VoxelHandler> additionalLayerHandlers, int groundLevel = 50)
        {
            this.biomeNoiseSettings = biomeNoiseSettings;
            this.domainWarping = domainWarping;
            this.startLayerHandler = startLayerHandler;
            this.additionalLayerHandlers = additionalLayerHandlers;
            this._groundLevel = groundLevel;
        }

        public ChunkData ProcessWholeChunk(ChunkData data, Vector2i mapSeedOffset)
        {
            int sizeX = data.SizeX;
            int sizeY = data.SizeY;
            int sizeZ = data.SizeZ;

            int total2DPoints = sizeX * sizeZ;
            int total3DPoints = sizeX * sizeY * sizeZ;

            float[] xCoords2DArray = ArrayPool<float>.Shared.Rent(total2DPoints);
            float[] zCoords2DArray = ArrayPool<float>.Shared.Rent(total2DPoints);
            float[] noiseOutput2DArray = ArrayPool<float>.Shared.Rent(total2DPoints);

            float[] xCoords3DArray = ArrayPool<float>.Shared.Rent(total3DPoints);
            float[] yCoords3DArray = ArrayPool<float>.Shared.Rent(total3DPoints);
            float[] zCoords3DArray = ArrayPool<float>.Shared.Rent(total3DPoints);
            float[] noiseOutput3DArray = ArrayPool<float>.Shared.Rent(total3DPoints);

            try
            {
                Span<float> xCoords2D = xCoords2DArray.AsSpan(0, total2DPoints);
                Span<float> zCoords2D = zCoords2DArray.AsSpan(0, total2DPoints);
                Span<float> noiseOutput2D = noiseOutput2DArray.AsSpan(0, total2DPoints);

                Span<float> xCoords3D = xCoords3DArray.AsSpan(0, total3DPoints);
                Span<float> yCoords3D = yCoords3DArray.AsSpan(0, total3DPoints);
                Span<float> zCoords3D = zCoords3DArray.AsSpan(0, total3DPoints);
                Span<float> noiseOutput3D = noiseOutput3DArray.AsSpan(0, total3DPoints);

                int i2D_setup = 0;
                for (int z = 0; z < sizeZ; z++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        xCoords2D[i2D_setup] = data.Position.X + x;
                        zCoords2D[i2D_setup] = data.Position.Y + z;
                        i2D_setup++;
                    }
                }

                noiseOutput2D.Clear();

                if (!useDomainWarping)
                {
                    Noise.GradientNoise2D(xCoords2D.ToArray(), zCoords2D.ToArray(), noiseOutput2D.ToArray(),
                        biomeNoiseSettings.noiseZoom, biomeNoiseSettings.noiseZoom, 1f, 100);
                }
                else
                {
                    var temp2D = domainWarping.GenerateDomainNoiseBatch(xCoords2D.ToArray(), zCoords2D.ToArray(), biomeNoiseSettings);
                    temp2D.CopyTo(noiseOutput2D);
                }

                for (int k = 0; k < noiseOutput2D.Length; k++)
                {
                    noiseOutput2D[k] = (noiseOutput2D[k] + 1f) * 0.5f;
                }

                int i3D = 0;
                for (int z = 0; z < sizeZ; z++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            xCoords3D[i3D] = data.Position.X + x;
                            yCoords3D[i3D] = y;
                            zCoords3D[i3D] = data.Position.Y + z;
                            i3D++;
                        }
                    }
                }

                MyNoise.OctaveGradientNoise3D(xCoords3D, yCoords3D, zCoords3D, noiseOutput3D, biomeNoiseSettings, 12345);

                int i2D = 0;
                i3D = 0;

                for (int z = 0; z < sizeZ; z++)
                {
                    for (int x = 0; x < sizeX; x++)
                    {
                        float terrainHeight = noiseOutput2D[i2D];
                        terrainHeight = MyNoise.Redistribution(terrainHeight, biomeNoiseSettings);
                        float baseHeightFloat = MyNoise.RemapValue01(terrainHeight, _groundLevel, data.SizeY - 10);
                        int groundPosition = (int)baseHeightFloat;

                        for (int y = 0; y < sizeY; y++)
                        {
                            float noise3DValue = noiseOutput3D[i3D];
                            float density = (baseHeightFloat - y) + (noise3DValue * ChunkSettings.CrazyValue);

                            float densityAbove;
                            if (y < sizeY - 1)
                            {
                                float noiseAboveValue = noiseOutput3D[i3D + 1];

                                densityAbove = (baseHeightFloat - (y + 1)) + (noiseAboveValue * ChunkSettings.CrazyValue);
                            }
                            else
                            {
                                densityAbove = -1f;
                            }
                            var ctx = new VoxelGenerationContext(data, x, y, z, groundPosition, mapSeedOffset, density, densityAbove);
                            startLayerHandler.Handle(ctx);

                            i3D++;
                        }

                        i2D++;
                    }
                }
                return data;
            }
            finally
            {
                ArrayPool<float>.Shared.Return(xCoords2DArray);
                ArrayPool<float>.Shared.Return(zCoords2DArray);
                ArrayPool<float>.Shared.Return(noiseOutput2DArray);

                ArrayPool<float>.Shared.Return(xCoords3DArray);
                ArrayPool<float>.Shared.Return(yCoords3DArray);
                ArrayPool<float>.Shared.Return(zCoords3DArray);
                ArrayPool<float>.Shared.Return(noiseOutput3DArray);
            }
        }
    }
}
