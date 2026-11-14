using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Threading;

namespace HotVirtualKeyboard.Automation
{
    public class FocusChangedEventArgs : EventArgs
    {
        public bool IsTextField { get; set; }
        public Rect BoundingRect { get; set; }
        public string ControlTypeName { get; set; }
    }

    public class FocusTracker
    {
        private static readonly Lazy<FocusTracker> _instance = new Lazy<FocusTracker>(CreateInstance);
        private static FocusTracker CreateInstance()
        {
            return new FocusTracker();
        }

        public static FocusTracker Instance
        {
            get { return _instance.Value; }
        }

        public event EventHandler<FocusChangedEventArgs> FocusChanged;

        private AutomationFocusChangedEventHandler _focusHandler;
        private readonly int _currentProcessId;
        private bool _isListening;
        private readonly Dispatcher _dispatcher;

        public bool IsAutoShowEnabled { get; set; }

        public FocusTracker()
        {
            IsAutoShowEnabled = true;
            _currentProcessId = Process.GetCurrentProcess().Id;
            _dispatcher = Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;
        }

        public void Start()
        {
            if (_isListening) return;

            try
            {
                _focusHandler = new AutomationFocusChangedEventHandler(OnAutomationFocusChanged);
                System.Windows.Automation.Automation.AddAutomationFocusChangedEventHandler(_focusHandler);
                _isListening = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UI Automation Focus tracking error: " + ex.Message);
            }
        }

        public void Stop()
        {
            if (!_isListening) return;

            try
            {
                if (_focusHandler != null)
                {
                    System.Windows.Automation.Automation.RemoveAutomationFocusChangedEventHandler(_focusHandler);
                    _focusHandler = null;
                }
                _isListening = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UI Automation Stop error: " + ex.Message);
            }
        }

        private void OnAutomationFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            if (!IsAutoShowEnabled) return;

            try
            {
                var element = sender as AutomationElement;
                if (element == null) return;

                // Check process ID to ignore focus within our own keyboard window
                int processId = element.Current.ProcessId;
                if (processId == _currentProcessId || processId == 0)
                {
                    return;
                }

                bool isText = IsTextControl(element);
                Rect rect = Rect.Empty;

                if (isText)
                {
                    try
                    {
                        var uiaRect = element.Current.BoundingRectangle;
                        if (!uiaRect.IsEmpty)
                        {
                            rect = new Rect(uiaRect.X, uiaRect.Y, uiaRect.Width, uiaRect.Height);
                        }
                    }
                    catch
                    {
                        rect = Rect.Empty;
                    }
                }

                string typeName = element.Current.ControlType != null ? element.Current.ControlType.ProgrammaticName : "Unknown";

                _dispatcher.BeginInvoke(new Action(delegate
                {
                    EventHandler<FocusChangedEventArgs> handler = FocusChanged;
                    if (handler != null)
                    {
                        FocusChangedEventArgs args = new FocusChangedEventArgs();
                        args.IsTextField = isText;
                        args.BoundingRect = rect;
                        args.ControlTypeName = typeName;
                        handler(this, args);
                    }
                }), DispatcherPriority.Normal);
            }
            catch
            {
                // UI Automation elements can become invalid when switching tabs/windows
            }
        }

        public static bool IsCurrentFocusTextField()
        {
            try
            {
                AutomationElement focused = AutomationElement.FocusedElement;
                if (focused == null) return false;

                int myPid = Process.GetCurrentProcess().Id;
                if (focused.Current.ProcessId == myPid || focused.Current.ProcessId == 0)
                {
                    return true; // Clicking inside own keyboard
                }

                return IsTextControl(focused);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsPointTextInputControl(Point physicalPoint)
        {
            try
            {
                var point = new System.Windows.Point(physicalPoint.X, physicalPoint.Y);
                AutomationElement element = AutomationElement.FromPoint(point);
                if (element != null && IsTextControl(element))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public static bool IsPointOrFocusTextField(Point physicalPoint)
        {
            return IsPointTextInputControl(physicalPoint);
        }

        public static Rect GetCaretOrElementRect(Point physicalPoint)
        {
            try
            {
                AutomationElement focused = AutomationElement.FocusedElement;
                if (focused != null && IsTextControl(focused))
                {
                    var r = focused.Current.BoundingRectangle;
                    if (!r.IsEmpty) return new Rect(r.X, r.Y, r.Width, r.Height);
                }
            }
            catch { }

            try
            {
                var point = new System.Windows.Point(physicalPoint.X, physicalPoint.Y);
                AutomationElement element = AutomationElement.FromPoint(point);
                if (element != null && IsTextControl(element))
                {
                    var r = element.Current.BoundingRectangle;
                    if (!r.IsEmpty) return new Rect(r.X, r.Y, r.Width, r.Height);
                }
            }
            catch { }

            return Rect.Empty;
        }

        public static bool IsTextControl(AutomationElement element)
        {
            if (element == null) return false;

            try
            {
                var controlType = element.Current.ControlType;

                // 1. Direct Edit controls (e.g. Notepad, Word, browser URL bars, input boxes)
                if (controlType == ControlType.Edit)
                {
                    object valPattern;
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valPattern))
                    {
                        var vp = valPattern as ValuePattern;
                        if (vp != null && vp.Current.IsReadOnly)
                            return false;
                    }
                    return true;
                }

                // 2. ComboBox controls with editable input
                if (controlType == ControlType.ComboBox)
                {
                    return element.Current.IsKeyboardFocusable;
                }

                // 3. Document controls (e.g. web browser tabs in Chrome/Edge/Brave)
                // A normal web document body is NOT an editable text field!
                // Only treat as text field if contenteditable or non-read-only ValuePattern exists
                if (controlType == ControlType.Document)
                {
                    object valPattern;
                    if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valPattern))
                    {
                        var vp = valPattern as ValuePattern;
                        if (vp != null && !vp.Current.IsReadOnly && element.Current.IsKeyboardFocusable)
                            return true;
                    }

                    return false;
                }

                // 4. Check general ValuePattern for editable focusable controls
                object valObj;
                if (element.TryGetCurrentPattern(ValuePattern.Pattern, out valObj))
                {
                    var vp = valObj as ValuePattern;
                    if (vp != null && !vp.Current.IsReadOnly && element.Current.IsKeyboardFocusable)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Element may have been destroyed
            }

            return false;
        }
    }
}
