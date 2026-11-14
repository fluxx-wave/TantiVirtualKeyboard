using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using HotVirtualKeyboard.Automation;
using HotVirtualKeyboard.Config;
using HotVirtualKeyboard.Core;
using HotVirtualKeyboard.Layouts;
using HotVirtualKeyboard.LockScreen;

namespace HotVirtualKeyboard
{
    public partial class SettingsWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly MainWindow _mainWindow;
        private bool _isLoading = true;

        public SettingsWindow(AppSettings settings, MainWindow mainWindow = null)
        {
            _settings = settings;
            _mainWindow = mainWindow;
            InitializeComponent();

            _isLoading = true;
            LoadSettingsIntoUI();
            _isLoading = false;

            WireRealtimeEvents();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch { }
            }
        }

        private void WireRealtimeEvents()
        {
            // Real-time Theme Updates
            RbThemeDarkFluent.Checked += delegate { if (!_isLoading) ApplyThemeRealtime(KeyboardTheme.DarkFluent); };
            RbThemeSolidFilled.Checked += delegate { if (!_isLoading) ApplyThemeRealtime(KeyboardTheme.SolidFilled); };
            RbThemeAppleGlassmorphism.Checked += delegate { if (!_isLoading) ApplyThemeRealtime(KeyboardTheme.AppleGlassmorphism); };
            RbThemeAppleTranslucentDark.Checked += delegate { if (!_isLoading) ApplyThemeRealtime(KeyboardTheme.AppleTranslucentDark); };

            // Real-time Docking Updates
            RbDockBottom.Checked += delegate { if (!_isLoading) ApplyDockRealtime(DockMode.DockBottom); };
            RbDockTop.Checked += delegate { if (!_isLoading) ApplyDockRealtime(DockMode.DockTop); };
            RbDockFloating.Checked += delegate { if (!_isLoading) ApplyDockRealtime(DockMode.Floating); };

            // Real-time Layout Updates
            CbDefaultLayout.SelectionChanged += delegate
            {
                if (!_isLoading)
                {
                    var selectedItem = CbDefaultLayout.SelectedItem as ComboBoxItem;
                    if (selectedItem != null && selectedItem.Tag != null)
                    {
                        string tag = selectedItem.Tag.ToString();
                        _settings.DefaultLayoutId = tag;
                        LayoutManager.Instance.SelectLayoutById(tag);
                    }
                }
            };

            // Real-time Key Repeat Updates
            SliderRepeatDelay.ValueChanged += delegate
            {
                if (!_isLoading)
                {
                    _settings.KeyRepeatDelayMs = (int)SliderRepeatDelay.Value;
                    RepeatManager.Instance.InitialDelayMs = _settings.KeyRepeatDelayMs;
                }
            };

            SliderRepeatInterval.ValueChanged += delegate
            {
                if (!_isLoading)
                {
                    _settings.KeyRepeatIntervalMs = (int)SliderRepeatInterval.Value;
                    RepeatManager.Instance.RepeatIntervalMs = _settings.KeyRepeatIntervalMs;
                }
            };

            // Real-time Toggles
            ChkAutoShow.Checked += delegate { if (!_isLoading) { _settings.AutoShowOnFocus = true; FocusTracker.Instance.IsAutoShowEnabled = true; } };
            ChkAutoShow.Unchecked += delegate { if (!_isLoading) { _settings.AutoShowOnFocus = false; FocusTracker.Instance.IsAutoShowEnabled = false; } };

            ChkSound.Checked += delegate
            {
                if (!_isLoading)
                {
                    _settings.SoundFeedback = true;
                    AudioManager.Instance.IsSoundEnabled = true;
                    AudioManager.Instance.PlayKeySound();
                }
            };
            ChkSound.Unchecked += delegate
            {
                if (!_isLoading)
                {
                    _settings.SoundFeedback = false;
                    AudioManager.Instance.IsSoundEnabled = false;
                }
            };

            ChkLockScreen.Checked += delegate
            {
                if (!_isLoading)
                {
                    _settings.LockScreenIntegration = true;
                    LockScreenManager.Instance.IsLockScreenIntegrationEnabled = true;
                }
            };
            ChkLockScreen.Unchecked += delegate
            {
                if (!_isLoading)
                {
                    _settings.LockScreenIntegration = false;
                    LockScreenManager.Instance.IsLockScreenIntegrationEnabled = false;
                }
            };
        }

        private void ApplyThemeRealtime(KeyboardTheme theme)
        {
            _settings.Theme = theme;
            if (_mainWindow != null)
            {
                _mainWindow.ApplySettings();
            }
        }

        private void ApplyDockRealtime(DockMode dock)
        {
            _settings.Dock = dock;
            if (_mainWindow != null)
            {
                _mainWindow.ApplySettings();
            }
        }

        private void NavBtn_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelTyping == null || PanelAppearance == null || PanelLockScreen == null) return;

            PanelTyping.Visibility = NavBtnTyping.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelAppearance.Visibility = NavBtnAppearance.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelLockScreen.Visibility = NavBtnLockScreen.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadSettingsIntoUI()
        {
            // Theme
            switch (_settings.Theme)
            {
                case KeyboardTheme.SolidFilled:
                    RbThemeSolidFilled.IsChecked = true;
                    break;
                case KeyboardTheme.AppleGlassmorphism:
                    RbThemeAppleGlassmorphism.IsChecked = true;
                    break;
                case KeyboardTheme.AppleTranslucentDark:
                    RbThemeAppleTranslucentDark.IsChecked = true;
                    break;
                case KeyboardTheme.DarkFluent:
                default:
                    RbThemeDarkFluent.IsChecked = true;
                    break;
            }

            // General & Typing
            ChkAutoShow.IsChecked = _settings.AutoShowOnFocus;
            ChkSound.IsChecked = _settings.SoundFeedback;
            SliderRepeatDelay.Value = _settings.KeyRepeatDelayMs;
            SliderRepeatInterval.Value = _settings.KeyRepeatIntervalMs;

            // Docking
            switch (_settings.Dock)
            {
                case DockMode.DockTop:
                    RbDockTop.IsChecked = true;
                    break;
                case DockMode.Floating:
                    RbDockFloating.IsChecked = true;
                    break;
                case DockMode.DockBottom:
                default:
                    RbDockBottom.IsChecked = true;
                    break;
            }

            // Layout
            foreach (ComboBoxItem item in CbDefaultLayout.Items)
            {
                string tag = item.Tag != null ? item.Tag.ToString() : null;
                if (string.Equals(tag, _settings.DefaultLayoutId, StringComparison.OrdinalIgnoreCase))
                {
                    CbDefaultLayout.SelectedItem = item;
                    break;
                }
            }

            // Lock screen
            ChkLockScreen.IsChecked = _settings.LockScreenIntegration;
            UpdateOskStatus();
        }

        private void BtnTestSound_Click(object sender, RoutedEventArgs e)
        {
            AudioManager.Instance.PlayKeySound();
        }

        private void BtnTestVoice_Click(object sender, RoutedEventArgs e)
        {
            VoiceInputManager.Instance.StartListening();
        }

        private void BtnTestTapAccent_Click(object sender, RoutedEventArgs e)
        {
            AudioManager.Instance.PlayKeySound();
            Brush accent = Application.Current.Resources["AccentBrush"] as Brush;
            if (accent != null)
            {
                BtnTestTapAccent.Background = accent;
                BtnTestTapAccent.Foreground = Brushes.White;

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(200);
                timer.Tick += delegate
                {
                    timer.Stop();
                    BtnTestTapAccent.ClearValue(BackgroundProperty);
                    BtnTestTapAccent.ClearValue(ForegroundProperty);
                };
                timer.Start();
            }
        }

        private void BtnPresetFromSettings_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null || btn.Tag == null) return;

            string preset = btn.Tag.ToString();
            switch (preset)
            {
                case "compact":
                    _settings.FloatingWidth = 720;
                    _settings.FloatingHeight = 250;
                    break;
                case "large":
                    _settings.FloatingWidth = 1000;
                    _settings.FloatingHeight = 360;
                    break;
                case "wide":
                    _settings.FloatingWidth = 1180;
                    _settings.FloatingHeight = 400;
                    break;
                case "standard":
                default:
                    _settings.FloatingWidth = 860;
                    _settings.FloatingHeight = 310;
                    break;
            }

            TxtAccentStatus.Text = string.Format("Preset set to {0}x{1}", (int)_settings.FloatingWidth, (int)_settings.FloatingHeight);

            if (_mainWindow != null)
            {
                if (_settings.Dock == DockMode.Floating)
                {
                    _mainWindow.Width = _settings.FloatingWidth;
                    _mainWindow.Height = _settings.FloatingHeight;
                }
                _mainWindow.ApplySettings();
            }
        }

        private void UpdateOskStatus()
        {
            bool isReg = EaseOfAccessHelper.IsRegisteredForEaseOfAccess();
            TxtOskStatus.Text = isReg ? "✓ Registered as Windows System On-Screen Keyboard" : "Not registered as Windows System On-Screen Keyboard";
            TxtOskStatus.Foreground = isReg 
                ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) 
                : new SolidColorBrush(Color.FromRgb(148, 163, 184));
        }

        private void BtnRegisterOsk_Click(object sender, RoutedEventArgs e)
        {
            bool ok = EaseOfAccessHelper.RegisterAsDefaultOsk();
            if (ok)
            {
                MessageBox.Show(
                    "Hot Virtual Keyboard successfully registered as the Windows Ease of Access On-Screen Keyboard!\n\nYou can now activate it from the Windows Lock Screen via the Accessibility menu.",
                    "Ease of Access Registration",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Registration requires Administrator permissions. Please run as Administrator or run setup_lockscreen.ps1.",
                    "Permission Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            UpdateOskStatus();
        }

        private void BtnUnregisterOsk_Click(object sender, RoutedEventArgs e)
        {
            EaseOfAccessHelper.UnregisterFromEaseOfAccess();
            UpdateOskStatus();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (RbThemeSolidFilled.IsChecked == true) _settings.Theme = KeyboardTheme.SolidFilled;
            else if (RbThemeAppleGlassmorphism.IsChecked == true) _settings.Theme = KeyboardTheme.AppleGlassmorphism;
            else if (RbThemeAppleTranslucentDark.IsChecked == true) _settings.Theme = KeyboardTheme.AppleTranslucentDark;
            else _settings.Theme = KeyboardTheme.DarkFluent;

            _settings.AutoShowOnFocus = ChkAutoShow.IsChecked == true;
            _settings.SoundFeedback = ChkSound.IsChecked == true;
            _settings.KeyRepeatDelayMs = (int)SliderRepeatDelay.Value;
            _settings.KeyRepeatIntervalMs = (int)SliderRepeatInterval.Value;

            if (RbDockTop.IsChecked == true) _settings.Dock = DockMode.DockTop;
            else if (RbDockFloating.IsChecked == true) _settings.Dock = DockMode.Floating;
            else _settings.Dock = DockMode.DockBottom;

            var selectedItem = CbDefaultLayout.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                _settings.DefaultLayoutId = selectedItem.Tag != null ? selectedItem.Tag.ToString() : "touch";
            }

            _settings.LockScreenIntegration = ChkLockScreen.IsChecked == true;

            // Apply immediately to running systems
            FocusTracker.Instance.IsAutoShowEnabled = _settings.AutoShowOnFocus;
            AudioManager.Instance.IsSoundEnabled = _settings.SoundFeedback;
            RepeatManager.Instance.InitialDelayMs = _settings.KeyRepeatDelayMs;
            RepeatManager.Instance.RepeatIntervalMs = _settings.KeyRepeatIntervalMs;
            LockScreenManager.Instance.IsLockScreenIntegrationEnabled = _settings.LockScreenIntegration;

            _settings.Save();
            if (_mainWindow != null)
            {
                _mainWindow.ApplySettings();
            }
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            if (_mainWindow != null)
            {
                _mainWindow.ApplySettings();
            }
            Close();
        }
    }
}
