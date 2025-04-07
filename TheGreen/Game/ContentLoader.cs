using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace TheGreen.Game
{
    static class ContentLoader
    {
        public static Texture2D PlayerTexture;
        public static Texture2D[] TileTextures;
        public static Texture2D[] ItemTextures;
        public static Texture2D[] EnemyTextures;
        public static Texture2D ItemSlotTexture;
        public static SpriteFont GameFont;
        public static Texture2D Cracks;
        public static Texture2D TreesBackground;
        public static Texture2D TreesFartherBackground;
        public static Texture2D TreesFarthestBackground;
        public static Texture2D MountainsBackground;
        public static Texture2D LiquidTexture;

        public static void Load(ContentManager content)
        {
            TileTextures = new Texture2D[200];
            ItemTextures = new Texture2D[200];
            EnemyTextures = new Texture2D[200];
            PlayerTexture = content.Load<Texture2D>("Assets/Textures/Player/Player");
            ItemSlotTexture = content.Load<Texture2D>("Assets/Textures/UIComponents/ItemSlot");
            GameFont = content.Load<SpriteFont>("Assets/Fonts/File");
            Cracks = content.Load<Texture2D>("Assets/Textures/Tiles/Extras/Cracks");
            TreesBackground = content.Load<Texture2D>(Globals.NativeResolution.X == 1280 ? "Assets/Textures/Backgrounds/Upscaled/TreesUpscaled" : "Assets/Textures/Backgrounds/Normal/Trees");
            TreesFartherBackground = content.Load<Texture2D>(Globals.NativeResolution.X == 1280 ? "Assets/Textures/Backgrounds/Upscaled/TreesFartherUpscaled" : "Assets/Textures/Backgrounds/Normal/TreesFarther");
            TreesFarthestBackground = content.Load<Texture2D>(Globals.NativeResolution.X == 1280 ? "Assets/Textures/Backgrounds/Upscaled/TreesFarthestUpscaled" : "Assets/Textures/Backgrounds/Normal/TreesFarthest");
            MountainsBackground = content.Load<Texture2D>("Assets/Textures/Backgrounds/Normal/Mountains");
            LiquidTexture = content.Load<Texture2D>("Assets/Textures/Tiles/Liquids/Liquid0");

            //load tile textures into an array
            int numTiles = Directory.GetFiles(content.RootDirectory + "/Assets/Textures/Tiles").Length;
            for (int i = 1; i <= numTiles; i++)
            {
                TileTextures[i] = content.Load<Texture2D>("Assets/Textures/Tiles/Tile" + i);
            }

            int numItems = Directory.GetFiles(content.RootDirectory + "/Assets/Textures/Items").Length;
            for (int i = 0; i < numItems; i++)
            {
                ItemTextures[i] = content.Load<Texture2D>("Assets/Textures/Items/Item" + i);
            }

            int numEnemies = Directory.GetFiles(content.RootDirectory + "/Assets/Textures/Enemies").Length;
            for (int i = 0; i < numEnemies; i++)
            {
                EnemyTextures[i] = content.Load<Texture2D>("Assets/Textures/Enemies/Enemy" + i);
            }
        }
    }
}
