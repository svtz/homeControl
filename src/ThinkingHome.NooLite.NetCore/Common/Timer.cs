using System;
using System.Threading;

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
            _internalTimer = new System.Threading.Timer(OnTimerTriggered, null, _milliseconds, _milliseconds);
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
            var handler = Interlocked.CompareExchange(ref Elapsed, null, null);
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}