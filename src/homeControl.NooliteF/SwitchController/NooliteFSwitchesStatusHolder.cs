using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using homeControl.NooliteF.Configuration;

namespace homeControl.NooliteF.SwitchController
{
    internal sealed class NooliteFSwitchesStatusHolder
    {
        private readonly ConcurrentDictionary<byte, (byte, bool)> _statuses = new ConcurrentDictionary<byte,(byte, bool)>();

        public void SetStatus(byte channel, byte power, bool isOn)
        {
            _statuses.AddOrUpdate(channel, _ => (power, isOn), (_1, _2) => (power, isOn));
        }
        
        public (byte power, bool isOn)? GetStatus(byte channel)
        {
            if (_statuses.TryGetValue(channel, out var value))
                return value;
            
            return null;
        }
    }
}