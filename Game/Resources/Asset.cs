namespace MultiplayerVoxelGame.Game.Resources
{
    public class Asset
    {
        public string Name { get; private set; }
        public string FilePath { get; private set; }
        public AssetType Type { get; private set; }

        public Asset(string name, string filePath, AssetType assetType)
        {
            Name = name;
            FilePath = filePath;
            Type = assetType;
        }
    }
}
