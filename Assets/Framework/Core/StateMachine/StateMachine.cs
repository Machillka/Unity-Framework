using System.Collections.Generic;
using System;

namespace Framework.Core.FSM
{
    public class StateMachine
    {
        // id -> edge
        private Dictionary<string, List<Transition>> _transitions;
        // id -> state
        private Dictionary<string, IState> _states;
        private IState _current;
        public object BlackBoard { get; }

        public void Start(IState initState)
        {
            _current = initState;
            _current.OnEnter();
        }

        public void AddTransition(string fromId, ITransitionCondition cond, string toId)
        {
            if (!_transitions.TryGetValue(fromId, out var conditionList))
            {
                // 不存在时先创建
                conditionList = new();
                _transitions[fromId] = conditionList;
            }
            conditionList.Add(new Transition(_states[toId], cond));
        }

        public void AddTransition(IState fromState, ITransitionCondition cond, IState toState)
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

        public void OnExcute()
        {
            if (_current == null)
                return;
            _current.OnExcute();
            if (_transitions.TryGetValue(_current.Id, out var conditionList))
            {
                foreach (var c in conditionList)
                {
                    if (c.Condition.Evaluate())
                    {
                        ChangeState(c.To);
                        break;
                    }
                }
            }
        }

        public void ChangeState(IState to)
        {
            _current.OnExit();
            _current = to;
            _current.OnEnter();
        }

        public class Transition
        {
            public Transition(IState to, ITransitionCondition condition)
            {
                To = to;
                Condition = condition;
            }

            public IState To;
            public ITransitionCondition Condition;
        }
    }
}