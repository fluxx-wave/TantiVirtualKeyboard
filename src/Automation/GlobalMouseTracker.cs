using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using HotVirtualKeyboard.Core;

namespace HotVirtualKeyboard.Automation
{
    public class GlobalMouseTracker
    {
        private static readonly Lazy<GlobalMouseTracker> _instance = new Lazy<GlobalMouseTracker>(CreateInstance);
        private static GlobalMouseTracker CreateInstance()
        {
            return new GlobalMouseTracker();
        }

        public static GlobalMouseTracker Instance
        {
            get { return _instance.Value; }
        }

        public event EventHandler<Point> OutsideClickOccurred;

        private IntPtr _hookId = IntPtr.Zero;
        private Win32.LowLevelMouseProc _proc;
        public Func<Point, bool> IsInsideKeyboard { get; set; }

        public GlobalMouseTracker()
        {
            _proc = HookCallback;
        }

        public void Start()
        {
            if (_hookId != IntPtr.Zero) return;

            try
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    IntPtr hModule = Win32.GetModuleHandle(curModule.ModuleName);
                    _hookId = Win32.SetWindowsHookEx(Win32.WH_MOUSE_LL, _proc, hModule, 0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set low-level mouse hook: " + ex.Message);
            }
        }

        public void Stop()
        {
            if (_hookId != IntPtr.Zero)
            {
                try
                {
                    Win32.UnhookWindowsHookEx(_hookId);
                }
                catch { }
                _hookId = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == Win32.WM_LBUTTONDOWN || msg == Win32.WM_NCLBUTTONDOWN)
                {
                    try
                    {
                        var hookStruct = (Win32.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.MSLLHOOKSTRUCT));
                        Point clickPoint = new Point(hookStruct.pt.X, hookStruct.pt.Y);

                        bool inside = false;
                        if (IsInsideKeyboard != null)
                        {
                            inside = IsInsideKeyboard(clickPoint);
                        }

                        if (!inside)
                        {
                            EventHandler<Point> handler = OutsideClickOccurred;
                            if (handler != null)
                            {
                                handler(this, clickPoint);
                            }
                        }
                    }
                    catch { }
                }
            }

            return Win32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
}
