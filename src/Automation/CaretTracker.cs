using System;
using System.Windows;
using HotVirtualKeyboard.Core;

namespace HotVirtualKeyboard.Automation
{
    public static class CaretTracker
    {
        public static Point CalculateOptimalWindowPosition(Rect physicalTargetRect, double windowWidth, double windowHeight)
        {
            Rect workArea = SystemParameters.WorkArea;
            double scale = DpiHelper.GetSystemDpiScale();

            // Convert UI Automation physical screen coordinates to WPF DIPs
            Rect dipTarget = DpiHelper.PhysicalToDip(physicalTargetRect, scale);

            // If no target rect, place at bottom center of working area
            if (dipTarget.IsEmpty || dipTarget.Width <= 0 || dipTarget.Height <= 0)
            {
                double defaultX = workArea.Left + Math.Max(10.0, (workArea.Width - windowWidth) / 2.0);
                double defaultY = workArea.Bottom - windowHeight - 10.0;
                return ClampPoint(new Point(defaultX, defaultY), windowWidth, windowHeight, workArea);
            }

            // 1. START MENU & SEARCH PANE AVOIDANCE (Windows 10 & 11):
            // Windows 11 Start Menu and Search flyouts occupy the bottom or bottom-center area (elevated shell band).
            // When Start Menu or Search is open, place the virtual keyboard safely at the TOP of the screen!
            Win32.RECT startRect;
            bool isStartOpen = Win32.IsStartMenuOrSearchOpen(out startRect);
            if (isStartOpen)
            {
                double safeX = workArea.Left + Math.Max(10.0, (workArea.Width - windowWidth) / 2.0);
                double safeY = workArea.Top + 15.0;
                return ClampPoint(new Point(safeX, safeY), windowWidth, windowHeight, workArea);
            }

            // Also detect if the focused element is located inside the typical Start Menu/Search region at the bottom
            if (dipTarget.Bottom > workArea.Bottom - 550 && dipTarget.Top > workArea.Top + 200)
            {
                double safeX = workArea.Left + Math.Max(10.0, (workArea.Width - windowWidth) / 2.0);
                double safeY = workArea.Top + 15.0;
                return ClampPoint(new Point(safeX, safeY), windowWidth, windowHeight, workArea);
            }

            double targetBottom = dipTarget.Bottom;
            double spaceBelow = workArea.Bottom - targetBottom;
            double spaceAbove = dipTarget.Top - workArea.Top;

            double posX = dipTarget.Left;
            double posY;

            // 1. Try placing directly below the focused input if space permits
            if (spaceBelow >= windowHeight + 15.0 && (targetBottom + 10.0 + windowHeight <= workArea.Bottom))
            {
                posY = targetBottom + 10.0;
            }
            // 2. Try placing directly above the focused input
            else if (spaceAbove >= windowHeight + 15.0 && (dipTarget.Top - windowHeight - 10.0 >= workArea.Top))
            {
                posY = dipTarget.Top - windowHeight - 10.0;
            }
            // 3. Fallback: align to bottom of work area (strictly above the taskbar)
            else
            {
                posY = workArea.Bottom - windowHeight - 10.0;
            }

            return ClampPoint(new Point(posX, posY), windowWidth, windowHeight, workArea);
        }

        private static Point ClampPoint(Point pt, double windowWidth, double windowHeight, Rect workArea)
        {
            double safeWidth = Math.Min(windowWidth, workArea.Width - 10.0);
            double safeHeight = Math.Min(windowHeight, workArea.Height - 10.0);

            double clampedX = Math.Max(workArea.Left + 5.0, Math.Min(pt.X, workArea.Right - safeWidth - 5.0));
            double clampedY = Math.Max(workArea.Top + 5.0, Math.Min(pt.Y, workArea.Bottom - safeHeight - 5.0));

            return new Point(clampedX, clampedY);
        }
    }
}
