using System;
using System.Collections.Generic;
using System.Text;
using Watertight.Input;
using WTKey = Watertight.Input.Key;
using VdKey = Veldrid.Key;
using System.Linq;

namespace Watertight.Rendering.VeldridRendering.Input
{
    class VeldridInputSource : IInputSource
    {     
        public event KeyEvent OnPressedEvent;
        public event KeyEvent OnReleasedEvent;

        
        internal void OnWindowKeyEvent(Veldrid.KeyEvent obj)
        {
            if(obj.Repeat)
            {
                return;
            }

            if (obj.Down)
            {
                
                OnPressedEvent?.Invoke(obj.Key.Convert(), PlayerFlags.ANY_PLAYER);
            }
            else
            {
                OnReleasedEvent?.Invoke(obj.Key.Convert(), PlayerFlags.ANY_PLAYER);
            }
           
        }
        internal void OnWindowMouseEvent(Veldrid.MouseEvent obj)
        {
           
            if (obj.Down)
            {
                OnPressedEvent?.Invoke(obj.MouseButton.Convert(), PlayerFlags.ANY_PLAYER);
            }
            else
            {
                OnReleasedEvent?.Invoke(obj.MouseButton.Convert(), PlayerFlags.ANY_PLAYER);
            }
        }


        Veldrid.InputSnapshot lastInputSnapshot;
        internal void LastInputSnapshot(Veldrid.InputSnapshot inputSnapshot)
        {
            lastInputSnapshot = inputSnapshot;
        }

        public float PollAxis(WTKey KeyName, PlayerFlags Player)
        {
            if(lastInputSnapshot.KeyEvents.Count(x => x.Key == KeyName.Convert()) > 0)
            {
                return lastInputSnapshot.KeyEvents.First(x => x.Key == KeyName.Convert()).Down ? 0 : 1;
            }
            if(KeyName == WTKey.MouseX)
            {
                return lastInputSnapshot.MousePosition.X;
            }
            if(KeyName == WTKey.MouseY)
            {
                return lastInputSnapshot.MousePosition.Y;
            }

            return 0;
          
        }

        public bool PollInput(WTKey KeyName, PlayerFlags Player)
        {
            if (lastInputSnapshot.KeyEvents.Count(x => x.Key == KeyName.Convert()) > 0)
            {
                return lastInputSnapshot.KeyEvents.First(x => x.Key == KeyName.Convert()).Down;
            }
            return lastInputSnapshot.IsMouseDown(KeyName.ConvertMouse());

            //return false;
        }
    }
}
