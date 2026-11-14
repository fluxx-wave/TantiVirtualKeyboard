using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Win32;
using HotVirtualKeyboard.Core;

namespace HotVirtualKeyboard.LockScreen
{
    public class LockScreenManager
    {
        private static readonly Lazy<LockScreenManager> _instance = new Lazy<LockScreenManager>(CreateInstance);
        private static LockScreenManager CreateInstance()
        {
            return new LockScreenManager();
        }

        public static LockScreenManager Instance
        {
            get { return _instance.Value; }
        }

        public event EventHandler<bool> LockStateChanged;

        private Thread _lockScreenThread;
        private LockScreenWindow _currentLockWindow;
        private bool _isLocked = false;

        public bool IsLockScreenIntegrationEnabled { get; set; }
        public bool IsLocked
        {
            get { return _isLocked; }
        }

        public LockScreenManager()
        {
            IsLockScreenIntegrationEnabled = true;
        }

        public void Initialize()
        {
            try
            {
                SystemEvents.SessionSwitch += OnSessionSwitch;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("LockScreenManager init error: " + ex.Message);
            }
        }

        public void Shutdown()
        {
            try
            {
                SystemEvents.SessionSwitch -= OnSessionSwitch;
                CloseLockScreenWindow();
            }
            catch { }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (!IsLockScreenIntegrationEnabled) return;

            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                _isLocked = true;
                SpawnLockScreenKeyboard();
                EventHandler<bool> handler = LockStateChanged;
                if (handler != null)
                {
                    handler(this, true);
                }
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                _isLocked = false;
                CloseLockScreenWindow();
                EventHandler<bool> handler = LockStateChanged;
                if (handler != null)
                {
                    handler(this, false);
                }
            }
        }

        public void SpawnLockScreenKeyboard()
        {
            CloseLockScreenWindow();

            _lockScreenThread = new Thread(RunLockScreenSTA);
            _lockScreenThread.SetApartmentState(ApartmentState.STA);
            _lockScreenThread.IsBackground = true;
            _lockScreenThread.Start();
        }

        private void RunLockScreenSTA()
        {
            IntPtr hInputDesktop = IntPtr.Zero;
            try
            {
                // Correct access mask for Winlogon desktop without asking for unprivileged GENERIC_ALL
                uint desktopAccess = Win32.DESKTOP_CREATEWINDOW |
                                     Win32.DESKTOP_READOBJECTS |
                                     Win32.DESKTOP_WRITEOBJECTS |
                                     Win32.DESKTOP_SWITCHDESKTOP |
                                     Win32.DESKTOP_ENUMERATE;

                hInputDesktop = Win32.OpenInputDesktop(0, false, desktopAccess);
                if (hInputDesktop == IntPtr.Zero)
                {
                    // Fallback to explicit Winlogon desktop name
                    hInputDesktop = Win32.OpenDesktop("Winlogon", 0, false, desktopAccess);
                }

                if (hInputDesktop != IntPtr.Zero)
                {
                    bool switched = Win32.SetThreadDesktop(hInputDesktop);
                    if (switched)
                    {
                        Debug.WriteLine("SetThreadDesktop succeeded for Winlogon lock screen.");
                    }
                }

                // Create the dedicated lock screen window on this desktop
                _currentLockWindow = new LockScreenWindow();
                _currentLockWindow.Closed += delegate
                {
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                };
                _currentLockWindow.Show();

                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error in RunLockScreenSTA: " + ex.Message);
            }
            finally
            {
                if (hInputDesktop != IntPtr.Zero)
                {
                    Win32.CloseDesktop(hInputDesktop);
                }
                _currentLockWindow = null;
            }
        }

        public void CloseLockScreenWindow()
        {
            try
            {
                if (_currentLockWindow != null)
                {
                    _currentLockWindow.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        try
                        {
                            _currentLockWindow.Close();
                        }
                        catch { }
                    }));
                }

                if (_lockScreenThread != null && _lockScreenThread.IsAlive)
                {
                    _lockScreenThread.Join(500);
                    _lockScreenThread = null;
                }
            }
            catch { }
        }
    }
}
