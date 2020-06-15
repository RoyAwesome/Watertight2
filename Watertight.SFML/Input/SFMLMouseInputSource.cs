using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Watertight.Input;
using SFMLKeyEvent = SFML.Window.KeyEvent;
using WTKeyEvent = Watertight.Input.KeyEvent;

namespace Watertight.SFML.Input
{
    class SFMLMouseInputSource : IInputSource
    {
        public event WTKeyEvent OnPressedEvent;
        public event WTKeyEvent OnReleasedEvent;

        internal void Window_KeyPressed(object sender, MouseButtonEventArgs e)
        {
            OnPressedEvent?.Invoke(SFMLKeyConverter.Convert(e.Button), PlayerFlags.ANY_PLAYER);
        }

        internal void Window_KeyReleased(object sender, MouseButtonEventArgs e)
        {
            OnReleasedEvent?.Invoke(SFMLKeyConverter.Convert(e.Button), PlayerFlags.ANY_PLAYER);
        }

        internal void Window_MouseWheelScrolled(object sender, MouseWheelScrollEventArgs e)
        {
            Key WheelEvent = e.Delta >= 0 ? Key.MouseWheelUp : Key.MouseWheelDown;

            if(e.Delta != 0)
            {
                OnPressedEvent?.Invoke(WheelEvent, PlayerFlags.ANY_PLAYER);
                OnReleasedEvent?.Invoke(WheelEvent, PlayerFlags.ANY_PLAYER);
            }           
        }


        public float PollAxis(Key KeyName, PlayerFlags Player)
        {
            Vector2i MousePos = Mouse.GetPosition(SFMLRenderer.Instance.Window);
            if(KeyName == Key.MouseX)
            {
                return MousePos.X;
            }
            if(KeyName == Key.MouseY)
            {
                return MousePos.Y;
            }
           
            return PollInput(KeyName, Player) ? 1.0f : 0.0f;
        }

        public bool PollInput(Key KeyName, PlayerFlags Player)
        {
            return Mouse.IsButtonPressed(SFMLKeyConverter.ConvertMouse(KeyName));
        }
    }
}
