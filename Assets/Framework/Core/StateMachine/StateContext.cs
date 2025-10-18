using System;

namespace Framework.Core.FSM
{
    public class StateContext
    {
        // 存放共享数据
        public object BlackBoard { get; }
        public Action<IState> RequestStateChange { get; }

        public StateContext(object blackBoard, Action<IState> request)
        {
            BlackBoard = blackBoard;
            RequestStateChange = request;
        }
    }
}