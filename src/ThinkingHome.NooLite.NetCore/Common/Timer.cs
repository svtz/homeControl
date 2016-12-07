using System;

namespace ThinkingHome.NooLite.Common
{
    internal sealed class Timer : IDisposable
    {
        private readonly int _milliseconds;
        private System.Threading.Timer _internalTimer;

        public Timer(int milliseconds)
        {
            _milliseconds = milliseconds;
        }

        public void Start()
        {
            _internalTimer = new System.Threading.Timer(OnTimerTriggered, null, _milliseconds, 0);
        }

        public void Dispose()
        {
            _internalTimer?.Dispose();
        }

        private void OnTimerTriggered(object state)
        {
            OnElapsed();
        }

        public event EventHandler Elapsed;

        private void OnElapsed()
        {
            Elapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}