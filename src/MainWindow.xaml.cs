using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using HotVirtualKeyboard.Automation;
using HotVirtualKeyboard.Config;
using HotVirtualKeyboard.Core;
using HotVirtualKeyboard.Layouts;
using HotVirtualKeyboard.LockScreen;
using HotVirtualKeyboard.Prediction;

namespace HotVirtualKeyboard
{
    public partial class MainWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly List<Button> _renderedKeyButtons;
        private readonly HashSet<Button> _activePressedButtons;
        private readonly DispatcherTimer _autoHideTimer;
        private readonly DispatcherTimer _outsideClickTimer;
        private readonly DispatcherTimer _topmostTimer;
        private SettingsWindow _activeSettingsWindow = null;
        private int _lastTouchTick = 0;
        private long _lastPressTick = 0;
        private readonly object _pressLock = new object();
        private bool _isClamping = false;
        private bool _isResizeMode = false;

        public MainWindow(AppSettings settings)
        {
            _settings = settings;
            _renderedKeyButtons = new List<Button>();
            _activePressedButtons = new HashSet<Button>();

            InitializeComponent();

            ThemeHelper.ApplyAccentColorToResources();
            ApplySettings();

            // Setup Auto-Hide Timer when clicking non-input areas
            _autoHideTimer = new DispatcherTimer();
            _autoHideTimer.Interval = TimeSpan.FromMilliseconds(200);
            _autoHideTimer.Tick += delegate
            {
                _autoHideTimer.Stop();
                // Never auto-hide while the user is using the Settings window
                if (_activeSettingsWindow != null && _activeSettingsWindow.IsVisible) return;

                if (Visibility == Visibility.Visible && _settings.AutoShowOnFocus)
                {
                    // Check if current focused control in Windows is an editable text field
                    if (!FocusTracker.IsCurrentFocusTextField())
                    {
                        Visibility = Visibility.Collapsed;
                    }
                }
            };

            // Setup Rapid Outside-Click Detection Timer (30ms) for Instant Re-opening
            _outsideClickTimer = new DispatcherTimer();
            _outsideClickTimer.Interval = TimeSpan.FromMilliseconds(30);
            _outsideClickTimer.Tick += delegate
            {
                _outsideClickTimer.Stop();
                if (!_settings.AutoShowOnFocus) return;

                // While Settings Window is open, do not auto-hide or reposition keyboard
                if (_activeSettingsWindow != null && _activeSettingsWindow.IsVisible) return;

                if (_outsideClickTimer.Tag is Point)
                {
                    Point clickPoint = (Point)_outsideClickTimer.Tag;
                    bool isText = FocusTracker.IsPointTextInputControl(clickPoint);

                    if (isText)
                    {
                        _autoHideTimer.Stop();
                        if (Visibility != Visibility.Visible)
                        {
                            Visibility = Visibility.Visible;
                        }
                        if (_settings.Dock == DockMode.Floating)
                        {
                            Rect textRect = FocusTracker.GetCaretOrElementRect(clickPoint);
                            if (!textRect.IsEmpty)
                            {
                                double curW = ActualWidth > 50 ? ActualWidth : Width;
                                double curH = ActualHeight > 50 ? ActualHeight : Height;
                                var optimalPos = CaretTracker.CalculateOptimalWindowPosition(textRect, curW, curH);
                                Left = optimalPos.X;
                                Top = optimalPos.Y;
                            }
                        }
                        else if (_settings.Dock == DockMode.DockBottom)
                        {
                            var workArea = SystemParameters.WorkArea;
                            double curH = ActualHeight > 50 ? ActualHeight : Height;
                            Win32.RECT startRect;
                            if (Win32.IsStartMenuOrSearchOpen(out startRect))
                            {
                                Top = workArea.Top;
                            }
                            else
                            {
                                Top = workArea.Bottom - curH;
                            }
                        }
                        ClampToViewport();
                        ReassertTopmost();
                    }
                    else
                    {
                        // Outside click on a non-input area: immediately hide / minimize virtual keyboard
                        if (Visibility == Visibility.Visible)
                        {
                            Visibility = Visibility.Collapsed;
                        }
                    }
                }
            };

            // Setup Global Mouse Tracker for reliable outside click auto-hide & instant reopen
            GlobalMouseTracker.Instance.IsInsideKeyboard = delegate(Point screenPt)
            {
                try
                {
                    double scale = DpiHelper.GetSystemDpiScale();
                    Point dipPt = DpiHelper.PhysicalToDip(screenPt, scale);

                    // Check if inside MainWindow
                    if (Visibility == Visibility.Visible)
                    {
                        double curW = ActualWidth > 50 ? ActualWidth : Width;
                        double curH = ActualHeight > 50 ? ActualHeight : Height;
                        Rect kbRect = new Rect(Left, Top, curW, curH);
                        if (kbRect.Contains(dipPt)) return true;
                    }

                    // Check if inside active SettingsWindow
                    if (_activeSettingsWindow != null && _activeSettingsWindow.IsVisible)
                    {
                        double setW = _activeSettingsWindow.ActualWidth > 50 ? _activeSettingsWindow.ActualWidth : _activeSettingsWindow.Width;
                        double setH = _activeSettingsWindow.ActualHeight > 50 ? _activeSettingsWindow.ActualHeight : _activeSettingsWindow.Height;
                        Rect setRect = new Rect(_activeSettingsWindow.Left, _activeSettingsWindow.Top, setW, setH);
                        if (setRect.Contains(dipPt)) return true;
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            };

            GlobalMouseTracker.Instance.OutsideClickOccurred += delegate(object s, Point clickPoint)
            {
                if (!_settings.AutoShowOnFocus) return;
                _outsideClickTimer.Stop();
                _outsideClickTimer.Tag = clickPoint;
                _outsideClickTimer.Start();
            };

            GlobalMouseTracker.Instance.Start();

            // Setup periodic topmost keep-alive timer (500ms for crisp responsiveness)
            _topmostTimer = new DispatcherTimer();
            _topmostTimer.Interval = TimeSpan.FromMilliseconds(500);
            _topmostTimer.Tick += delegate
            {
                if (Visibility == Visibility.Visible)
                {
                    ReassertTopmost();
                }
            };
            _topmostTimer.Start();

            // Wire up Core Events
            KeyboardState.Instance.ModifiersChanged += delegate(object s, EventArgs e)
            {
                UpdateModifierVisuals();
            };

            LayoutManager.Instance.CurrentLayoutChanged += delegate(object s, KeyboardLayout layout)
            {
                RenderLayout(layout);
            };

            WordPredictor.Instance.SuggestionsUpdated += delegate(object s, IReadOnlyList<string> list)
            {
                UpdateSuggestionsUI(list);
            };

            VoiceInputManager.Instance.ListeningStateChanged += delegate(object s, bool isListening)
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    UpdateVoiceInputUI(isListening);
                }));
            };

            FocusTracker.Instance.FocusChanged += OnFocusTrackerChanged;
            LockScreenManager.Instance.LockStateChanged += OnLockStateChanged;

            // Render Initial Layout & Suggestions
            RenderLayout(LayoutManager.Instance.CurrentLayout);
            UpdateSuggestionsUI(WordPredictor.Instance.GetSuggestions());

            LocationChanged += delegate { ClampToViewport(); };
            SizeChanged += OnWindowSizeChanged;
            Closed += delegate { GlobalMouseTracker.Instance.Stop(); };
        }

        #region Window Styles & WS_EX_NOACTIVATE & 8-Direction Border Resizing
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // Set WS_EX_NOACTIVATE to prevent focus stealing from target apps
            int exStyle = Win32.GetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE).ToInt32();
            exStyle |= Win32.WS_EX_NOACTIVATE | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST;
            Win32.SetWindowLongPtr(hwnd, Win32.GWL_EXSTYLE, new IntPtr(exStyle));

            HwndSource source = HwndSource.FromHwnd(hwnd);
            if (source != null)
            {
                source.AddHook(WndProc);
            }

            ApplyTheme(_settings.Theme);
            ApplyDocking();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32.WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(Win32.MA_NOACTIVATE);
            }

            // Real border dragging resize for borderless window (8 directions)
            if (msg == Win32.WM_NCHITTEST)
            {
                int screenX = (short)(lParam.ToInt32() & 0xFFFF);
                int screenY = (short)((lParam.ToInt32() >> 16) & 0xFFFF);

                try
                {
                    Point pt = PointFromScreen(new Point(screenX, screenY));
                    const double borderThickness = 12.0;

                    if (_settings.Dock == DockMode.Floating)
                    {
                        bool isLeft = pt.X <= borderThickness;
                        bool isRight = pt.X >= ActualWidth - borderThickness;
                        bool isTop = pt.Y <= borderThickness;
                        bool isBottom = pt.Y >= ActualHeight - borderThickness;

                        if (isTop && isLeft) { handled = true; return new IntPtr(Win32.HTTOPLEFT); }
                        if (isTop && isRight) { handled = true; return new IntPtr(Win32.HTTOPRIGHT); }
                        if (isBottom && isLeft) { handled = true; return new IntPtr(Win32.HTBOTTOMLEFT); }
                        if (isBottom && isRight) { handled = true; return new IntPtr(Win32.HTBOTTOMRIGHT); }
                        if (isLeft) { handled = true; return new IntPtr(Win32.HTLEFT); }
                        if (isRight) { handled = true; return new IntPtr(Win32.HTRIGHT); }
                        if (isTop) { handled = true; return new IntPtr(Win32.HTTOP); }
                        if (isBottom) { handled = true; return new IntPtr(Win32.HTBOTTOM); }
                    }
                    else if (_settings.Dock == DockMode.DockBottom)
                    {
                        // Allow dragging top border to resize docked keyboard height
                        if (pt.Y <= borderThickness)
                        {
                            handled = true;
                            return new IntPtr(Win32.HTTOP);
                        }
                    }
                    else if (_settings.Dock == DockMode.DockTop)
                    {
                        // Allow dragging bottom border to resize docked keyboard height
                        if (pt.Y >= ActualHeight - borderThickness)
                        {
                            handled = true;
                            return new IntPtr(Win32.HTBOTTOM);
                        }
                    }
                }
                catch { }
            }

            return IntPtr.Zero;
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 200 && e.NewSize.Height > 100)
            {
                Width = e.NewSize.Width;
                Height = e.NewSize.Height;

                if (_settings.Dock == DockMode.Floating)
                {
                    _settings.FloatingWidth = e.NewSize.Width;
                    _settings.FloatingHeight = e.NewSize.Height;
                    _settings.Save();
                }
            }
            ClampToViewport();
        }
        #endregion

        #region Viewport Clamping & Docking
        public void ApplySettings()
        {
            Opacity = _settings.WindowOpacity;
            ApplyTheme(_settings.Theme);
            ApplyDocking();
        }

        private void ApplyTheme(KeyboardTheme theme)
        {
            ThemeHelper.ApplyThemePalette(theme);

            switch (theme)
            {
                case KeyboardTheme.SolidFilled:
                    RootBorder.Background = (Brush)Application.Current.Resources["KbBgBrush"];
                    RootBorder.BorderBrush = (Brush)Application.Current.Resources["KbBorderBrush"];
                    RootBorder.BorderThickness = new Thickness(2);
                    RootBorder.CornerRadius = new CornerRadius(4);
                    HeaderBar.Background = (Brush)Application.Current.Resources["KbHeaderBgBrush"];
                    TopFresnelLayer.Visibility = Visibility.Collapsed;
                    DwmHelper.DisableBlur(this);
                    break;
                case KeyboardTheme.AppleGlassmorphism:
                    // Apple Liquid Glass: Crystal refractive capsule with DWM acrylic blur & top specular Fresnel highlight
                    RootBorder.Background = (Brush)Application.Current.Resources["KbBgBrush"];
                    RootBorder.BorderBrush = (Brush)Application.Current.Resources["KbBorderBrush"];
                    RootBorder.BorderThickness = new Thickness(1.5);
                    RootBorder.CornerRadius = new CornerRadius(18);
                    HeaderBar.Background = (Brush)Application.Current.Resources["KbHeaderBgBrush"];
                    TopFresnelLayer.Visibility = Visibility.Visible;
                    DwmHelper.EnableBlur(this, true, 0x30F6FAFD);
                    break;
                case KeyboardTheme.AppleTranslucentDark:
                    // Apple Obsidian Liquid Glass
                    RootBorder.Background = (Brush)Application.Current.Resources["KbBgBrush"];
                    RootBorder.BorderBrush = (Brush)Application.Current.Resources["KbBorderBrush"];
                    RootBorder.BorderThickness = new Thickness(1.5);
                    RootBorder.CornerRadius = new CornerRadius(18);
                    HeaderBar.Background = (Brush)Application.Current.Resources["KbHeaderBgBrush"];
                    TopFresnelLayer.Visibility = Visibility.Visible;
                    DwmHelper.EnableBlur(this, true, 0x60181A24);
                    break;
                case KeyboardTheme.DarkFluent:
                default:
                    RootBorder.Background = (Brush)Application.Current.Resources["KbBgBrush"];
                    RootBorder.BorderBrush = (Brush)Application.Current.Resources["KbBorderBrush"];
                    RootBorder.BorderThickness = new Thickness(2);
                    RootBorder.CornerRadius = new CornerRadius(6);
                    HeaderBar.Background = (Brush)Application.Current.Resources["KbHeaderBgBrush"];
                    TopFresnelLayer.Visibility = Visibility.Collapsed;
                    DwmHelper.DisableBlur(this);
                    break;
            }

            if (LayoutManager.Instance != null && LayoutManager.Instance.CurrentLayout != null)
            {
                RenderLayout(LayoutManager.Instance.CurrentLayout);
            }
        }

        private void ApplyDocking()
        {
            Rect workArea = SystemParameters.WorkArea;

            if (_settings.Dock == DockMode.DockBottom)
            {
                BottomDragBar.Visibility = Visibility.Collapsed;
                DragPillHandle.Visibility = Visibility.Collapsed;
                BtnDockToggle.Content = "⤓";
                BtnDockToggle.ToolTip = "Switch to Floating Mode";

                double dockHeight = Math.Max(220, Math.Min(360, workArea.Height * 0.36));
                Width = workArea.Width;
                Height = dockHeight;
                Left = workArea.Left;
                Top = workArea.Bottom - dockHeight;
            }
            else if (_settings.Dock == DockMode.DockTop)
            {
                BottomDragBar.Visibility = Visibility.Collapsed;
                DragPillHandle.Visibility = Visibility.Collapsed;
                BtnDockToggle.Content = "⤒";
                BtnDockToggle.ToolTip = "Switch to Floating Mode";

                double dockHeight = Math.Max(220, Math.Min(360, workArea.Height * 0.36));
                Width = workArea.Width;
                Height = dockHeight;
                Left = workArea.Left;
                Top = workArea.Top;
            }
            else // Floating
            {
                BottomDragBar.Visibility = Visibility.Visible;
                DragPillHandle.Visibility = Visibility.Visible;
                BtnDockToggle.Content = "⤢";
                BtnDockToggle.ToolTip = "Dock to Screen Bottom";

                double maxWidth = Math.Max(400, workArea.Width - 20);
                double maxHeight = Math.Max(200, workArea.Height - 20);
                Width = Math.Min(maxWidth, Math.Max(480, _settings.FloatingWidth > 0 ? _settings.FloatingWidth : 860));
                Height = Math.Min(maxHeight, Math.Max(200, _settings.FloatingHeight > 0 ? _settings.FloatingHeight : 310));

                double posX = Math.Max(workArea.Left + 10, Math.Min(_settings.FloatingX, workArea.Right - Width - 10));
                double posY = Math.Max(workArea.Top + 10, Math.Min(_settings.FloatingY, workArea.Bottom - Height - 10));

                Left = posX;
                Top = posY;
            }

            ClampToViewport();
            ReassertTopmost();
        }

        private void ClampToViewport()
        {
            if (_isClamping) return;
            _isClamping = true;

            try
            {
                Rect workArea = SystemParameters.WorkArea;
                double curW = ActualWidth > 50 ? ActualWidth : Width;
                double curH = ActualHeight > 50 ? ActualHeight : Height;

                if (_settings.Dock == DockMode.DockBottom)
                {
                    double expectedTop = workArea.Bottom - curH;
                    if (Math.Abs(Left - workArea.Left) > 1) Left = workArea.Left;
                    if (Math.Abs(Width - workArea.Width) > 1) Width = workArea.Width;
                    if (Math.Abs(Top - expectedTop) > 1) Top = expectedTop;
                }
                else if (_settings.Dock == DockMode.DockTop)
                {
                    if (Math.Abs(Left - workArea.Left) > 1) Left = workArea.Left;
                    if (Math.Abs(Width - workArea.Width) > 1) Width = workArea.Width;
                    if (Math.Abs(Top - workArea.Top) > 1) Top = workArea.Top;
                }
                else // Floating
                {
                    double clampedX = Math.Max(workArea.Left + 5.0, Math.Min(Left, workArea.Right - curW - 5.0));
                    double clampedY = Math.Max(workArea.Top + 5.0, Math.Min(Top, workArea.Bottom - curH - 5.0));

                    if (Math.Abs(Left - clampedX) > 1) Left = clampedX;
                    if (Math.Abs(Top - clampedY) > 1) Top = clampedY;
                }
            }
            catch { }
            finally
            {
                _isClamping = false;
            }
        }

        private void ReassertTopmost()
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    Win32.SetWindowPos(hwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0,
                        Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
                }

                if (_activeSettingsWindow != null && _activeSettingsWindow.IsVisible)
                {
                    // Ensure Settings Window stays strictly on top of keyboard!
                    IntPtr settingsHwnd = new WindowInteropHelper(_activeSettingsWindow).Handle;
                    if (settingsHwnd != IntPtr.Zero)
                    {
                        Win32.SetWindowPos(settingsHwnd, Win32.HWND_TOPMOST, 0, 0, 0, 0,
                            Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
                    }
                }
            }
            catch { }
        }
        #endregion

        #region Windows 10 Key Rendering, Dedicated Accent Tap Flash & Anti-Double-Click
        private void RenderLayout(KeyboardLayout layout)
        {
            KeyboardMatrixContainer.Children.Clear();
            KeyboardMatrixContainer.RowDefinitions.Clear();
            KeyboardMatrixContainer.ColumnDefinitions.Clear();
            _renderedKeyButtons.Clear();
            _activePressedButtons.Clear();

            if (layout == null || layout.Rows.Count == 0) return;

            for (int r = 0; r < layout.Rows.Count; r++)
            {
                KeyboardMatrixContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
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
                    var button = CreateKeyButton(keyModel);
                    Grid.SetColumn(button, c);
                    rowGrid.Children.Add(button);
                    _renderedKeyButtons.Add(button);
                }

                Grid.SetRow(rowGrid, r);
                KeyboardMatrixContainer.Children.Add(rowGrid);
            }

            UpdateModifierVisuals();
        }

        private Button CreateKeyButton(KeyModel model)
        {
            bool isSpecial = model.ActionType == KeyActionType.Modifier || 
                             model.ActionType == KeyActionType.Macro || 
                             model.VkCode == Win32.VK_BACK || 
                             model.VkCode == Win32.VK_RETURN || 
                             model.VkCode == Win32.VK_TAB || 
                             model.VkCode == Win32.VK_ESCAPE;

            var btn = new Button
            {
                Style = isSpecial ? (Style)FindResource("Win10SpecialKeyStyle") : (Style)FindResource("Win10KeyStyle"),
                Focusable = false,
                Tag = model
            };

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!string.IsNullOrEmpty(model.ShiftLabel))
            {
                var shiftTxt = new TextBlock
                {
                    Text = model.ShiftLabel,
                    FontSize = 10,
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 1)
                };
                stack.Children.Add(shiftTxt);
            }

            double fontSize = 16;
            if (model.PrimaryLabel.Length > 2) fontSize = 12;
            if (model.PrimaryLabel.Length > 6) fontSize = 11;

            var primaryTxt = new TextBlock
            {
                Text = model.PrimaryLabel,
                FontSize = fontSize,
                FontWeight = FontWeights.Normal,
                Foreground = (Brush)FindResource("TextLightBrush"),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            stack.Children.Add(primaryTxt);

            btn.Content = stack;

            // ADAPTIVE ACCENT COLOR TAP FEEDBACK HANDLERS
            btn.PreviewTouchDown += delegate(object s, TouchEventArgs e)
            {
                e.Handled = true;
                _lastTouchTick = Environment.TickCount;
                FlashAccentTap(btn, model);
                ProcessSafeKeyPress(model);

                if (model.IsRepeatable && model.ActionType != KeyActionType.Modifier)
                {
                    RepeatManager.Instance.StartRepeat(delegate { ProcessSafeKeyPress(model); });
                }
            };

            btn.PreviewTouchUp += delegate(object s, TouchEventArgs e)
            {
                e.Handled = true;
                RestoreButtonVisual(btn, model);
                RepeatManager.Instance.StopRepeat();
            };

            btn.PreviewMouseDown += delegate(object s, MouseButtonEventArgs e)
            {
                e.Handled = true;
                if (Math.Abs(Environment.TickCount - _lastTouchTick) < 300)
                {
                    return;
                }

                FlashAccentTap(btn, model);
                ProcessSafeKeyPress(model);

                if (model.IsRepeatable && model.ActionType != KeyActionType.Modifier)
                {
                    RepeatManager.Instance.StartRepeat(delegate { ProcessSafeKeyPress(model); });
                }
            };

            btn.PreviewMouseUp += delegate(object s, MouseButtonEventArgs e)
            {
                e.Handled = true;
                RestoreButtonVisual(btn, model);
                RepeatManager.Instance.StopRepeat();
            };

            btn.MouseLeave += delegate
            {
                RestoreButtonVisual(btn, model);
                RepeatManager.Instance.StopRepeat();
            };

            return btn;
        }

        private void FlashAccentTap(Button btn, KeyModel model)
        {
            try
            {
                _activePressedButtons.Add(btn);

                // Find the dedicated AccentFlashLayer inside the button template
                var flashLayer = btn.Template.FindName("AccentFlashLayer", btn) as Border;
                if (flashLayer != null)
                {
                    flashLayer.Opacity = 1.0;
                }

                // Guarantee a visible, rich 160ms flash so the human eye sees the accent color
                var flashTimer = new DispatcherTimer();
                flashTimer.Interval = TimeSpan.FromMilliseconds(160);
                flashTimer.Tick += delegate
                {
                    flashTimer.Stop();
                    if (!_activePressedButtons.Contains(btn) && !IsActiveModifier(model))
                    {
                        if (flashLayer != null)
                        {
                            flashLayer.Opacity = 0.0;
                        }
                    }
                };
                flashTimer.Start();
            }
            catch { }
        }

        private void RestoreButtonVisual(Button btn, KeyModel model)
        {
            try
            {
                _activePressedButtons.Remove(btn);

                if (!IsActiveModifier(model))
                {
                    var flashLayer = btn.Template.FindName("AccentFlashLayer", btn) as Border;
                    if (flashLayer != null)
                    {
                        flashLayer.Opacity = 0.0;
                    }
                }
            }
            catch { }
        }

        private bool IsActiveModifier(KeyModel model)
        {
            if (model == null || model.ActionType != KeyActionType.Modifier) return false;
            var state = KeyboardState.Instance;
            return (model.Tag == "Shift" && state.IsShiftActive) ||
                   (model.Tag == "Caps" && state.IsCapsLock) ||
                   (model.Tag == "Ctrl" && state.IsCtrlDown) ||
                   (model.Tag == "Alt" && state.IsAltDown) ||
                   (model.Tag == "Win" && state.IsWinDown);
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

                HandleKeyPress(key);
            }
        }

        private void HandleKeyPress(KeyModel key)
        {
            AudioManager.Instance.PlayKeySound();

            switch (key.ActionType)
            {
                case KeyActionType.Modifier:
                    HandleModifierKey(key);
                    break;

                case KeyActionType.Character:
                    HandleCharacterKey(key);
                    break;

                case KeyActionType.VirtualKey:
                case KeyActionType.Navigation:
                    KeyInjector.SendVirtualKey(key.VkCode);
                    if (key.VkCode == Win32.VK_BACK)
                    {
                        WordPredictor.Instance.OnBackspace();
                    }
                    else if (key.VkCode == Win32.VK_RETURN || key.VkCode == Win32.VK_SPACE || key.VkCode == Win32.VK_TAB)
                    {
                        WordPredictor.Instance.CommitCurrentWord();
                    }
                    break;

                case KeyActionType.Macro:
                    HandleMacro(key.MacroCommand);
                    break;
            }
        }

        private void HandleModifierKey(KeyModel key)
        {
            var state = KeyboardState.Instance;
            switch (key.Tag)
            {
                case "Shift":
                    state.ToggleShift();
                    break;
                case "Caps":
                    KeyInjector.SendVirtualKey(Win32.VK_CAPITAL);
                    state.NotifyChanged();
                    break;
                case "Ctrl":
                    state.ToggleCtrl();
                    break;
                case "Alt":
                    state.ToggleAlt();
                    break;
                case "Win":
                    state.ToggleWin();
                    break;
            }
        }

        private void HandleCharacterKey(KeyModel key)
        {
            var state = KeyboardState.Instance;

            // CRITICAL FIX FOR VIRTUAL CTRL+V (PASTE), CTRL+C (COPY), CTRL+A, ETC.
            // When Ctrl, Alt, or Win is active, we MUST send the hardware virtual key event!
            if (state.IsCtrlDown || state.IsAltDown || state.IsWinDown)
            {
                if (key.VkCode != 0)
                {
                    KeyInjector.SendVirtualKey(key.VkCode);
                }
                return;
            }

            if (key.VkCode != 0)
            {
                bool capitalize = state.ShouldCapitalize;
                char chToSend = '\0';

                if (!string.IsNullOrEmpty(key.ShiftLabel) && state.IsShiftActive)
                {
                    chToSend = key.ShiftLabel[0];
                }
                else if (!string.IsNullOrEmpty(key.PrimaryLabel))
                {
                    chToSend = capitalize ? char.ToUpper(key.PrimaryLabel[0]) : char.ToLower(key.PrimaryLabel[0]);
                }

                if (chToSend != '\0')
                {
                    KeyInjector.SendUnicodeChar(chToSend);
                    WordPredictor.Instance.AppendChar(chToSend);
                }
                else
                {
                    KeyInjector.SendVirtualKey(key.VkCode);
                }
            }
            else if (!string.IsNullOrEmpty(key.PrimaryLabel))
            {
                KeyInjector.SendText(key.PrimaryLabel);
                WordPredictor.Instance.CommitCurrentWord();
            }
        }

        private void HandleMacro(string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            if (command.StartsWith("SWITCH_LAYOUT:"))
            {
                string layoutId = command.Substring("SWITCH_LAYOUT:".Length);
                LayoutManager.Instance.SelectLayoutById(layoutId);
            }
            else if (command == "HIDE")
            {
                ToggleVisibility();
            }
            else if (command == "VOICE_INPUT")
            {
                VoiceInputManager.Instance.ToggleListening();
            }
        }

        private void UpdateModifierVisuals()
        {
            var state = KeyboardState.Instance;
            bool capitalize = state.ShouldCapitalize;

            foreach (var btn in _renderedKeyButtons)
            {
                var model = btn.Tag as KeyModel;
                if (model == null) continue;

                if (model.ActionType == KeyActionType.Character && model.PrimaryLabel.Length == 1 && char.IsLetter(model.PrimaryLabel[0]))
                {
                    var sp = btn.Content as StackPanel;
                    if (sp != null && sp.Children.Count > 0)
                    {
                        var tb = sp.Children[sp.Children.Count - 1] as TextBlock;
                        if (tb != null)
                        {
                            tb.Text = capitalize ? model.PrimaryLabel.ToUpper() : model.PrimaryLabel.ToLower();
                        }
                    }
                }
                else if (model.ActionType == KeyActionType.Modifier)
                {
                    bool active = IsActiveModifier(model);

                    // Illuminate modifier in Windows Accent Color via AccentFlashLayer
                    var flashLayer = btn.Template.FindName("AccentFlashLayer", btn) as Border;
                    if (flashLayer != null)
                    {
                        flashLayer.Opacity = active ? 1.0 : 0.0;
                    }
                }
            }
        }
        #endregion

        #region Word Prediction (Windows 10 Style)
        private void UpdateSuggestionsUI(IReadOnlyList<string> suggestions)
        {
            SuggestionsContainer.Items.Clear();

            if (suggestions == null || suggestions.Count == 0) return;

            foreach (var word in suggestions)
            {
                var currentWord = word;
                var pill = new Button
                {
                    Content = currentWord,
                    Style = (Style)FindResource("Win10SuggestionStyle")
                };

                pill.Click += delegate(object s, RoutedEventArgs e)
                {
                    ApplySuggestion(currentWord);
                };

                SuggestionsContainer.Items.Add(pill);
            }
        }

        private void ApplySuggestion(string word)
        {
            string currentPrefix = WordPredictor.Instance.CurrentPrefix;
            AudioManager.Instance.PlayKeySound();

            if (!string.IsNullOrEmpty(currentPrefix) && word.StartsWith(currentPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string remaining = word.Substring(currentPrefix.Length) + " ";
                KeyInjector.SendText(remaining);
            }
            else
            {
                for (int i = 0; i < currentPrefix.Length; i++)
                {
                    KeyInjector.SendVirtualKey(Win32.VK_BACK);
                }
                KeyInjector.SendText(word + " ");
            }

            WordPredictor.Instance.CommitCurrentWord();
            WordPredictor.Instance.LearnWord(word);
        }
        #endregion

        #region Focus Tracking & Auto-Hide on Non-Input Area
        private void OnFocusTrackerChanged(object sender, FocusChangedEventArgs e)
        {
            if (!_settings.AutoShowOnFocus) return;
            if (_activeSettingsWindow != null && _activeSettingsWindow.IsVisible) return;

            if (e.IsTextField)
            {
                // Touch & Click Detection:
                // If keyboard is visible, update position for the newly focused field.
                // If keyboard is collapsed, check if this was triggered by active user interaction (touch/click within 750ms)
                bool shouldShow = false;
                if (Visibility == Visibility.Visible)
                {
                    shouldShow = true;
                }
                else
                {
                    // Touch screen tap trigger: user touched an input box on touchscreen!
                    if (Win32.HasRecentUserInput(750))
                    {
                        shouldShow = true;
                    }
                }

                if (shouldShow)
                {
                    _autoHideTimer.Stop();

                    if (Visibility != Visibility.Visible)
                    {
                        Visibility = Visibility.Visible;
                    }

                    if (_settings.Dock == DockMode.Floating && !e.BoundingRect.IsEmpty)
                    {
                        double curW = ActualWidth > 50 ? ActualWidth : Width;
                        double curH = ActualHeight > 50 ? ActualHeight : Height;
                        var optimalPos = CaretTracker.CalculateOptimalWindowPosition(e.BoundingRect, curW, curH);
                        Left = optimalPos.X;
                        Top = optimalPos.Y;
                    }
                    else if (_settings.Dock == DockMode.DockBottom)
                    {
                        var workArea = SystemParameters.WorkArea;
                        double curH = ActualHeight > 50 ? ActualHeight : Height;
                        Win32.RECT startRect;
                        if (Win32.IsStartMenuOrSearchOpen(out startRect) || (!e.BoundingRect.IsEmpty && e.BoundingRect.Bottom > workArea.Bottom - 500 && e.BoundingRect.Top > workArea.Top + 200))
                        {
                            Top = workArea.Top;
                        }
                        else
                        {
                            Top = workArea.Bottom - curH;
                        }
                    }

                    ClampToViewport();
                    ReassertTopmost();
                }
            }
            else
            {
                // Non-input area focused: auto-hide debounced timer only if currently visible
                if (Visibility == Visibility.Visible)
                {
                    _autoHideTimer.Stop();
                    _autoHideTimer.Start();
                }
            }
        }

        private void OnLockStateChanged(object sender, bool isLocked)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (isLocked)
                {
                    LockBadge.Visibility = Visibility.Visible;
                }
                else
                {
                    LockBadge.Visibility = Visibility.Collapsed;
                }
            }));
        }
        #endregion

        #region Resize Tool & Presets
        private void BtnResize_Click(object sender, RoutedEventArgs e)
        {
            _isResizeMode = !_isResizeMode;
            ResizeTray.Visibility = _isResizeMode ? Visibility.Visible : Visibility.Collapsed;
            BtnResize.Background = _isResizeMode 
                ? (Brush)Application.Current.Resources["AccentBrush"] 
                : Brushes.Transparent;
        }

        private void BtnSizePreset_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.Tag != null)
            {
                string preset = btn.Tag.ToString();
                var workArea = SystemParameters.WorkArea;

                if (_settings.Dock == DockMode.Floating)
                {
                    switch (preset)
                    {
                        case "compact":
                            Width = 720;
                            Height = 250;
                            break;
                        case "large":
                            Width = 1000;
                            Height = 360;
                            break;
                        case "wide":
                            Width = Math.Min(1180, workArea.Width - 40);
                            Height = 400;
                            break;
                        case "standard":
                        default:
                            Width = 860;
                            Height = 310;
                            break;
                    }
                    _settings.FloatingWidth = Width;
                    _settings.FloatingHeight = Height;
                }
                else
                {
                    switch (preset)
                    {
                        case "compact":
                            Height = 240;
                            break;
                        case "large":
                            Height = 340;
                            break;
                        case "wide":
                            Height = 390;
                            break;
                        case "standard":
                        default:
                            Height = 290;
                            break;
                    }
                    Top = workArea.Bottom - Height;
                }

                _settings.Save();
                ClampToViewport();
            }
        }

        private void BtnStepSize_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            string action = btn.Tag.ToString();
            var workArea = SystemParameters.WorkArea;
            double curW = ActualWidth > 50 ? ActualWidth : Width;
            double curH = ActualHeight > 50 ? ActualHeight : Height;

            if (action == "inc")
            {
                curW = Math.Min(workArea.Width - 20, curW * 1.10);
                curH = Math.Min(workArea.Height - 40, curH * 1.10);
            }
            else if (action == "dec")
            {
                curW = Math.Max(480, curW * 0.90);
                curH = Math.Max(200, curH * 0.90);
            }

            Width = curW;
            Height = curH;

            if (_settings.Dock == DockMode.Floating)
            {
                _settings.FloatingWidth = curW;
                _settings.FloatingHeight = curH;
            }

            _settings.Save();
            ClampToViewport();
        }

        private void BtnResizeDone_Click(object sender, RoutedEventArgs e)
        {
            _isResizeMode = false;
            ResizeTray.Visibility = Visibility.Collapsed;
            BtnResize.Background = Brushes.Transparent;
            _settings.Save();
        }
        #endregion

        #region Window Interactions & Controls
        public void ToggleVisibility()
        {
            if (Visibility == Visibility.Visible)
            {
                Visibility = Visibility.Collapsed;
            }
            else
            {
                Visibility = Visibility.Visible;
                ClampToViewport();
                ReassertTopmost();
            }
        }

        private void HeaderBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_settings.Dock == DockMode.Floating)
            {
                try
                {
                    DragMove();
                    ClampToViewport();
                    _settings.FloatingX = Left;
                    _settings.FloatingY = Top;
                    _settings.Save();
                }
                catch { }
            }
        }

        private void BtnDismiss_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void BtnVoiceInput_Click(object sender, RoutedEventArgs e)
        {
            VoiceInputManager.Instance.ToggleListening();
        }

        private void BtnStopVoice_Click(object sender, RoutedEventArgs e)
        {
            VoiceInputManager.Instance.StopListening();
        }

        private void UpdateVoiceInputUI(bool isListening)
        {
            VoiceIndicatorBanner.Visibility = isListening ? Visibility.Visible : Visibility.Collapsed;
            SuggestionsContainer.Visibility = isListening ? Visibility.Collapsed : Visibility.Visible;

            if (isListening)
            {
                BtnVoiceInput.Background = (Brush)Application.Current.Resources["AccentBrush"];
                BtnVoiceInput.Foreground = Brushes.White;
            }
            else
            {
                BtnVoiceInput.ClearValue(BackgroundProperty);
                BtnVoiceInput.ClearValue(ForegroundProperty);
            }
        }

        private void BtnDockToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_settings.Dock == DockMode.DockBottom)
            {
                _settings.Dock = DockMode.Floating;
            }
            else
            {
                _settings.Dock = DockMode.DockBottom;
            }

            _settings.Save();
            ApplyDocking();
        }

        private void BtnLayoutMenu_Click(object sender, RoutedEventArgs e)
        {
            HeaderBar.ContextMenu.IsOpen = true;
        }

        private void MenuItemLayout_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            string tag = mi != null && mi.Tag != null ? mi.Tag.ToString() : null;
            if (!string.IsNullOrEmpty(tag))
            {
                LayoutManager.Instance.SelectLayoutById(tag);
                _settings.DefaultLayoutId = tag;
                _settings.Save();
            }
        }

        private void MenuDockBottom_Click(object sender, RoutedEventArgs e)
        {
            _settings.Dock = DockMode.DockBottom;
            _settings.Save();
            ApplyDocking();
        }

        private void MenuDockFloat_Click(object sender, RoutedEventArgs e)
        {
            _settings.Dock = DockMode.Floating;
            _settings.Save();
            ApplyDocking();
        }

        private void MenuItemTheme_Click(object sender, RoutedEventArgs e)
        {
            var mi = sender as MenuItem;
            if (mi != null && mi.Tag != null)
            {
                try
                {
                    KeyboardTheme theme = (KeyboardTheme)Enum.Parse(typeof(KeyboardTheme), mi.Tag.ToString());
                    _settings.Theme = theme;
                    _settings.Save();
                    ApplyTheme(theme);
                }
                catch { }
            }
        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (_activeSettingsWindow != null && _activeSettingsWindow.IsLoaded)
            {
                _activeSettingsWindow.Activate();
                return;
            }

            _activeSettingsWindow = new SettingsWindow(_settings, this);
            _activeSettingsWindow.Owner = this;
            _activeSettingsWindow.Topmost = true;

            var workArea = SystemParameters.WorkArea;
            _activeSettingsWindow.Left = Math.Max(workArea.Left + 20, (workArea.Width - _activeSettingsWindow.Width) / 2.0);
            _activeSettingsWindow.Top = Math.Max(workArea.Top + 20, Math.Min(Top - _activeSettingsWindow.Height - 10, workArea.Top + 40));

            _activeSettingsWindow.Closed += delegate
            {
                _activeSettingsWindow = null;
                ReassertTopmost();
            };

            _activeSettingsWindow.Show();
        }
        #endregion
    }
}
