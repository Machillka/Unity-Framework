using System;
using System.Collections.Generic;

namespace Framework.Core.FSM
{
    public class StateContext
    {
        // 存放共享数据
        private readonly IDictionary<string, object> _blackBoard;
        public Action<IState> RequestStateChange { get; }

        public StateContext(IDictionary<string, object> blackBoard, Action<IState> request)
        {
            _blackBoard = blackBoard;
            RequestStateChange = request;
        }

        public T Get<T>(string key) => _blackBoard.TryGetValue(key, out var v) ? (T)v : default;
        public void Set(string key, object value) => _blackBoard[key] = value;
    }
}