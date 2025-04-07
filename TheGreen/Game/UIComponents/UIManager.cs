using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace TheGreen.Game.UIComponents
{
    /// <summary>
    /// updating and drawing for all UIComponent Containers.
    /// </summary>
    public static class UIManager
    {
        private static List<UIComponentContainer> _uiComponentContainers = new List<UIComponentContainer>();

        public static void Update(double delta)
        {
            _uiComponentContainers.First()?.Update(delta);
        }
        public static void Draw(SpriteBatch spriteBatch)
        {
            _uiComponentContainers.First()?.Draw(spriteBatch);
        }

        public static void RegisterContainer(UIComponentContainer container)
        {
            _uiComponentContainers.Insert(0, container);
        }

        public static void UnregisterContainer(UIComponentContainer container)
        {
            _uiComponentContainers.Remove(container);
        }
    }
}
