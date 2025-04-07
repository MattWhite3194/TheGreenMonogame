using Microsoft.Xna.Framework.Graphics;
using TheGreen.Game.Items.WeaponBehaviors;

namespace TheGreen.Game.Items
{
    public class WeaponItem : Item
    {
        private int _baseDamage;
        private int _baseKnockback;
        public int Damage { get { return _baseDamage; } }
        public int Knockback { get { return _baseKnockback; } }
        public bool SpriteDoesDamage;
        private IWeapon _weaponBehavior;
        public IWeapon WeaponBehavior { get { return _weaponBehavior; } }
        public WeaponItem(int id, string name, string description, Texture2D image, bool stackable, double useSpeed, bool autoUse, bool spriteDoesDamage, int baseDamage, int baseKnockback, UseStyle useStyle = UseStyle.Swing, IWeapon weaponBehavior = null) : base(id, name, description, image, stackable, useSpeed, autoUse, useStyle)
        {
            SpriteDoesDamage = spriteDoesDamage;
            _baseDamage = baseDamage;
            _baseKnockback = baseKnockback;
            _weaponBehavior = weaponBehavior;
        }
        public override bool UseItem()
        {
            return _weaponBehavior?.UseItem() ?? true;
        }
    }
}
