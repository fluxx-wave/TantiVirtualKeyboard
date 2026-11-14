using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using HotVirtualKeyboard.Config;

namespace HotVirtualKeyboard.Core
{
    public static class ThemeHelper
    {
        public static Color GetWindowsAccentColor()
        {
            // 1. Try registry DWM AccentColor (DWORD ABGR in Windows 10/11)
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AccentColor");
                        if (val != null)
                        {
                            uint abgr = Convert.ToUInt32(val);
                            byte a = (byte)((abgr >> 24) & 0xFF);
                            byte b = (byte)((abgr >> 16) & 0xFF);
                            byte g = (byte)((abgr >> 8) & 0xFF);
                            byte r = (byte)(abgr & 0xFF);

                            if (a == 0) a = 255;
                            // Check if valid color (not pitch black)
                            if (r > 10 || g > 10 || b > 10)
                            {
                                return Color.FromArgb(a, r, g, b);
                            }
                        }
                    }
                }
            }
            catch { }

            // 2. Try Explorer AccentColorMenu
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent"))
                {
                    if (key != null)
                    {
                        object val = key.GetValue("AccentColorMenu");
                        if (val != null)
                        {
                            uint abgr = Convert.ToUInt32(val);
                            byte a = (byte)((abgr >> 24) & 0xFF);
                            byte b = (byte)((abgr >> 16) & 0xFF);
                            byte g = (byte)((abgr >> 8) & 0xFF);
                            byte r = (byte)(abgr & 0xFF);

                            if (a == 0) a = 255;
                            if (r > 10 || g > 10 || b > 10)
                            {
                                return Color.FromArgb(a, r, g, b);
                            }
                        }
                    }
                }
            }
            catch { }

            // 3. Try SystemParameters.WindowGlassColor
            try
            {
                Color c = SystemParameters.WindowGlassColor;
                if (c.R > 10 || c.G > 10 || c.B > 10)
                {
                    return Color.FromArgb(255, c.R, c.G, c.B);
                }
            }
            catch { }

            // Fallback Windows 10 Blue
            return Color.FromRgb(0, 120, 215);
        }

        public static void ApplyAccentColorToResources()
        {
            try
            {
                Color accent = GetWindowsAccentColor();
                var brush = new SolidColorBrush(accent);
                brush.Freeze();

                Application.Current.Resources["AccentBrush"] = brush;
                Application.Current.Resources["KeyPressedBrush"] = brush;
                Application.Current.Resources["KeyActiveBrush"] = brush;

                // Create lighter hover tint
                byte hoverR = (byte)Math.Min(255, accent.R + 30);
                byte hoverG = (byte)Math.Min(255, accent.G + 30);
                byte hoverB = (byte)Math.Min(255, accent.B + 30);
                var hoverBrush = new SolidColorBrush(Color.FromArgb(255, hoverR, hoverG, hoverB));
                hoverBrush.Freeze();
                Application.Current.Resources["AccentHoverBrush"] = hoverBrush;

                // Create gentle translucent accent for badges and selections
                var glowBrush = new SolidColorBrush(Color.FromArgb(60, accent.R, accent.G, accent.B));
                glowBrush.Freeze();
                Application.Current.Resources["AccentGlowBrush"] = glowBrush;
            }
            catch { }
        }

        public static void ApplyThemePalette(KeyboardTheme theme)
        {
            try
            {
                Color winAccent = GetWindowsAccentColor();
                var accentBrush = new SolidColorBrush(winAccent);
                accentBrush.Freeze();

                switch (theme)
                {
                    case KeyboardTheme.SolidFilled:
                        // Solid Filled Matte Dark
                        SetResource("KbBgBrush", new SolidColorBrush(Color.FromRgb(16, 16, 18)));
                        SetResource("KbHeaderBgBrush", new SolidColorBrush(Color.FromRgb(24, 24, 28)));
                        SetResource("KbBorderBrush", new SolidColorBrush(Color.FromRgb(46, 46, 56)));
                        SetResource("KeyNormalBrush", new SolidColorBrush(Color.FromRgb(36, 36, 44)));
                        SetResource("KeySpecialBrush", new SolidColorBrush(Color.FromRgb(28, 28, 34)));
                        SetResource("KeyHoverBrush", new SolidColorBrush(Color.FromRgb(52, 52, 62)));
                        SetResource("TextLightBrush", new SolidColorBrush(Color.FromRgb(240, 240, 240)));
                        SetResource("TextMutedBrush", new SolidColorBrush(Color.FromRgb(140, 140, 150)));
                        SetResource("KeyBorderBrush", new SolidColorBrush(Color.FromRgb(55, 55, 68)));
                        SetResource("AccentBrush", accentBrush);
                        Application.Current.Resources["KeyCornerRadius"] = new CornerRadius(4);
                        Application.Current.Resources["KeyBorderThickness"] = new Thickness(1);
                        Application.Current.Resources["KeySheenOpacity"] = 0.0;
                        break;

                    case KeyboardTheme.AppleGlassmorphism:
                        // Apple Liquid Glass (iOS Liquid Crystal Glass Aesthetic)
                        var appleBlue = new SolidColorBrush(Color.FromRgb(0, 122, 255));
                        appleBlue.Freeze();

                        var appleHoverBlue = new SolidColorBrush(Color.FromRgb(10, 132, 255));
                        appleHoverBlue.Freeze();

                        var appleGlow = new SolidColorBrush(Color.FromArgb(60, 0, 122, 255));
                        appleGlow.Freeze();

                        // 1. Liquid Glass Shell Background: Multi-stop liquid crystal dispersion
                        var liquidShell = new LinearGradientBrush();
                        liquidShell.StartPoint = new Point(0.0, 0.0);
                        liquidShell.EndPoint = new Point(1.0, 1.0);
                        liquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(242, 246, 250, 254), 0.0));
                        liquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(228, 238, 244, 251), 0.4));
                        liquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(215, 230, 239, 248), 0.8));
                        liquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(205, 222, 233, 245), 1.0));
                        liquidShell.Freeze();
                        SetResource("KbBgBrush", liquidShell);

                        // 2. Crystalline Specular Rim Border: Pure white top reflection down to ice rim
                        var liquidRim = new LinearGradientBrush();
                        liquidRim.StartPoint = new Point(0.0, 0.0);
                        liquidRim.EndPoint = new Point(0.0, 1.0);
                        liquidRim.GradientStops.Add(new GradientStop(Color.FromArgb(240, 255, 255, 255), 0.0));
                        liquidRim.GradientStops.Add(new GradientStop(Color.FromArgb(140, 255, 255, 255), 0.5));
                        liquidRim.GradientStops.Add(new GradientStop(Color.FromArgb(90, 180, 200, 225), 1.0));
                        liquidRim.Freeze();
                        SetResource("KbBorderBrush", liquidRim);

                        // 3. Liquid Header Bar: Glossy refractive crystal command bar
                        var liquidHeader = new LinearGradientBrush();
                        liquidHeader.StartPoint = new Point(0.0, 0.0);
                        liquidHeader.EndPoint = new Point(0.0, 1.0);
                        liquidHeader.GradientStops.Add(new GradientStop(Color.FromArgb(220, 245, 248, 253), 0.0));
                        liquidHeader.GradientStops.Add(new GradientStop(Color.FromArgb(185, 225, 235, 246), 1.0));
                        liquidHeader.Freeze();
                        SetResource("KbHeaderBgBrush", liquidHeader);

                        // 4. Floating Liquid Pebble Keys: Specular pearl sheen to liquid glass body
                        var liquidKey = new LinearGradientBrush();
                        liquidKey.StartPoint = new Point(0.0, 0.0);
                        liquidKey.EndPoint = new Point(0.0, 1.0);
                        liquidKey.GradientStops.Add(new GradientStop(Color.FromArgb(250, 255, 255, 255), 0.0));
                        liquidKey.GradientStops.Add(new GradientStop(Color.FromArgb(235, 250, 252, 254), 0.45));
                        liquidKey.GradientStops.Add(new GradientStop(Color.FromArgb(210, 235, 243, 250), 1.0));
                        liquidKey.Freeze();
                        SetResource("KeyNormalBrush", liquidKey);

                        // 5. Special Function Keys: Frosted smoky liquid crystal
                        var liquidSpecialKey = new LinearGradientBrush();
                        liquidSpecialKey.StartPoint = new Point(0.0, 0.0);
                        liquidSpecialKey.EndPoint = new Point(0.0, 1.0);
                        liquidSpecialKey.GradientStops.Add(new GradientStop(Color.FromArgb(220, 230, 236, 245), 0.0));
                        liquidSpecialKey.GradientStops.Add(new GradientStop(Color.FromArgb(195, 215, 226, 238), 0.5));
                        liquidSpecialKey.GradientStops.Add(new GradientStop(Color.FromArgb(175, 198, 214, 230), 1.0));
                        liquidSpecialKey.Freeze();
                        SetResource("KeySpecialBrush", liquidSpecialKey);

                        // 6. Key Hover: Brilliant liquid specular bloom
                        var liquidHover = new LinearGradientBrush();
                        liquidHover.StartPoint = new Point(0.0, 0.0);
                        liquidHover.EndPoint = new Point(0.0, 1.0);
                        liquidHover.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.0));
                        liquidHover.GradientStops.Add(new GradientStop(Color.FromArgb(245, 250, 255, 255), 0.5));
                        liquidHover.GradientStops.Add(new GradientStop(Color.FromArgb(225, 240, 252, 255), 1.0));
                        liquidHover.Freeze();
                        SetResource("KeyHoverBrush", liquidHover);

                        // 7. Crystalline Glass Bezel: Sharp top white highlight with soft bottom shadow
                        var liquidKeyBorder = new LinearGradientBrush();
                        liquidKeyBorder.StartPoint = new Point(0.0, 0.0);
                        liquidKeyBorder.EndPoint = new Point(0.0, 1.0);
                        liquidKeyBorder.GradientStops.Add(new GradientStop(Color.FromArgb(240, 255, 255, 255), 0.0));
                        liquidKeyBorder.GradientStops.Add(new GradientStop(Color.FromArgb(90, 160, 185, 210), 1.0));
                        liquidKeyBorder.Freeze();
                        SetResource("KeyBorderBrush", liquidKeyBorder);

                        // 8. Typography & Accents
                        SetResource("TextLightBrush", new SolidColorBrush(Color.FromRgb(24, 24, 32)));
                        SetResource("TextMutedBrush", new SolidColorBrush(Color.FromRgb(105, 110, 120)));
                        SetResource("AccentBrush", appleBlue);
                        SetResource("AccentHoverBrush", appleHoverBlue);
                        SetResource("AccentGlowBrush", appleGlow);

                        // 9. Pill-round keys with 1px border and liquid sheen
                        Application.Current.Resources["KeyCornerRadius"] = new CornerRadius(8);
                        Application.Current.Resources["KeyBorderThickness"] = new Thickness(1);
                        Application.Current.Resources["KeySheenOpacity"] = 0.75;
                        break;

                    case KeyboardTheme.AppleTranslucentDark:
                        // Apple Dark Liquid Glass (macOS Liquid Obsidian Glass)
                        var appleAzure = new SolidColorBrush(Color.FromRgb(10, 132, 255));
                        appleAzure.Freeze();

                        var appleHoverAzure = new SolidColorBrush(Color.FromRgb(56, 155, 255));
                        appleHoverAzure.Freeze();

                        var appleDarkGlow = new SolidColorBrush(Color.FromArgb(60, 10, 132, 255));
                        appleDarkGlow.Freeze();

                        // 1. Dark Liquid Obsidian Shell: Multi-stop smoky gradient
                        var darkLiquidShell = new LinearGradientBrush();
                        darkLiquidShell.StartPoint = new Point(0.0, 0.0);
                        darkLiquidShell.EndPoint = new Point(1.0, 1.0);
                        darkLiquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(220, 34, 38, 52), 0.0));
                        darkLiquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(195, 24, 27, 38), 0.5));
                        darkLiquidShell.GradientStops.Add(new GradientStop(Color.FromArgb(180, 16, 18, 26), 1.0));
                        darkLiquidShell.Freeze();
                        SetResource("KbBgBrush", darkLiquidShell);

                        // 2. Specular Rim Border: Crystal edge highlight
                        var darkLiquidRim = new LinearGradientBrush();
                        darkLiquidRim.StartPoint = new Point(0.0, 0.0);
                        darkLiquidRim.EndPoint = new Point(0.0, 1.0);
                        darkLiquidRim.GradientStops.Add(new GradientStop(Color.FromArgb(90, 255, 255, 255), 0.0));
                        darkLiquidRim.GradientStops.Add(new GradientStop(Color.FromArgb(30, 255, 255, 255), 1.0));
                        darkLiquidRim.Freeze();
                        SetResource("KbBorderBrush", darkLiquidRim);

                        // 3. Header Bar
                        var darkLiquidHeader = new LinearGradientBrush();
                        darkLiquidHeader.StartPoint = new Point(0.0, 0.0);
                        darkLiquidHeader.EndPoint = new Point(0.0, 1.0);
                        darkLiquidHeader.GradientStops.Add(new GradientStop(Color.FromArgb(185, 42, 46, 62), 0.0));
                        darkLiquidHeader.GradientStops.Add(new GradientStop(Color.FromArgb(145, 28, 32, 44), 1.0));
                        darkLiquidHeader.Freeze();
                        SetResource("KbHeaderBgBrush", darkLiquidHeader);

                        // 4. Floating Liquid Smoky Keys
                        var darkLiquidKey = new LinearGradientBrush();
                        darkLiquidKey.StartPoint = new Point(0.0, 0.0);
                        darkLiquidKey.EndPoint = new Point(0.0, 1.0);
                        darkLiquidKey.GradientStops.Add(new GradientStop(Color.FromArgb(115, 80, 90, 112), 0.0));
                        darkLiquidKey.GradientStops.Add(new GradientStop(Color.FromArgb(82, 58, 66, 84), 0.5));
                        darkLiquidKey.GradientStops.Add(new GradientStop(Color.FromArgb(58, 38, 46, 60), 1.0));
                        darkLiquidKey.Freeze();
                        SetResource("KeyNormalBrush", darkLiquidKey);

                        // 5. Special Keys
                        var darkSpecialKey = new LinearGradientBrush();
                        darkSpecialKey.StartPoint = new Point(0.0, 0.0);
                        darkSpecialKey.EndPoint = new Point(0.0, 1.0);
                        darkSpecialKey.GradientStops.Add(new GradientStop(Color.FromArgb(85, 56, 64, 80), 0.0));
                        darkSpecialKey.GradientStops.Add(new GradientStop(Color.FromArgb(46, 30, 38, 50), 1.0));
                        darkSpecialKey.Freeze();
                        SetResource("KeySpecialBrush", darkSpecialKey);

                        // 6. Hover
                        var darkHover = new LinearGradientBrush();
                        darkHover.StartPoint = new Point(0.0, 0.0);
                        darkHover.EndPoint = new Point(0.0, 1.0);
                        darkHover.GradientStops.Add(new GradientStop(Color.FromArgb(150, 108, 120, 146), 0.0));
                        darkHover.GradientStops.Add(new GradientStop(Color.FromArgb(105, 80, 92, 118), 1.0));
                        darkHover.Freeze();
                        SetResource("KeyHoverBrush", darkHover);

                        // 7. Key Border
                        var darkKeyBorder = new LinearGradientBrush();
                        darkKeyBorder.StartPoint = new Point(0.0, 0.0);
                        darkKeyBorder.EndPoint = new Point(0.0, 1.0);
                        darkKeyBorder.GradientStops.Add(new GradientStop(Color.FromArgb(70, 255, 255, 255), 0.0));
                        darkKeyBorder.GradientStops.Add(new GradientStop(Color.FromArgb(20, 255, 255, 255), 1.0));
                        darkKeyBorder.Freeze();
                        SetResource("KeyBorderBrush", darkKeyBorder);

                        // 8. Typography & Accents
                        SetResource("TextLightBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                        SetResource("TextMutedBrush", new SolidColorBrush(Color.FromRgb(165, 170, 182)));
                        SetResource("AccentBrush", appleAzure);
                        SetResource("AccentHoverBrush", appleHoverAzure);
                        SetResource("AccentGlowBrush", appleDarkGlow);

                        // 9. Pill-round keys with 1px border and liquid sheen
                        Application.Current.Resources["KeyCornerRadius"] = new CornerRadius(8);
                        Application.Current.Resources["KeyBorderThickness"] = new Thickness(1);
                        Application.Current.Resources["KeySheenOpacity"] = 0.45;
                        break;

                    case KeyboardTheme.DarkFluent:
                    default:
                        // Windows 10/11 Fluent (Default)
                        SetResource("KbBgBrush", new SolidColorBrush(Color.FromArgb(235, 24, 24, 24)));
                        SetResource("KbHeaderBgBrush", new SolidColorBrush(Color.FromRgb(32, 32, 32)));
                        SetResource("KbBorderBrush", new SolidColorBrush(Color.FromRgb(56, 56, 56)));
                        SetResource("KeyNormalBrush", new SolidColorBrush(Color.FromRgb(44, 44, 44)));
                        SetResource("KeySpecialBrush", new SolidColorBrush(Color.FromRgb(34, 34, 34)));
                        SetResource("KeyHoverBrush", new SolidColorBrush(Color.FromRgb(60, 60, 60)));
                        SetResource("TextLightBrush", new SolidColorBrush(Color.FromRgb(255, 255, 255)));
                        SetResource("TextMutedBrush", new SolidColorBrush(Color.FromRgb(153, 153, 153)));
                        SetResource("KeyBorderBrush", new SolidColorBrush(Color.FromArgb(26, 255, 255, 255)));
                        SetResource("AccentBrush", accentBrush);
                        Application.Current.Resources["KeyCornerRadius"] = new CornerRadius(4);
                        Application.Current.Resources["KeyBorderThickness"] = new Thickness(0, 0, 0, 1);
                        Application.Current.Resources["KeySheenOpacity"] = 0.0;
                        break;
                }
            }
            catch { }
        }

        private static void SetResource(string key, Brush brush)
        {
            if (brush != null)
            {
                brush.Freeze();
                Application.Current.Resources[key] = brush;
            }
        }
    }
}
