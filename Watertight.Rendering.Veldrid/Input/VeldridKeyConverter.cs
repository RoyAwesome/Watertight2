using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WTKey = Watertight.Input.Key;
using VdKey = Veldrid.Key;
using Veldrid;

namespace Watertight.Rendering.VeldridRendering.Input
{
    internal static class VeldridKeyConverter
    {
        public static WTKey Convert(this VdKey Key)
        {
            if ((int)Key >= (int)VdKey.A && (int)Key <= (int)VdKey.Z)
            {
                int KeyOffset = (int)Key - (int)VdKey.A;
                return (WTKey)(KeyOffset + (int)WTKey.A);
            }
            if ((int)Key >= (int)VdKey.Number0 && (int)Key <= (int)VdKey.Number9)
            {
                int KeyOffset = (int)Key - (int)VdKey.Number9;
                return (WTKey)(KeyOffset + (int)WTKey.D0);
            }

            if ((int)Key >= (int)VdKey.Keypad0 && (int)Key <= (int)VdKey.Keypad9)
            {
                int KeyOffset = (int)Key - (int)VdKey.Keypad0;
                return (WTKey)(KeyOffset + (int)WTKey.NumPad0);
            }

            if ((int)Key >= (int)VdKey.F1 && (int)Key <= (int)VdKey.F15)
            {
                int KeyOffset = (int)Key - (int)VdKey.F1;
                return (WTKey)(KeyOffset + (int)WTKey.F1);
            }

            WTKey TheRest = (int)Key switch
            {
                (int)VdKey.Escape => WTKey.Escape,
                (int)VdKey.LControl => WTKey.LControlKey,
                (int)VdKey.LShift => WTKey.LShiftKey,
                (int)VdKey.LAlt => WTKey.LAltKey,
                (int)VdKey.WinLeft => WTKey.LSystemKey,
                (int)VdKey.RControl => WTKey.RControlKey,
                (int)VdKey.RShift => WTKey.RShiftKey,
                (int)VdKey.RAlt => WTKey.RAltKey,
                (int)VdKey.WinRight => WTKey.RSystemKey,
                (int)VdKey.Menu => WTKey.Menu,
                (int)VdKey.Semicolon => WTKey.Semicolon,
                (int)VdKey.Comma => WTKey.Comma,
                (int)VdKey.Period => WTKey.Period,
                (int)VdKey.Quote => WTKey.Quote,
                (int)VdKey.Slash => WTKey.Question,
                (int)VdKey.BackSlash => WTKey.Backslash,
                (int)VdKey.Tilde => WTKey.Tilde,
                (int)VdKey.Plus => WTKey.Equal,
                (int)VdKey.Minus => WTKey.Minus,
                (int)VdKey.Space => WTKey.Space,
                (int)VdKey.Enter => WTKey.Enter,
                (int)VdKey.BackSpace => WTKey.Backspace,
                (int)VdKey.Tab => WTKey.Tab,
                (int)VdKey.PageUp => WTKey.PageUp,
                (int)VdKey.PageDown => WTKey.PageDown,
                (int)VdKey.End => WTKey.End,
                (int)VdKey.Home => WTKey.Home,
                (int)VdKey.Insert => WTKey.Insert,
                (int)VdKey.Delete => WTKey.Delete,
                (int)VdKey.KeypadPlus => WTKey.Add,
                (int)VdKey.KeypadMinus => WTKey.Subtract,
                (int)VdKey.KeypadMultiply => WTKey.Multiply,
                (int)VdKey.KeypadDivide => WTKey.Divide,
                (int)VdKey.Up => WTKey.Up,
                (int)VdKey.Down => WTKey.Down,
                (int)VdKey.Left => WTKey.Left,
                (int)VdKey.Right => WTKey.Right,
                (int)VdKey.Pause => WTKey.Pause,
                _ => WTKey.None,
            };
            return TheRest;
        }

