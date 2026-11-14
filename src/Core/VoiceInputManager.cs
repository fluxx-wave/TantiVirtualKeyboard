using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace HotVirtualKeyboard.Core
{
    public class VoiceInputManager
    {
        private static readonly Lazy<VoiceInputManager> _instance = new Lazy<VoiceInputManager>(CreateInstance);
        private static VoiceInputManager CreateInstance()
        {
            return new VoiceInputManager();
        }

        public static VoiceInputManager Instance
        {
            get { return _instance.Value; }
        }

        public event EventHandler<bool> ListeningStateChanged;

        private bool _isListening = false;
        private DispatcherTimer _autoTimeoutTimer;

        public bool IsListening
        {
            get { return _isListening; }
            private set
            {
                if (_isListening != value)
                {
                    _isListening = value;
                    EventHandler<bool> handler = ListeningStateChanged;
                    if (handler != null)
                    {
                        handler(this, _isListening);
                    }
                }
            }
        }

        public VoiceInputManager()
        {
            _autoTimeoutTimer = new DispatcherTimer();
            _autoTimeoutTimer.Interval = TimeSpan.FromSeconds(25);
            _autoTimeoutTimer.Tick += delegate
            {
                _autoTimeoutTimer.Stop();
                StopListening();
            };
        }

        public void ToggleListening()
        {
            if (IsListening)
            {
                StopListening();
            }
            else
            {
                StartListening();
            }
        }

        public void StartListening()
        {
            try
            {
                IsListening = true;
                _autoTimeoutTimer.Stop();
                _autoTimeoutTimer.Start();

                // Trigger Windows 10 / 11 Native Voice Typing / Dictation (Win + H)
                // Win32.VK_LWIN = 0x5B, 'H' = 0x48
                KeyInjector.SendHotkey(Win32.VK_LWIN, 0x48);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error starting voice input: " + ex.Message);
                StopListening();
            }
        }

        public void StopListening()
        {
            _autoTimeoutTimer.Stop();
            if (IsListening)
            {
                IsListening = false;
            }
        }
    }
}
