using System;

namespace HotVirtualKeyboard.Core
{
    public class KeyboardState
    {
        private static readonly Lazy<KeyboardState> _instance = new Lazy<KeyboardState>(CreateInstance);
        private static KeyboardState CreateInstance()
        {
            return new KeyboardState();
        }

        public static KeyboardState Instance
        {
            get { return _instance.Value; }
        }

        public event EventHandler ModifiersChanged;

        public bool IsShiftActive
        {
            get { return IsShiftDown || IsShiftLocked; }
        }

        public bool IsShiftDown { get; set; }
        public bool IsShiftLocked { get; set; }

        public bool IsCtrlDown { get; set; }
        public bool IsAltDown { get; set; }
        public bool IsWinDown { get; set; }
        public bool IsAltGrDown { get; set; }

        public bool IsCapsLock
        {
            get
            {
                return (Win32.GetKeyState(Win32.VK_CAPITAL) & 0x0001) != 0;
            }
        }

        public bool IsNumLock
        {
            get
            {
                return (Win32.GetKeyState(Win32.VK_NUMLOCK) & 0x0001) != 0;
            }
        }

        public bool IsScrollLock
        {
            get
            {
                return (Win32.GetKeyState(Win32.VK_SCROLL) & 0x0001) != 0;
            }
        }

        public bool ShouldCapitalize
        {
            get { return (IsShiftActive ^ IsCapsLock); }
        }

        public void ToggleShift()
        {
            if (!IsShiftDown && !IsShiftLocked)
            {
                IsShiftDown = true;
                IsShiftLocked = false;
            }
            else if (IsShiftDown && !IsShiftLocked)
            {
                IsShiftDown = true;
                IsShiftLocked = true;
            }
            else
            {
                IsShiftDown = false;
                IsShiftLocked = false;
            }
            NotifyChanged();
        }

        public void ToggleCtrl()
        {
            IsCtrlDown = !IsCtrlDown;
            NotifyChanged();
        }

        public void ToggleAlt()
        {
            IsAltDown = !IsAltDown;
            NotifyChanged();
        }

        public void ToggleWin()
        {
            IsWinDown = !IsWinDown;
            NotifyChanged();
        }

        public void ToggleAltGr()
        {
            IsAltGrDown = !IsAltGrDown;
            NotifyChanged();
        }

        public void OnCharacterInjected()
        {
            // If shift was temporarily pressed (not locked), reset it after 1 keystroke
            if (IsShiftDown && !IsShiftLocked)
            {
                IsShiftDown = false;
                NotifyChanged();
            }

            // Release transient modifier toggles if desired or maintain until toggled off
            if (IsCtrlDown || IsAltDown || IsWinDown || IsAltGrDown)
            {
                IsCtrlDown = false;
                IsAltDown = false;
                IsWinDown = false;
                IsAltGrDown = false;
                NotifyChanged();
            }
        }

        public void NotifyChanged()
        {
            EventHandler handler = ModifiersChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
