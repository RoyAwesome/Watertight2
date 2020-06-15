using System;
using System.Collections.Generic;
using System.Text;
using SFML.Window;
using SFMLKey = SFML.Window.Keyboard.Key;
using WTKey = Watertight.Input.Key;

namespace Watertight.SFML.Input
{
    /// <summary>
    /// Converts SFML Keys to Watertight Keys
    /// </summary>
    public static class SFMLKeyConverter
    {
        #region Keyboard Conversion
        public static WTKey Convert(SFMLKey Key)
        {

            if((int)Key >= (int)SFMLKey.A && (int)Key <= (int)SFMLKey.Z)
            {
                int KeyOffset = (int)Key - (int)SFMLKey.A;
                return (WTKey)(KeyOffset + (int)WTKey.A);
            }
            if((int)Key >= (int)SFMLKey.Num0 && (int)Key <= (int)SFMLKey.Num9)
            {
                int KeyOffset = (int)Key - (int)SFMLKey.Num0;
                return (WTKey)(KeyOffset + (int)WTKey.D0);
            }

            if ((int)Key >= (int)SFMLKey.Numpad0 && (int)Key <= (int)SFMLKey.Numpad9)
            {
                int KeyOffset = (int)Key - (int)SFMLKey.Numpad0;
                return (WTKey)(KeyOffset + (int)WTKey.NumPad0);
            }

            if ((int)Key >= (int)SFMLKey.F1 && (int)Key <= (int)SFMLKey.F15)
            {
                int KeyOffset = (int)Key - (int)SFMLKey.F1;
                return (WTKey)(KeyOffset + (int)WTKey.F1);
            }

            WTKey TheRest = (int)Key switch
            {
                (int)SFMLKey.Escape => WTKey.Escape,
                (int)SFMLKey.LControl => WTKey.LControlKey,
                (int)SFMLKey.LShift => WTKey.LShiftKey,
                (int)SFMLKey.LAlt => WTKey.LAltKey,
                (int)SFMLKey.LSystem => WTKey.LSystemKey,
                (int)SFMLKey.RControl => WTKey.RControlKey,
                (int)SFMLKey.RShift => WTKey.RShiftKey,
                (int)SFMLKey.RAlt => WTKey.RAltKey,
                (int)SFMLKey.RSystem => WTKey.RSystemKey,
                (int)SFMLKey.Menu => WTKey.Menu,
                (int)SFMLKey.Semicolon => WTKey.Semicolon,
                (int)SFMLKey.Comma => WTKey.Comma,
                (int)SFMLKey.Period => WTKey.Period,
                (int)SFMLKey.Quote => WTKey.Quote,
                (int)SFMLKey.Slash => WTKey.Question,
                (int)SFMLKey.Backslash => WTKey.Backslash,             
                (int)SFMLKey.Tilde => WTKey.Tilde,
                (int)SFMLKey.Equal => WTKey.Equal,
                (int)SFMLKey.Hyphen => WTKey.Minus,
                (int)SFMLKey.Space => WTKey.Space,
                (int)SFMLKey.Enter => WTKey.Enter,
                (int)SFMLKey.Backspace => WTKey.Backspace,
                (int)SFMLKey.Tab => WTKey.Tab,
                (int)SFMLKey.PageUp => WTKey.PageUp,
                (int)SFMLKey.PageDown => WTKey.PageDown,
                (int)SFMLKey.End => WTKey.End,
                (int)SFMLKey.Home => WTKey.Home,
                (int)SFMLKey.Insert => WTKey.Insert,
                (int)SFMLKey.Delete => WTKey.Delete,
                (int)SFMLKey.Add => WTKey.Add,
                (int)SFMLKey.Subtract => WTKey.Subtract,
                (int)SFMLKey.Multiply => WTKey.Multiply,
                (int)SFMLKey.Divide => WTKey.Divide,
                (int)SFMLKey.Up => WTKey.Up,
                (int)SFMLKey.Down => WTKey.Down,
                (int)SFMLKey.Left => WTKey.Left,
                (int)SFMLKey.Right => WTKey.Right,
                (int)SFMLKey.Pause => WTKey.Pause,
                _ => WTKey.None,
            };
            return TheRest;
        }

