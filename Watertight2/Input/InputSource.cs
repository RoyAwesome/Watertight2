using System;
using System.Collections.Generic;
using System.Text;

namespace Watertight.Input
{
    public delegate void KeyEvent(Key KeyEvent, PlayerFlags Player);

    public interface IInputSource
    {
        event KeyEvent OnPressedEvent;
        event KeyEvent OnReleasedEvent;

        //Returns the raw value of an axis
        float PollAxis(Key KeyName, PlayerFlags Player);

        bool PollInput(Key KeyName, PlayerFlags Player);
    }
}
