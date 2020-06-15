using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Watertight.Input
{

    public class InputProcessor
    {
        public enum InputEvent
        {
            Pressed,
            Released,
        }

        class ActionBinding
        {
            public string Name;
            public Action Action;
            public InputEvent InputEvent;
        }

        class ActionBindingName
        {
            public string Name;
            public List<Key> Keys
            {
                get;
            } = new List<Key>();
        }


        static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static List<IInputSource> InputSources = new List<IInputSource>();

       
        static List<ActionBinding> ActionBindingEvents = new List<ActionBinding>();
        static List<ActionBindingName> ActionBindingNameList = new List<ActionBindingName>();

        public static void RegisterInputSource(IInputSource InputSource)
        {
            if(InputSources.Contains(InputSource))
            {
                return;
            }

            InputSource.OnPressedEvent += InputSource_OnPressedEvent;
            InputSource.OnReleasedEvent += InputSource_OnReleasedEvent;
            InputSources.Add(InputSource);
        }

        public static void BindInput(string ActionName, Action Binding, InputEvent InputMode)
        {
            ActionBinding AB = GetActionBinding(ActionName, InputMode);
            if (AB != null)
            {
                AB.Action += Binding;
            }
            else
            {
                AB = new ActionBinding
                {
                    Name = ActionName,
                    Action = Binding,
                    InputEvent = InputMode,
                };
                ActionBindingEvents.Add(AB);
            }
        }

       public static void RemoveBinding(string ActionName, Action Binding, InputEvent InputMode)
       {
            ActionBinding AB = GetActionBinding(ActionName, InputMode);
            if (AB != null)
            {
                AB.Action -= Binding;

                if(AB.Action == null)
                {
                    ActionBindingEvents.Remove(AB);
                }
            }
        }

        public static void UpdateActionBinding(string ActionName, params Key[] keys)
        {
            var BN = ActionBindingNameList.FirstOrDefault(x => x.Name == ActionName);
            if(BN != null)
            {
                foreach(Key key in keys)
                {
                    if (!BN.Keys.Contains(key))
                    {
                        BN.Keys.Add(key);
                    }
                }                       
            }
            else
            {
                BN = new ActionBindingName
                {
                    Name = ActionName,
                };
                BN.Keys.AddRange(keys);

                ActionBindingNameList.Add(BN);
            }
        }

        public static bool IsKeyDown(Key KeyEvent, PlayerFlags Player = PlayerFlags.ANY_PLAYER)
        {
            bool KeyDown = false;
            foreach(IInputSource source in InputSources)
            {
                KeyDown |= source.PollInput(KeyEvent, Player);
            }

            return KeyDown;
        }

        public static float PollAxis(Key AxisKey, PlayerFlags Player = PlayerFlags.ANY_PLAYER)
        {
            float AxisValue = 0.0f;
            foreach (IInputSource source in InputSources)
            {
                AxisValue += source.PollAxis(AxisKey, Player);
            }

            return AxisValue;
        }

        private static void InputSource_OnReleasedEvent(Key KeyEvent, PlayerFlags Player)
        {
            foreach (string ActionName in MatchBinding(KeyEvent))
            {
                CallActionEvent(ActionName, InputEvent.Released);
            }
        }

        private static void InputSource_OnPressedEvent(Key KeyEvent, PlayerFlags Player)
        {
            foreach(string ActionName in MatchBinding(KeyEvent))
            {
                CallActionEvent(ActionName, InputEvent.Pressed);
            }           
        }

        private static void CallActionEvent(string ActionName, InputEvent Mode)
        {
            Logger.Info(string.Format("{0} - {1}", ActionName, Mode.ToString()));
            ActionBinding AB = GetActionBinding(ActionName, Mode);
             AB?.Action?.Invoke();
        }

        private static ActionBinding GetActionBinding(string ActionName, InputEvent Mode)
        {
            return ActionBindingEvents.FirstOrDefault(x => x.Name == ActionName && x.InputEvent == Mode);
        }

        private static IEnumerable<string> MatchBinding(Key Key)
        {
            return ActionBindingNameList.Where(x => x.Keys.Contains(Key)).Select(x => x.Name);
        }
    }
}
