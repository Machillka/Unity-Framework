using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core.EventSystem
{
    /// <summary>
    /// 使用 channel 作为定位的广播系统
    /// </summary>
    public class EventBus : IEventBus
    {
        // (channel -> 委托方法)
        private readonly Dictionary<string, List<Delegate>> _handlers = new();

        public void Subscribe<TEvent>(string channel, Action<TEvent> handler)
        {
            if (handler == null)
                return;
            if (!_handlers.TryGetValue(channel, out var list))
            {
                list = new List<Delegate>();
                _handlers[channel] = list;
            }
            list.Add(handler);
        }

        public void Unsubscribe<TEvent>(string channel, Action<TEvent> handler)
        {
            if (handler == null)
                return;
            if (_handlers.TryGetValue(channel, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                    _handlers.Remove(channel);
            }
        }

        public void Publish<TEvent>(string channel, TEvent evt)
        {
            if (!_handlers.TryGetValue(channel, out var list))
                return;

            var snapshot = list.ToArray();
            foreach (var dele in list)
            {
                try
                {
                    ((Action<TEvent>)dele).Invoke(evt);
                }
                catch (Exception exp)
                {
                    Debug.LogException(exp);
                }
            }
        }
    }
}