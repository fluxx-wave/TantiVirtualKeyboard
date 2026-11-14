using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using HotVirtualKeyboard.Core;
using HotVirtualKeyboard.Layouts;

namespace HotVirtualKeyboard.LockScreen
{
    public partial class LockScreenWindow : Window
    {
        private bool _isLetterMode = false;
        private int _lastTouchTick = 0;
        private long _lastPressTick = 0;
        private readonly object _pressLock = new object();
        private readonly List<Button> _renderedButtons = new List<Button>();

        public LockScreenWindow()
        {
            InitializeComponent();
            RenderLayout();

            Loaded += delegate
            {
                PositionOnLockScreen();
            };
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int exStyle = Win32.GetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE).ToInt32();
            exStyle |= Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST;
            Win32.SetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE, new IntPtr(exStyle));

            HwndSource source = HwndSource.FromHwnd(hwnd);
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(Win32.MA_NOACTIVATE);
            }
            return IntPtr.Zero;
        }

        private void PositionOnLockScreen()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left + Math.Max(10.0, (workArea.Width - Width) / 2.0);
            Top = workArea.Bottom - Height - 20.0;
        }

        private void RenderLayout()
        {
            LockKeysGrid.Children.Clear();
            LockKeysGrid.RowDefinitions.Clear();
            LockKeysGrid.ColumnDefinitions.Clear();
            _renderedButtons.Clear();

            KeyboardLayout layout = _isLetterMode 
                ? LayoutDefinitions.CreateTouchLayout() 
                : LayoutDefinitions.CreateNumpadPinLayout();

            if (layout == null || layout.Rows.Count == 0) return;

            for (int r = 0; r < layout.Rows.Count; r++)
            {
                LockKeysGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int r = 0; r < layout.Rows.Count; r++)
            {
                var rowModel = layout.Rows[r];
                var rowGrid = new Grid();

                foreach (var key in rowModel.Keys)
                {
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(key.WidthRatio, GridUnitType.Star)
                    });
                }

                for (int c = 0; c < rowModel.Keys.Count; c++)
                {
                    var keyModel = rowModel.Keys[c];
                    var btn = CreateKeyButton(keyModel);
                    Grid.SetColumn(btn, c);
                    rowGrid.Children.Add(btn);
                    _renderedButtons.Add(btn);
                }

                Grid.SetRow(rowGrid, r);
                LockKeysGrid.Children.Add(rowGrid);
            }
        }

        private Button CreateKeyButton(KeyModel model)
        {
            var btn = new Button
            {
                Focusable = false,
                Background = new SolidColorBrush(Color.FromRgb(44, 44, 44)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin = new Thickness(2),
                Tag = model
            };

            var txt = new TextBlock
            {
                Text = model.PrimaryLabel,
                FontSize = model.PrimaryLabel.Length > 2 ? 12 : 18,
                FontWeight = FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            btn.Content = txt;

            btn.PreviewTouchDown += delegate(object s, TouchEventArgs e)
            {
                e.Handled = true;
                _lastTouchTick = Environment.TickCount;
                ProcessSafeKeyPress(model);
            };

            btn.PreviewMouseDown += delegate(object s, MouseButtonEventArgs e)
            {
                e.Handled = true;
                if (Math.Abs(Environment.TickCount - _lastTouchTick) < 300)
                {
                    return;
                }
                ProcessSafeKeyPress(model);
            };

            return btn;
        }

        private void ProcessSafeKeyPress(KeyModel key)
        {
            lock (_pressLock)
            {
                long now = DateTime.UtcNow.Ticks;
                if (now - _lastPressTick < TimeSpan.FromMilliseconds(90).Ticks)
                {
                    return;
                }
                _lastPressTick = now;

                AudioManager.Instance.PlayKeySound();

                if (key.VkCode != 0)
                {
                    KeyInjector.SendVirtualKey(key.VkCode);
                }
                else if (!string.IsNullOrEmpty(key.PrimaryLabel))
                {
                    if (key.PrimaryLabel == "ABC")
                    {
                        BtnModeToggle_Click(null, null);
                        return;
                    }
                    KeyInjector.SendText(key.PrimaryLabel);
                }
            }
        }

        private void BtnModeToggle_Click(object sender, RoutedEventArgs e)
        {
            _isLetterMode = !_isLetterMode;
            BtnModeToggle.Content = _isLetterMode ? "Switch to PIN Numpad" : "Switch to Full Letters";
            Width = _isLetterMode ? 800 : 680;
            Height = _isLetterMode ? 300 : 280;
            RenderLayout();
            PositionOnLockScreen();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
