using System.Collections.Generic;
using TheGreen.Game.Items.WeaponBehaviors;
using TheGreen.Game.Tiles;

namespace TheGreen.Game.Items
{
    /// <summary>
    /// Holds information about items.
    /// </summary>
    public static class ItemDatabase
    {
        private static Dictionary<int, Item> _items = new Dictionary<int, Item>
        {
            {0, new TileItem(0, "Dirt", "Keep your hands off my dirt!", ContentLoader.ItemTextures[0], 1) },
            {1, new TileItem(1, "Stone", "Hard as a rock.", ContentLoader.ItemTextures[1], 3) },
            {2, new WeaponItem(2, "Basic Pickaxe", "Go and break something.", ContentLoader.ItemTextures[2], false, 0.2f, true, true, 20, 2, UseStyle.Swing, new Pickaxe(30)) }, //0.2f
            {3, new TileItem(3, "Torch", "Light it up!", ContentLoader.ItemTextures[3], 7) },
            {4, new LiquidItem(4, "Water Bucket", "It's a little wet.", ContentLoader.ItemTextures[4], 1) },
            {5, new TileItem(5, "Chest", "For storing shiny things!", ContentLoader.ItemTextures[5], 8) },
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Item object with the specified ID</returns>
        public static Item InstantiateItemByID(int id, int quantity = 1)
        {
            Item item = CloneItem(_items[id]);
            item.Quantity = quantity;
            return item;
        }

        private static Item CloneItem(Item item)
        {
            return item switch
            {
                TileItem tileItem => new TileItem(tileItem.ID, tileItem.Name, tileItem.Description, tileItem.Image, tileItem.TileID),
                WeaponItem weapon => new WeaponItem(weapon.ID, weapon.Name, weapon.Description, weapon.Image, weapon.Stackable, weapon.UseSpeed, weapon.AutoUse, weapon.SpriteDoesDamage, weapon.Damage, weapon.Knockback, useStyle: weapon.UseStyle, weaponBehavior: weapon.WeaponBehavior),
                LiquidItem liquid => new LiquidItem(liquid.ID, liquid.Name, liquid.Description, liquid.Image, liquid.LiquidID),
                _ => null
            };
        }

        public static Item InstantiateItemByTileID(ushort tileID, int quantity = 1)
        {
            int itemID = TileDatabase.GetTileData(tileID).ItemID;
            return itemID == -1 ? null : InstantiateItemByID(itemID);
        }
    }
}
