using System;
using System.Collections.Generic;

namespace homeControl.Tests.Noolite
{
    internal sealed class TestObservable<T> : IObservable<T>
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        
        private sealed class Subscription : IDisposable
        {
            private readonly TestObservable<T> _owner;
            public IObserver<T> Observer { get; }

            public Subscription(TestObservable<T> owner, IObserver<T> observer)
            {
                _owner = owner;
                Observer = observer;

                lock (_owner)
                {
                    _owner._subscriptions.Add(this);
                }
            }

            public void Dispose()
            {
                lock (_owner)
                {
                    _owner._subscriptions.Remove(this);
                }
            }
        }
        
        public IDisposable Subscribe(IObserver<T> observer)
        {
            return new Subscription(this, observer);
        }

        public void Add(T element)
        {
            lock (this)
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.Observer.OnNext(element);
                }
            }
        }
        
        public void Complete()
        {
            lock (this)
            {
                foreach (var subscription in _subscriptions.ToArray())
                {
                    subscription.Observer.OnCompleted();
                }
            }
        }
    }
}