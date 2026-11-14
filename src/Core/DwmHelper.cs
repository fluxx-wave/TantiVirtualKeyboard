using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace HotVirtualKeyboard.Core
{
    public static class DwmHelper
    {
        public enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData
        {
            public int Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public const int WCA_ACCENT_POLICY = 19;

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        public static void EnableBlur(Window window, bool enableAcrylic, int gradientColor)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return;

                var accent = new AccentPolicy();
                accent.AccentState = enableAcrylic ? AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND : AccentState.ACCENT_ENABLE_BLURBEHIND;
                accent.AccentFlags = 2; // draw all borders
                accent.GradientColor = gradientColor; // ABGR format

                int size = Marshal.SizeOf(accent);
                IntPtr pData = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(accent, pData, false);

                var data = new WindowCompositionAttributeData();
                data.Attribute = WCA_ACCENT_POLICY;
                data.Data = pData;
                data.SizeOfData = size;

                SetWindowCompositionAttribute(hwnd, ref data);
                Marshal.FreeHGlobal(pData);
            }
            catch { }
        }

        public static void DisableBlur(Window window)
        {
            try
            {
                IntPtr hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == IntPtr.Zero) return;

                var accent = new AccentPolicy();
                accent.AccentState = AccentState.ACCENT_DISABLED;

                int size = Marshal.SizeOf(accent);
                IntPtr pData = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(accent, pData, false);

                var data = new WindowCompositionAttributeData();
                data.Attribute = WCA_ACCENT_POLICY;
                data.Data = pData;
                data.SizeOfData = size;

                SetWindowCompositionAttribute(hwnd, ref data);
                Marshal.FreeHGlobal(pData);
            }
            catch { }
        }
    }
}
