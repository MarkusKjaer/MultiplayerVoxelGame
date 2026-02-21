using CubeEngine.Engine.Client.World;
using CubeEngine.Util;
using MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers;
using MultiplayerVoxelGame.Util;
using NoiseDotNet;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class BiomeGenerator
    {
        public int waterThreshold = 50;

        public NoiseSettings biomeNoiseSettings;

        public DomainWarping domainWarping;

        public bool useDomainWarping = true;

        public VoxelHandler startLayerHandler;

        public List<VoxelHandler> additionalLayerHandlers;

        public BiomeGenerator(NoiseSettings biomeNoiseSettings, DomainWarping domainWarping, VoxelHandler startLayerHandler, List<VoxelHandler> additionalLayerHandlers)
        {
            this.biomeNoiseSettings = biomeNoiseSettings;
            this.domainWarping = domainWarping;
            this.startLayerHandler = startLayerHandler;
            this.additionalLayerHandlers = additionalLayerHandlers;
        }

        //public ChunkData ProcessChunkColumn(ChunkData data, int x, int z, Vector2i mapSeedOffset)
        //{
        //    biomeNoiseSettings.worldOffset = mapSeedOffset;
        //    int groundPosition = GetSurfaceHeightNoise((int)(data.Position.X + x), (int)(data.Position.Y + z), data.SizeY);

        //    for (float y = data.Position.Y; y < data.Position.Y + data.SizeY; y++)
        //    {
        //        var voxelGenerationContext = new VoxelGenerationContext(data, x, (int)y, z, groundPosition, mapSeedOffset);

        //        startLayerHandler.Handle(voxelGenerationContext);
        //    }

        //    foreach (var layer in additionalLayerHandlers)
        //    {
        //        var voxelGenerationContext = new VoxelGenerationContext(data, x, (int)data.Position.Y, z, groundPosition, mapSeedOffset);

        //        layer.Handle(voxelGenerationContext);
        //    }

        //    return data;
        //}

        public ChunkData ProcessWholeChunk(ChunkData data, Vector2i mapSeedOffset)
        {
            int sizeX = data.SizeX;
            int sizeZ = data.SizeZ;
            int totalPoints = sizeX * sizeZ;

            float[] xCoords = new float[totalPoints];
            float[] zCoords = new float[totalPoints];
            float[] noiseOutput = new float[totalPoints];

            int i = 0;
            for (int z = 0; z < sizeZ; z++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    xCoords[i] = data.Position.X + x;
                    zCoords[i] = data.Position.Y + z; 
                    i++;
                }
            }

            if (!useDomainWarping)
            {
                Noise.GradientNoise2D(xCoords, zCoords, noiseOutput,
                    biomeNoiseSettings.noiseZoom, biomeNoiseSettings.noiseZoom, 1f, 100);
            }
            else
            {
                noiseOutput = domainWarping.GenerateDomainNoiseBatch(xCoords, zCoords, biomeNoiseSettings);
            }

            for (int k = 0; k < noiseOutput.Length; k++)
            {
                noiseOutput[k] = (noiseOutput[k] + 1f) * 0.5f;
            }

            i = 0;
            for (int z = 0; z < sizeZ; z++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    float terrainHeight = noiseOutput[i];
                    terrainHeight = MyNoise.Redistribution(terrainHeight, biomeNoiseSettings);
                    int groundPosition = MyNoise.RemapValue01ToInt(terrainHeight, 0, data.SizeY);

                    for (int y = 0; y < data.SizeY; y++)
                    {
                        var ctx = new VoxelGenerationContext(data, x, y, z, groundPosition, mapSeedOffset);
                        startLayerHandler.Handle(ctx);
                    }

                    var layerCtx = new VoxelGenerationContext(data, x, (int)data.Position.Y, z, groundPosition, mapSeedOffset);
                    foreach (var layer in additionalLayerHandlers)
                    {
                        layer.Handle(layerCtx);
                    }
                    i++;
                }
            }
            return data;
        }

        //private int GetSurfaceHeightNoise(int x, int z, int chunkHeight)
        //{
        //    float terrainHeight;
        //    if (useDomainWarping == false)
        //    {
        //        terrainHeight = MyNoise.OctavePerlin(x, z, biomeNoiseSettings);
        //    }
        //    else
        //    {
        //        terrainHeight = domainWarping.GenerateDomainNoise(x, z, biomeNoiseSettings);
        //    }

        //    terrainHeight = MyNoise.Redistribution(terrainHeight, biomeNoiseSettings);
        //    int surfaceHeight = MyNoise.RemapValue01ToInt(terrainHeight, 0, chunkHeight);
        //    return surfaceHeight;
        //}
    }
}
