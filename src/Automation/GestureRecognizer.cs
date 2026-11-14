using System;
using System.Windows;

namespace HotVirtualKeyboard.Automation
{
    public enum SwipeDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public class GestureRecognizer
    {
        private Point _startPoint;
        private bool _isTracking;
        private const double MinSwipeDistance = 70.0;

        public event EventHandler<SwipeDirection> SwipeDetected;

        public void OnTouchDown(Point pt)
        {
            _startPoint = pt;
            _isTracking = true;
        }

        public void OnTouchUp(Point pt)
        {
            if (!_isTracking) return;
            _isTracking = false;

            double dx = pt.X - _startPoint.X;
            double dy = pt.Y - _startPoint.Y;

            if (Math.Abs(dx) < MinSwipeDistance && Math.Abs(dy) < MinSwipeDistance)
            {
                return;
            }

            SwipeDirection dir = SwipeDirection.None;
            if (Math.Abs(dx) > Math.Abs(dy))
            {
                dir = dx > 0 ? SwipeDirection.Right : SwipeDirection.Left;
            }
            else
            {
                dir = dy > 0 ? SwipeDirection.Down : SwipeDirection.Up;
            }

            if (dir != SwipeDirection.None)
            {
                EventHandler<SwipeDirection> handler = SwipeDetected;
                if (handler != null)
                {
                    handler(this, dir);
                }
            }
        }
    }
}
