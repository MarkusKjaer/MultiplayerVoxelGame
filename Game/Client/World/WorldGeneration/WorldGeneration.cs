using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Util;
using MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers;
using MultiplayerVoxelGame.Util.Settings;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class WorldGeneration
    {
        private readonly int _seed;

        private DomainWarping domainWarping;

        #region Biome Settings

        private BiomeGenerator plainsBiomeGenerator;

        private NoiseSettings StoneLayerNoise = new()
        {
            noiseZoom = 0.01f,
            octaves = 1,
            offest = new Vector2i(-3200, 100),
            worldOffset = new Vector2i(2000, 0),
            persistance = 0.4f,
            redistributionModifier = 1.2f,
            exponent = 5f
        };

        private NoiseSettings WorldNoise = new()
        {
            noiseZoom = 0.005f,
            octaves = 3,
            offest = new Vector2i(-400, 3400),
            worldOffset = new Vector2i(2000, 0),
            persistance = 0.75f,
            redistributionModifier = 1.2f,
            exponent = 5f
        };

        private NoiseSettings NoiseSettingsDomainX = new()
        {
            noiseZoom = 0.01f,
            octaves = 3,
            offest = new Vector2i(600, 350),
            worldOffset = new Vector2i(0, 0),
            persistance = 0.5f,
            redistributionModifier = 1.2f,
            exponent = 5f
        };

        private NoiseSettings NoiseSettingsDomainY = new()
        {
            noiseZoom = 0.02f,
            octaves = 3,
            offest = new Vector2i(900, 1500),
            worldOffset = new Vector2i(0, 0),
            persistance = 0.5f,
            redistributionModifier = 1.2f,
            exponent = 5f
        };

        

        void InitBiomeGenerators()
        {
            domainWarping = new(
                NoiseSettingsDomainX,
                NoiseSettingsDomainY
            );

            UndergroundLayerHandler undergroundLayerHandler = new(null, VoxelType.Stone);
            SurfaceHandler surfaceHandler = new(VoxelType.Grass, undergroundLayerHandler);
            AirHandler airHandler = new(surfaceHandler);
            WaterHandler waterHandler = new(airHandler, ChunkSettings.WaterLevel);

            StoneLayerHandler stoneLayerHandler = new(StoneLayerNoise, domainWarping, null);

            var additionalHandlers = new List<VoxelHandler>()
            {
                stoneLayerHandler
            };

            

            plainsBiomeGenerator = new(
                WorldNoise,
                domainWarping,
                waterHandler,
                additionalHandlers
            );
        }


        #endregion

        public WorldGeneration(int seed)
        {
            _seed = seed;

            InitBiomeGenerators();
        }

        public WorldGeneration()
        {
            _seed = Guid.NewGuid().GetHashCode();

            InitBiomeGenerators();
        }

        public List<ChunkData> GenPartOfWorld(
            int chunkSize,
            int maxWorldHeight,
            List<Vector2> chunksToGenPosition)
        {
            var chunks = new List<ChunkData>();

            foreach (var pos in chunksToGenPosition)
            {
                chunks.Add(GenChunk(chunkSize, pos));
            }

            return chunks;
        }

        private ChunkData GenChunk(int chunkSize, Vector2 chunkIndex)
        {
            var chunkData = new ChunkData(chunkSize, ChunkSettings.Height, chunkSize, chunkIndex * chunkSize);
            Vector2i chunkInt = new Vector2i((int)chunkIndex.X, (int)chunkIndex.Y);

            return plainsBiomeGenerator.ProcessWholeChunk(chunkData, chunkInt);
        }
    }
}