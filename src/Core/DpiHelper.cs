using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace HotVirtualKeyboard.Core
{
    public static class DpiHelper
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private const int LOGPIXELSX = 88;
        private const int LOGPIXELSY = 90;

        public static double GetSystemDpiScale()
        {
            try
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                if (hdc != IntPtr.Zero)
                {
                    int dpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                    ReleaseDC(IntPtr.Zero, hdc);
                    if (dpiX > 0)
                    {
                        return dpiX / 96.0;
                    }
                }
            }
            catch { }

            return 1.0;
        }

        public static double GetVisualDpiScale(Visual visual)
        {
            if (visual != null)
            {
                PresentationSource source = PresentationSource.FromVisual(visual);
                if (source != null && source.CompositionTarget != null)
                {
                    return source.CompositionTarget.TransformToDevice.M11;
                }
            }
            return GetSystemDpiScale();
        }

        public static Point PhysicalToDip(Point physicalPoint, double scale)
        {
            if (scale <= 0) scale = 1.0;
            return new Point(physicalPoint.X / scale, physicalPoint.Y / scale);
        }

        public static Rect PhysicalToDip(Rect physicalRect, double scale)
        {
            if (scale <= 0) scale = 1.0;
            return new Rect(
                physicalRect.X / scale,
                physicalRect.Y / scale,
                physicalRect.Width / scale,
                physicalRect.Height / scale);
        }
    }
}