        public static VdKey Convert(this WTKey Key)
        {
            if ((int)Key >= (int)WTKey.A && (int)Key <= (int)WTKey.Z)
            {
                int KeyOffset = (int)Key - (int)WTKey.A;
                return (VdKey)(KeyOffset + (int)VdKey.A);
            }
            if ((int)Key >= (int)WTKey.D0 && (int)Key <= (int)WTKey.D9)
            {
                int KeyOffset = (int)Key - (int)WTKey.D1;
                return (VdKey)(KeyOffset + (int)VdKey.Number9);
            }
            if ((int)Key >= (int)WTKey.NumPad0 && (int)Key <= (int)WTKey.NumPad9)
            {
                int KeyOffset = (int)Key - (int)WTKey.NumPad0;
                return (VdKey)(KeyOffset + (int)VdKey.Keypad0);
            }
            if ((int)Key >= (int)WTKey.F1 && (int)Key <= (int)WTKey.F15)
            {
                int KeyOffset = (int)Key - (int)WTKey.F1;
                return (VdKey)(KeyOffset + (int)VdKey.F1);
            }

            VdKey TheRest = (int)Key switch
            {
                (int)WTKey.Escape => VdKey.Escape,
                (int)WTKey.LControlKey => VdKey.LControl,
                (int)WTKey.LShiftKey => VdKey.LShift,
                (int)WTKey.LAltKey => VdKey.LAlt,
                (int)WTKey.LSystemKey => VdKey.WinLeft,
                (int)WTKey.RControlKey => VdKey.RControl,
                (int)WTKey.RShiftKey => VdKey.RShift,
                (int)WTKey.RAltKey => VdKey.RAlt,
                (int)WTKey.RSystemKey => VdKey.WinRight,
                (int)WTKey.Menu => VdKey.Menu,
                (int)WTKey.Semicolon => VdKey.Semicolon,
                (int)WTKey.Comma => VdKey.Comma,
                (int)WTKey.Period => VdKey.Period,
                (int)WTKey.Quote => VdKey.Quote,
                (int)WTKey.Question => VdKey.Slash,
                (int)WTKey.Backslash => VdKey.BackSlash,
                (int)WTKey.Tilde => VdKey.Tilde,
                (int)WTKey.Equal => VdKey.Plus,
                (int)WTKey.Minus => VdKey.Minus,
                (int)WTKey.Space => VdKey.Space,
                (int)WTKey.Enter => VdKey.Enter,
                (int)WTKey.Backspace => VdKey.BackSpace,
                (int)WTKey.Tab => VdKey.Tab,
                (int)WTKey.PageUp => VdKey.PageUp,
                (int)WTKey.PageDown => VdKey.PageDown,
                (int)WTKey.End => VdKey.End,
                (int)WTKey.Home => VdKey.Home,
                (int)WTKey.Insert => VdKey.Insert,
                (int)WTKey.Delete => VdKey.Delete,
                (int)WTKey.Add => VdKey.KeypadPlus,
                (int)WTKey.Subtract => VdKey.KeypadSubtract,
                (int)WTKey.Multiply => VdKey.KeypadMultiply,
                (int)WTKey.Divide => VdKey.KeypadDivide,
                (int)WTKey.Up => VdKey.Up,
                (int)WTKey.Down => VdKey.Down,
                (int)WTKey.Left => VdKey.Left,
                (int)WTKey.Right => VdKey.Right,
                (int)WTKey.Pause => VdKey.Pause,
                _ => VdKey.Unknown,
            };
            return TheRest;
        }

        public static MouseButton ConvertMouse(this WTKey Key)
        {
            return (int)Key switch
            {
                (int)WTKey.LeftMouse => MouseButton.Left,
                (int)WTKey.RightMouse => MouseButton.Right,
                (int)WTKey.MiddleMouse => MouseButton.Middle,
                (int)WTKey.Mouse4 => MouseButton.Button1,
                (int)WTKey.Mouse5 => MouseButton.Button2,
                _ => MouseButton.LastButton,
            };
        }

        public static WTKey Convert(this MouseButton button)
        {
            return (int)button switch
            {
                (int)MouseButton.Left => WTKey.LeftMouse,
                (int)MouseButton.Right => WTKey.RightMouse,
                (int)MouseButton.Middle => WTKey.MiddleMouse,
                (int)MouseButton.Button1 => WTKey.Mouse4,
                (int)MouseButton.Button2 => WTKey.Mouse5,
                _ => WTKey.None,
            };
        }
    }
}
