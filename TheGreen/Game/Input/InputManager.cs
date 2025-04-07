using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheGreen.Game.Input
{
    public static class InputManager
    {
        //handles all game input and propogates it to input managers
        public static Dictionary<InputButton, Keys> KeyMappings = new Dictionary<InputButton, Keys>()
        {
            {InputButton.Up, Keys.W },
            {InputButton.Down, Keys.S },
            {InputButton.Left, Keys.A },
            {InputButton.Right, Keys.D},
            {InputButton.Jump, Keys.Space},
            {InputButton.Inventory, Keys.E}
        };
        private static List<IInputHandler> _inputHandlers = new();
        private static KeyboardState _previousKeyboardState = Keyboard.GetState();
        private static KeyboardState _currentKeyboardState = Keyboard.GetState();
        private static MouseState _previousMouseState;
        private static MouseState _currentMouseState;
        public static void RegisterHandler(IInputHandler handler)
        {
            if (_inputHandlers.Contains(handler))
            {
                _inputHandlers.Remove(handler);
            }
            _inputHandlers.Insert(0, handler);
        }

        public static void UnregisterHandler(IInputHandler handler)
        {
            if (_inputHandlers.Count > 0)
            {
                _inputHandlers.Remove(handler);
            }
        }

        public static void DispatchEvent(InputEvent @event)
        {
            //Handlers towards the beginning of the list have higher priority
            for (int i = 0; i < _inputHandlers.Count; i++)
            {
                if (i >= _inputHandlers.Count)
                    break;
                if (@event.handled)
                    break;
                _inputHandlers[i].HandleInput(@event);
            }
        }

        public static void MarkInputAsHandled(InputEvent @event)
        {
            @event.handled = true;
        }

        public static bool IsEventHandled(InputEvent @event) 
        { 
            return @event.handled; 
        }

        public static void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            //left mouse input
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                if (_previousMouseState.LeftButton != ButtonState.Pressed)
                {
                    DispatchEvent(new MouseInputEvent(InputEventType.MouseButtonDown, InputButton.LeftMouse));
                }
            }
            else if (_previousMouseState.LeftButton == ButtonState.Pressed)
            {
                DispatchEvent(new MouseInputEvent(InputEventType.MouseButtonUp, InputButton.LeftMouse));
            }

            //right mouse input
            if (_currentMouseState.RightButton == ButtonState.Pressed)
            {
                if (_previousMouseState.RightButton != ButtonState.Pressed)
                {
                    DispatchEvent(new MouseInputEvent(InputEventType.MouseButtonDown, InputButton.RightMouse));
                }
            }
            else if (_previousMouseState.RightButton == ButtonState.Pressed)
            {
                DispatchEvent(new MouseInputEvent(InputEventType.MouseButtonUp, InputButton.RightMouse));
            }

            //middle mouse input
            if (_currentMouseState.ScrollWheelValue < _previousMouseState.ScrollWheelValue)
            {
                DispatchEvent(new MouseInputEvent(InputEventType.MouseButtonUp, InputButton.MiddleMouse));
            }
            else if (_currentMouseState.ScrollWheelValue > _previousMouseState.ScrollWheelValue)
            {
                DispatchEvent(new MouseInputEvent(InputEventType.MouseButtonDown, InputButton.MiddleMouse));
            }


            foreach (var inputType in KeyMappings.Keys)
            {
                //if the key is down
                if (_currentKeyboardState.IsKeyDown(KeyMappings[inputType]))
                {
                    //if the key was not down last update send a key down event
                    if (!_previousKeyboardState.IsKeyDown(KeyMappings[inputType]))
                    {
                        DispatchEvent(new InputEvent(InputEventType.KeyDown, inputType));
                    }
                }
                //if the is not down and the key was down last update
                else if (_previousKeyboardState.IsKeyDown(KeyMappings[inputType]))
                {
                    DispatchEvent(new InputEvent(InputEventType.KeyUp, inputType));
                }
            }
        }
        public static Vector2 GetMouseWindowPosition()
        {
            return Vector2.Transform(Mouse.GetState().Position.ToVector2(), Matrix.Invert(TheGreen.UIScaleMatrix));
        }
        public static Point GetMouseWorldPosition()
        {
            Vector2 mousePosition = (Mouse.GetState().Position.ToVector2() - TheGreen.RenderDestination.Location.ToVector2()) * new Vector2(Globals.NativeResolution.X / (float)TheGreen.RenderDestination.Width) ;
            Point translation = Main.GetCameraPosition().ToPoint();
            return mousePosition.ToPoint() + translation;
        }
        public static Rectangle GetMouseWindowBounds()
        {
            Vector2 mousePosition = GetMouseWindowPosition();
            return new Rectangle((int)mousePosition.X, (int)mousePosition.Y, 1, 1);
        }
    }
}
