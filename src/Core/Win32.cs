using System;
using System.Runtime.InteropServices;

namespace HotVirtualKeyboard.Core
{
    public static class Win32
    {
        #region Window Styles and Messages
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_TOPMOST = 0x00000008;

        public const int WM_MOUSEACTIVATE = 0x0021;
        public const int MA_NOACTIVATE = 3;
        public const int MA_NOACTIVATEANDEAT = 4;

        public const int WM_NCACTIVATE = 0x0086;
        public const int WM_NCHITTEST = 0x0084;

        public const int HTCLIENT = 1;
        public const int HTCAPTION = 2;
        public const int HTLEFT = 10;
        public const int HTRIGHT = 11;
        public const int HTTOP = 12;
        public const int HTTOPLEFT = 13;
        public const int HTTOPRIGHT = 14;
        public const int HTBOTTOM = 15;
        public const int HTBOTTOMLEFT = 16;
        public const int HTBOTTOMRIGHT = 17;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOZORDER = 0x0004;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_SHOWWINDOW = 0x0040;
        public const uint SWP_HIDEWINDOW = 0x0080;

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        #endregion

        #region SendInput Constants and Structs
        public const uint INPUT_MOUSE = 0;
        public const uint INPUT_KEYBOARD = 1;
        public const uint INPUT_HARDWARE = 2;

        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint KEYEVENTF_UNICODE = 0x0004;
        public const uint KEYEVENTF_SCANCODE = 0x0008;

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint type;
            public InputUnion U;
            public static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
        #endregion

        #region Virtual Keys
        public const ushort VK_LBUTTON = 0x01;
        public const ushort VK_BACK = 0x08;
        public const ushort VK_TAB = 0x09;
        public const ushort VK_CLEAR = 0x0C;
        public const ushort VK_RETURN = 0x0D;
        public const ushort VK_SHIFT = 0x10;
        public const ushort VK_CONTROL = 0x11;
        public const ushort VK_MENU = 0x12; // Alt
        public const ushort VK_PAUSE = 0x13;
        public const ushort VK_CAPITAL = 0x14; // CapsLock
        public const ushort VK_ESCAPE = 0x1B;
        public const ushort VK_SPACE = 0x20;
        public const ushort VK_PRIOR = 0x21; // Page Up
        public const ushort VK_NEXT = 0x22; // Page Down
        public const ushort VK_END = 0x23;
        public const ushort VK_HOME = 0x24;
        public const ushort VK_LEFT = 0x25;
        public const ushort VK_UP = 0x26;
        public const ushort VK_RIGHT = 0x27;
        public const ushort VK_DOWN = 0x28;
        public const ushort VK_SNAPSHOT = 0x2C; // Print Screen
        public const ushort VK_INSERT = 0x2D;
        public const ushort VK_DELETE = 0x2E;
        public const ushort VK_LWIN = 0x5B;
        public const ushort VK_RWIN = 0x5C;
        public const ushort VK_APPS = 0x5D;
        public const ushort VK_NUMPAD0 = 0x60;
        public const ushort VK_MULTIPLY = 0x6A;
        public const ushort VK_ADD = 0x6B;
        public const ushort VK_SEPARATOR = 0x6C;
        public const ushort VK_SUBTRACT = 0x6D;
        public const ushort VK_DECIMAL = 0x6E;
        public const ushort VK_DIVIDE = 0x6F;
        public const ushort VK_F1 = 0x70;
        public const ushort VK_NUMLOCK = 0x90;
        public const ushort VK_SCROLL = 0x91;
        public const ushort VK_LSHIFT = 0xA0;
        public const ushort VK_RSHIFT = 0xA1;
        public const ushort VK_LCONTROL = 0xA2;
        public const ushort VK_RCONTROL = 0xA3;
        public const ushort VK_LMENU = 0xA4;
        public const ushort VK_RMENU = 0xA5;
        #endregion

        #region Desktop / Lock Screen Interop
        public const uint DESKTOP_READOBJECTS = 0x0001;
        public const uint DESKTOP_CREATEWINDOW = 0x0002;
        public const uint DESKTOP_CREATEMENU = 0x0004;
        public const uint DESKTOP_HOOKCONTROL = 0x0008;
        public const uint DESKTOP_JOURNALRECORD = 0x0010;
        public const uint DESKTOP_JOURNALPLAYBACK = 0x0020;
        public const uint DESKTOP_ENUMERATE = 0x0040;
        public const uint DESKTOP_WRITEOBJECTS = 0x0080;
        public const uint DESKTOP_SWITCHDESKTOP = 0x0100;
        public const uint GENERIC_ALL = 0x10000000;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr OpenInputDesktop(uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr OpenDesktop(string lpszDesktop, uint dwFlags, bool fInherit, uint dwDesiredAccess);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetThreadDesktop(IntPtr hDesktop);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetThreadDesktop(int dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool CloseDesktop(IntPtr hDesktop);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();
        #endregion

        #region Window & Input Imports
        [DllImport("user32.dll")]
        public static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        // 32-bit & 64-bit safe window styles
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        #endregion

        #region Caret & GUI Thread Info
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public int cbSize;
            public int flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// Detects if the Windows 10/11 Start Menu or Search flyout is currently open and returns its bounding rectangle.
        /// </summary>
        public static bool IsStartMenuOrSearchOpen(out RECT rect)
        {
            rect = new RECT();
            try
            {
                // 1. Check Start Menu CoreWindow
                IntPtr hStart = FindWindow("Windows.UI.Core.CoreWindow", "Start");
                if (hStart != IntPtr.Zero && IsWindowVisible(hStart))
                {
                    if (GetWindowRect(hStart, out rect) && (rect.Right - rect.Left > 100) && (rect.Bottom - rect.Top > 100))
                    {
                        return true;
                    }
                }

                // 2. Check Search Flyout CoreWindow
                IntPtr hSearch = FindWindow("Windows.UI.Core.CoreWindow", "Search");
                if (hSearch != IntPtr.Zero && IsWindowVisible(hSearch))
                {
                    if (GetWindowRect(hSearch, out rect) && (rect.Right - rect.Left > 100) && (rect.Bottom - rect.Top > 100))
                    {
                        return true;
                    }
                }

                // 3. Check Foreground Window process for StartMenuExperienceHost or SearchHost
                IntPtr fg = GetForegroundWindow();
                if (fg != IntPtr.Zero)
                {
                    uint pid = 0;
                    GetWindowThreadProcessId(fg, out pid);
                    if (pid != 0)
                    {
                        var p = System.Diagnostics.Process.GetProcessById((int)pid);
                        if (p != null)
                        {
                            string pName = p.ProcessName;
                            if (pName.IndexOf("StartMenuExperienceHost", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                pName.IndexOf("SearchHost", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (GetWindowRect(fg, out rect) && (rect.Right - rect.Left > 100))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch { }

            return false;
        }
        #endregion

        #region Global Low-Level Mouse Hook
        public const int WH_MOUSE_LL = 14;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_NCLBUTTONDOWN = 0x00A1;

        public delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        #endregion

        #region Last Input Info & Touch Detection
        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        public const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

        public static bool HasRecentUserInput(int maxMilliseconds = 650)
        {
            try
            {
                var lii = new LASTINPUTINFO();
                lii.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));
                if (GetLastInputInfo(ref lii))
                {
                    uint elapsed = (uint)Environment.TickCount - lii.dwTime;
                    return elapsed <= (uint)maxMilliseconds;
                }
            }
            catch { }
            return true;
        }
        #endregion
    }
}
