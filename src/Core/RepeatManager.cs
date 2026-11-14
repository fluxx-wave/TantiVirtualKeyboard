using System;
using System.Windows.Threading;

namespace HotVirtualKeyboard.Core
{
    public class RepeatManager
    {
        private static readonly Lazy<RepeatManager> _instance = new Lazy<RepeatManager>(CreateInstance);
        private static RepeatManager CreateInstance()
        {
            return new RepeatManager();
        }

        public static RepeatManager Instance
        {
            get { return _instance.Value; }
        }

        private readonly DispatcherTimer _delayTimer;
        private readonly DispatcherTimer _repeatTimer;
        private Action _currentAction;

        public int InitialDelayMs { get; set; }
        public int RepeatIntervalMs { get; set; }

        public RepeatManager()
        {
            InitialDelayMs = 400;
            RepeatIntervalMs = 50;

            _delayTimer = new DispatcherTimer();
            _delayTimer.Tick += OnDelayTick;

            _repeatTimer = new DispatcherTimer();
            _repeatTimer.Tick += OnRepeatTick;
        }

        public void StartRepeat(Action action)
        {
            StopRepeat();

            _currentAction = action;
            _delayTimer.Interval = TimeSpan.FromMilliseconds(InitialDelayMs);
            _delayTimer.Start();
        }

        public void StopRepeat()
        {
            _delayTimer.Stop();
            _repeatTimer.Stop();
            _currentAction = null;
        }

        private void OnDelayTick(object sender, EventArgs e)
        {
            _delayTimer.Stop();
            if (_currentAction != null)
            {
                _repeatTimer.Interval = TimeSpan.FromMilliseconds(RepeatIntervalMs);
                _repeatTimer.Start();
            }
        }

        private void OnRepeatTick(object sender, EventArgs e)
        {
            if (_currentAction != null)
            {
                _currentAction();
            }
            AudioManager.Instance.PlayKeySound();
        }
    }
}