        public static SFMLKey ConvertKeyboard(WTKey Key)
        {
            if ((int)Key >= (int)WTKey.A && (int)Key <= (int)WTKey.Z)
            {
                int KeyOffset = (int)Key - (int)WTKey.A;
                return (SFMLKey)(KeyOffset + (int)SFMLKey.A);
            }
            if ((int)Key >= (int)WTKey.D0 && (int)Key <= (int)WTKey.D9)
            {
                int KeyOffset = (int)Key - (int)WTKey.D1;
                return (SFMLKey)(KeyOffset + (int)SFMLKey.Num0);
            }
            if ((int)Key >= (int)WTKey.NumPad0 && (int)Key <= (int)WTKey.NumPad9)
            {
                int KeyOffset = (int)Key - (int)WTKey.NumPad0;
                return (SFMLKey)(KeyOffset + (int)SFMLKey.Numpad0);
            }
            if ((int)Key >= (int)WTKey.F1 && (int)Key <= (int)WTKey.F15)
            {
                int KeyOffset = (int)Key - (int)WTKey.F1;
                return (SFMLKey)(KeyOffset + (int)SFMLKey.F1);
            }

            SFMLKey TheRest = (int)Key switch
            {
                (int)WTKey.Escape => SFMLKey.Escape,
                (int)WTKey.LControlKey => SFMLKey.LControl,
                (int)WTKey.LShiftKey => SFMLKey.LShift,
                (int)WTKey.LAltKey => SFMLKey.LAlt,
                (int)WTKey.LSystemKey => SFMLKey.LSystem,
                (int)WTKey.RControlKey => SFMLKey.RControl,
                (int)WTKey.RShiftKey => SFMLKey.RShift,
                (int)WTKey.RAltKey => SFMLKey.RAlt,
                (int)WTKey.RSystemKey => SFMLKey.RSystem,
                (int)WTKey.Menu => SFMLKey.Menu,
                (int)WTKey.Semicolon => SFMLKey.Semicolon,
                (int)WTKey.Comma => SFMLKey.Comma,
                (int)WTKey.Period => SFMLKey.Period,
                (int)WTKey.Quote => SFMLKey.Quote,
                (int)WTKey.Question => SFMLKey.Slash,
                (int)WTKey.Backslash => SFMLKey.Backslash,
                (int)WTKey.Tilde => SFMLKey.Tilde,
                (int)WTKey.Equal => SFMLKey.Equal,
                (int)WTKey.Minus => SFMLKey.Hyphen,
                (int)WTKey.Space => SFMLKey.Space,
                (int)WTKey.Enter => SFMLKey.Enter,
                (int)WTKey.Backspace => SFMLKey.Backspace,
                (int)WTKey.Tab => SFMLKey.Tab,
                (int)WTKey.PageUp => SFMLKey.PageUp,
                (int)WTKey.PageDown => SFMLKey.PageDown,
                (int)WTKey.End => SFMLKey.End,
                (int)WTKey.Home => SFMLKey.Home,
                (int)WTKey.Insert => SFMLKey.Insert,
                (int)WTKey.Delete => SFMLKey.Delete,
                (int)WTKey.Add => SFMLKey.Add,
                (int)WTKey.Subtract => SFMLKey.Subtract,
                (int)WTKey.Multiply => SFMLKey.Multiply,
                (int)WTKey.Divide => SFMLKey.Divide,
                (int)WTKey.Up => SFMLKey.Up,
                (int)WTKey.Down => SFMLKey.Down,
                (int)WTKey.Left => SFMLKey.Left,
                (int)WTKey.Right => SFMLKey.Right,
                (int)WTKey.Pause => SFMLKey.Pause,
                _ => SFMLKey.Unknown,
            };
            return TheRest;
        }
        #endregion

        public static WTKey Convert(Mouse.Button key)
        {
            return key switch
            {
                Mouse.Button.Left => WTKey.LeftMouse,
                Mouse.Button.Right => WTKey.RightMouse,
                Mouse.Button.Middle => WTKey.MiddleMouse,
                Mouse.Button.XButton1 => WTKey.Mouse4,
                Mouse.Button.XButton2 => WTKey.Mouse5,
                _ => WTKey.None,
            };
        }

        public static Mouse.Button ConvertMouse(WTKey Key)
        {
            return Key switch
            {
                WTKey.LeftMouse => Mouse.Button.Left,
                WTKey.RightMouse => Mouse.Button.Right,
                WTKey.MiddleMouse => Mouse.Button.Middle,
                WTKey.Mouse4 => Mouse.Button.XButton1,
                WTKey.Mouse5 => Mouse.Button.XButton2,
                _ => Mouse.Button.ButtonCount,
            };
        }

    }
}
