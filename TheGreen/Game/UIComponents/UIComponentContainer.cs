using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using TheGreen.Game.Input;

namespace TheGreen.Game.UIComponents
{
    /// <summary>
    /// A collection of UIComponents.
    /// Should be used to create menu sections or UIComponent groups.
    /// </summary>
    public class UIComponentContainer : IInputHandler
    {
        
        private static UIComponent _focusedUIComponent;
        protected GraphicsDevice _graphicsDevice;
        public int ComponentCount;
        public Vector2 Position;
        public Vector2 Size;
        private List<UIComponent> _uiComponents = new List<UIComponent>();

        public UIComponentContainer(Vector2 position = default, Vector2 size = default, GraphicsDevice graphicsDevice = null)
        {
            Position = position;
            Size = size;
            _graphicsDevice = graphicsDevice;
            ComponentCount = 0;
        }
        public virtual void HandleInput(InputEvent @event)
        {
            foreach (UIComponent component in _uiComponents)
            {
                if (InputManager.IsEventHandled(@event)) break;

                if (!component.IsVisible()) continue;

                if (component.IsFocused())
                {
                    component.OnGuiInput(@event);
                }
                else if (@event is MouseInputEvent)
                {
                    if (InputManager.GetMouseWindowBounds().Intersects(component.GetBounds()))
                    {
                        component.OnGuiInput(@event);
                    }
                }
            }
        }
        public virtual void Update(double delta)
        {

            foreach (UIComponent component in _uiComponents)
            {
                if (!component.IsVisible()) continue;

                if (InputManager.GetMouseWindowBounds().Intersects(component.GetBounds()))
                {
                    if (!component.MouseInside)
                    {
                        component.OnMouseEntered.Invoke();
                        component.MouseInside = true;
                    }
                }
                else if (component.MouseInside)
                {
                    component.OnMouseExited.Invoke();
                    component.MouseInside = false;
                }
                component.Update(delta);
            }
        }
        public virtual void Draw(SpriteBatch spritebatch)
        {
            foreach (UIComponent component in _uiComponents)
            {
                if (component.IsVisible())
                {
                    component.Draw(spritebatch);
                }
            }
        }
        public void AddUIComponent(UIComponent component)
        {
            component.Position = component.Position + Position;
            _uiComponents.Add(component);
            ComponentCount++;
        }
        public void RemoveUIComponent(UIComponent component)
        {
            _uiComponents.Remove(component);
            ComponentCount--;
        }
        public UIComponent GetUIComponent(int index)
        {
            return _uiComponents[index];
        }
        public void SetFocusedComponent(UIComponent component)
        {
            _focusedUIComponent = component;
        }

        public UIComponent GetFocusedComponent(UIComponent component) 
        { 
            return _focusedUIComponent; 
        }
        public virtual void Dereference()
        {
            InputManager.UnregisterHandler(this);
            UIManager.UnregisterContainer(this);
        }
    }
}
