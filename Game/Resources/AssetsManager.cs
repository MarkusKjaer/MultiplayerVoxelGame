namespace MultiplayerVoxelGame.Game.Resources
{
    public class AssetsManager
    {
        public static AssetsManager Instance => _instance;
        private static AssetsManager _instance;

        private string _resourcePath;
        public Dictionary<(string, AssetType), Asset> LoadedAssets = [];

        public AssetsManager(string resourcePath)
        {
            _instance = this;

            _resourcePath = resourcePath;
            LoadAllResources();
        }

        private void LoadAllResources()
        {
            if (!Directory.Exists(_resourcePath)) return;

            // Get all file paths from the root and subdirectories
            string[] fileEntries = Directory.GetFiles(_resourcePath, "*.*", SearchOption.AllDirectories);

            foreach (string filePath in fileEntries)
            {
                string extension = Path.GetExtension(filePath).ToLower();
                AssetType? type = GetResourceType(extension);

                if (type.HasValue)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);

                    LoadedAssets[(fileName, type.Value)] = new Asset(fileName, filePath, type.Value);
                }
            }
        }

        private AssetType? GetResourceType(string extension)
        {
            return extension switch
            {
                ".png" => AssetType.PNG,
                ".xml" => AssetType.XML,
                ".obj" => AssetType.OBJ,
                ".frag" => AssetType.FRAG,
                ".vert" => AssetType.VERT,
                _ => null 
            };
        }
    }
}