using Microsoft.Xna.Framework;

namespace TheGreen.Game.UIComponents
{
    public class Grid : UIComponentContainer
    {
        private int _cols, _margin;

        public Grid(int cols, int margin = 5, Vector2 position = default, Vector2 size = default) : base(position, size)
        {
            this._cols = cols;
            this._margin = margin;
        }
        
        public void AddGridItem(UIComponent component)
        {
            int i = ComponentCount % _cols;
            int j = ComponentCount / _cols;
            component.Position = new Vector2((_margin * i) + (component.Size.X * i), (_margin * j) + (component.Size.Y * j));
            this.AddUIComponent(component);
            i = ComponentCount % _cols;
            j = ComponentCount / _cols;
            this.Size = new Vector2((_margin * (_cols + 1)) + (component.Size.X * _cols), (_margin * (j + 1)) + (component.Size.Y * j));
        }
    }
}
