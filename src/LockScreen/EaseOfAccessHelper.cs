using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace HotVirtualKeyboard.LockScreen
{
    public static class EaseOfAccessHelper
    {
        private const string AccessibilityKeyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Accessibility";
        private const string AppName = "HotVirtualKeyboard";

        public static bool IsRegisteredForEaseOfAccess()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(AccessibilityKeyPath, false))
                {
                    if (key == null) return false;
                    object val = key.GetValue("Configuration");
                    return val != null && val.ToString().Contains(AppName);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool RegisterAsDefaultOsk()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string subKey = string.Format(@"{0}\ATs\{1}", AccessibilityKeyPath, AppName);

                using (var atKey = Registry.LocalMachine.CreateSubKey(subKey))
                {
                    if (atKey != null)
                    {
                        atKey.SetValue("ApplicationName", "Hot Virtual Keyboard");
                        atKey.SetValue("BinaryPath", exePath);
                        atKey.SetValue("Profile", "osk");
                        atKey.SetValue("Description", "Industry-Grade On-Screen Keyboard");
                        atKey.SetValue("TerminateOnDesktopSwitch", 0, RegistryValueKind.DWord);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error registering Ease of Access: " + ex.Message);
                return false;
            }
        }

        public static bool UnregisterFromEaseOfAccess()
        {
            try
            {
                string subKey = string.Format(@"{0}\ATs\{1}", AccessibilityKeyPath, AppName);
                Registry.LocalMachine.DeleteSubKeyTree(subKey, false);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error unregistering Ease of Access: " + ex.Message);
                return false;
            }
        }
    }
}
