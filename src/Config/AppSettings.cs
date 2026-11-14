using System;
using System.IO;
using System.Xml.Serialization;

namespace HotVirtualKeyboard.Config
{
    public enum DockMode
    {
        Floating,
        DockBottom,
        DockTop
    }

    public enum KeyboardTheme
    {
        DarkFluent,          // Windows 10/11 Fluent (Default)
        SolidFilled,         // Solid Filled Matte Dark
        AppleGlassmorphism,  // Apple Glassmorphism (Frosted Light Glass)
        AppleTranslucentDark,// Apple Translucent Blurred Dark Glass
        LightFluent,
        HighContrast,
        Cyberpunk
    }

    public class AppSettings
    {
        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HotVirtualKeyboard");
        private static readonly string SettingsFilePath = Path.Combine(SettingsDir, "settings.xml");

        public KeyboardTheme Theme { get; set; }
        public double WindowOpacity { get; set; }
        public double UiScale { get; set; }
        public DockMode Dock { get; set; }
        public bool AutoShowOnFocus { get; set; }
        public bool SoundFeedback { get; set; }
        public int KeyRepeatDelayMs { get; set; }
        public int KeyRepeatIntervalMs { get; set; }
        public bool LockScreenIntegration { get; set; }
        public string DefaultLayoutId { get; set; }

        public double FloatingX { get; set; }
        public double FloatingY { get; set; }
        public double FloatingWidth { get; set; }
        public double FloatingHeight { get; set; }

        public AppSettings()
        {
            Theme = KeyboardTheme.DarkFluent;
            WindowOpacity = 0.95;
            UiScale = 1.0;
            Dock = DockMode.DockBottom;
            AutoShowOnFocus = true;
            SoundFeedback = true;
            KeyRepeatDelayMs = 400;
            KeyRepeatIntervalMs = 50;
            LockScreenIntegration = true;
            DefaultLayoutId = "touch";

            FloatingX = 150;
            FloatingY = 450;
            FloatingWidth = 860;
            FloatingHeight = 320;
        }

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var serializer = new XmlSerializer(typeof(AppSettings));
                    using (var stream = File.OpenRead(SettingsFilePath))
                    {
                        return (AppSettings)serializer.Deserialize(stream);
                    }
                }
            }
            catch { }

            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists(SettingsDir))
                {
                    Directory.CreateDirectory(SettingsDir);
                }

                var serializer = new XmlSerializer(typeof(AppSettings));
                using (var writer = new StreamWriter(SettingsFilePath))
                {
                    serializer.Serialize(writer, this);
                }
            }
            catch { }
        }
    }
}
