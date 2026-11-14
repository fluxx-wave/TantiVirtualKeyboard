using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using HotVirtualKeyboard.Automation;
using HotVirtualKeyboard.Config;
using HotVirtualKeyboard.Core;
using HotVirtualKeyboard.Layouts;
using HotVirtualKeyboard.LockScreen;
using Application = System.Windows.Application;

namespace HotVirtualKeyboard
{
    public partial class App : Application
    {
        private NotifyIcon _notifyIcon;
        private MainWindow _mainWindow;
        private AppSettings _settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Apply System Windows Accent Color dynamically to Application Resources
            ThemeHelper.ApplyAccentColorToResources();

            _settings = AppSettings.Load();

            // Initialize Core Systems
            AudioManager.Instance.IsSoundEnabled = _settings.SoundFeedback;
            RepeatManager.Instance.InitialDelayMs = _settings.KeyRepeatDelayMs;
            RepeatManager.Instance.RepeatIntervalMs = _settings.KeyRepeatIntervalMs;

            LayoutManager.Instance.SelectLayoutById(_settings.DefaultLayoutId);

            FocusTracker.Instance.IsAutoShowEnabled = _settings.AutoShowOnFocus;
            FocusTracker.Instance.Start();

            LockScreenManager.Instance.IsLockScreenIntegrationEnabled = _settings.LockScreenIntegration;
            LockScreenManager.Instance.Initialize();

            bool isLockScreenArg = false;
            if (e.Args != null)
            {
                foreach (var arg in e.Args)
                {
                    if (arg.Equals("/lockscreen", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("/osk", StringComparison.OrdinalIgnoreCase))
                    {
                        isLockScreenArg = true;
                        break;
                    }
                }
            }

            if (isLockScreenArg)
            {
                var lockWin = new LockScreen.LockScreenWindow();
                lockWin.Show();
                return;
            }

            // Create Main Keyboard Window in background (shows only when user clicks an input box)
            _mainWindow = new MainWindow(_settings);
            _mainWindow.Show();
            _mainWindow.Visibility = Visibility.Collapsed;

            // Setup System Tray
            SetupNotifyIcon();
        }

        private void SetupNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "Hot Virtual Keyboard",
                Visible = true
            };

            // Generate crisp vector icon in memory for tray
            using (var bmp = new Bitmap(16, 16))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(System.Drawing.Color.FromArgb(0, 120, 215));
                using (var brush = new SolidBrush(System.Drawing.Color.White))
                {
                    // Draw keyboard icon symbol
                    g.FillRectangle(brush, 2, 4, 12, 8);
                    using (var dark = new SolidBrush(System.Drawing.Color.FromArgb(0, 120, 215)))
                    {
                        g.FillRectangle(dark, 4, 6, 2, 2);
                        g.FillRectangle(dark, 7, 6, 2, 2);
                        g.FillRectangle(dark, 10, 6, 2, 2);
                        g.FillRectangle(dark, 4, 9, 8, 1);
                    }
                }
                IntPtr hIcon = bmp.GetHicon();
                _notifyIcon.Icon = System.Drawing.Icon.FromHandle(hIcon);
            }

            var contextMenu = new ContextMenuStrip();

            var showItem = new ToolStripMenuItem("Show / Hide Keyboard", null, (s, a) => _mainWindow.ToggleVisibility());
            showItem.Font = new System.Drawing.Font(showItem.Font, System.Drawing.FontStyle.Bold);
            contextMenu.Items.Add(showItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var layoutsMenu = new ToolStripMenuItem("Keyboard Layouts");
            foreach (var layout in LayoutManager.Instance.AvailableLayouts)
            {
                var item = new ToolStripMenuItem(layout.Name, null, (s, a) =>
                {
                    LayoutManager.Instance.CurrentLayout = layout;
                    _settings.DefaultLayoutId = layout.Id;
                    _settings.Save();
                });
                layoutsMenu.DropDownItems.Add(item);
            }
            contextMenu.Items.Add(layoutsMenu);

            var autoShowItem = new ToolStripMenuItem("Auto-Show on Text Field Focus", null, (s, a) =>
            {
                var mi = (ToolStripMenuItem)s;
                mi.Checked = !mi.Checked;
                _settings.AutoShowOnFocus = mi.Checked;
                FocusTracker.Instance.IsAutoShowEnabled = mi.Checked;
                _settings.Save();
            });
            autoShowItem.Checked = _settings.AutoShowOnFocus;
            contextMenu.Items.Add(autoShowItem);

            var soundItem = new ToolStripMenuItem("Typing Sound Clicks", null, (s, a) =>
            {
                var mi = (ToolStripMenuItem)s;
                mi.Checked = !mi.Checked;
                _settings.SoundFeedback = mi.Checked;
                AudioManager.Instance.IsSoundEnabled = mi.Checked;
                _settings.Save();
            });
            soundItem.Checked = _settings.SoundFeedback;
            contextMenu.Items.Add(soundItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("Settings...", null, (s, a) =>
            {
                var settingsWin = new SettingsWindow(_settings);
                settingsWin.ShowDialog();
                _mainWindow.ApplySettings();
            });
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit", null, (s, a) =>
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
                FocusTracker.Instance.Stop();
                LockScreenManager.Instance.Shutdown();
                Shutdown();
            });
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, a) => _mainWindow.ToggleVisibility();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            FocusTracker.Instance.Stop();
            LockScreenManager.Instance.Shutdown();
            base.OnExit(e);
        }
    }
}
