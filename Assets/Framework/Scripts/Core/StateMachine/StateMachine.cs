using System.Collections.Generic;
using System;

namespace Framework.Core.FSM
{
    public class StateMachine
    {
        // id -> edge
        private Dictionary<string, List<Transition>> _transitions = new();
        // id -> state
        private Dictionary<string, IState> _states = new();
        private IState _current;
        public IDictionary<string, object> BlackBoard { get; } = new Dictionary<string, object>();

        public void Start(IState initState)
        {
            _current = initState;
            _current.OnEnter(new StateContext(BlackBoard, RequestStateChange));
        }

        public void StartById(string id)
        {
            if (!HasState(id))
            {
                throw new Exception("");
            }

            Start(_states[id]);
        }

        // 提供给状态或动作调用以请求立即变更到指定状态
        private void RequestStateChange(IState target) => ChangeState(target, new StateContext(BlackBoard, RequestStateChange));

        /// <summary>
        /// 添加一条转化条件
        /// </summary>
        /// <param name="fromId">发生转移的状态</param>
        /// <param name="cond">发生转移的条件</param>
        /// <param name="toId">转移的目标被状态</param>
        public void AddTransition(string fromId, string toId, IGuard cond)
        {
            if (!_transitions.TryGetValue(fromId, out var conditionList))
            {
                // 不存在时先创建
                conditionList = new();
                _transitions[fromId] = conditionList;
            }
            conditionList.Add(new Transition(_states[toId], cond));
        }

        public void AddTransition(IState fromState, IState toState, IGuard cond)
        {
            if (!_transitions.TryGetValue(fromState.Id, out var conditionList))
            {
                // 不存在时先创建
                conditionList = new();
                _transitions[fromState.Id] = conditionList;
            }
            conditionList.Add(new Transition(_states[toState.Id], cond));
        }

        public void AddState(IState state)
        {
            string id = state.Id;
            if (_states.ContainsKey(id))
            {
                throw new Exception("Contains id");
            }

            _states[id] = state;
        }

        public void OnExcute(float deltaTime)
        {
            if (_current == null)
                return;
            var ctx = new StateContext(BlackBoard, RequestStateChange);
            _current.OnExcute(ctx, deltaTime);
            if (_transitions.TryGetValue(_current.Id, out var conditionList))
            {
                foreach (var c in conditionList)
                {
                    if (c.CanTransit(ctx))
                    {
                        ChangeState(c.To, ctx);
                        break;
                    }
                }
            }
        }

        public void ChangeState(IState to, StateContext ctx)
        {
            _current.OnExit(ctx);
            _current = to;
            _current.OnEnter(new StateContext(BlackBoard, RequestStateChange));
        }

        public bool HasState(string id) => _states.ContainsKey(id);

        public IState GetStateByID(string id)
        {
            if (!HasState(id))
                return null;
            return _states[id];
        }
    }
}