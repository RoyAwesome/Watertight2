using SFML.Window;
using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Input;
using SFMLKeyEvent = SFML.Window.KeyEvent;
using WTKeyEvent = Watertight.Input.KeyEvent;

namespace Watertight.SFML.Input
{
    class SFMLKeyboardInputSource
        : IInputSource
    {            
        public event WTKeyEvent OnPressedEvent;
        public event WTKeyEvent OnReleasedEvent;
             

        internal void Window_KeyPressed(object sender, KeyEventArgs e)
        {
            OnPressedEvent?.Invoke(SFMLKeyConverter.Convert(e.Code), PlayerFlags.ANY_PLAYER);
        }

        internal void Window_KeyReleased(object sender, KeyEventArgs e)
        {
            OnReleasedEvent?.Invoke(SFMLKeyConverter.Convert(e.Code), PlayerFlags.ANY_PLAYER);
        }

        public float PollAxis(Key KeyName, PlayerFlags Player)
        {
            if(PollInput(KeyName, Player))
            {
                return 1.0f;
            }
            else
            {
                return 0.0f;
            }
        }

        public bool PollInput(Key KeyName, PlayerFlags Player)
        {
            return Keyboard.IsKeyPressed(SFMLKeyConverter.ConvertKeyboard(KeyName));            
        }
    }
}
