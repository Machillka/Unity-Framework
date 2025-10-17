using System;

namespace Framework.Core.EventSystem
{
    public interface IEventBus
    {
        void Publish<T>(string channel, T payload);
        void Subscribe<T>(string channel, Action<T> handler);
        void Unsubscribe<T>(string channel, Action<T> handler);
    }
}