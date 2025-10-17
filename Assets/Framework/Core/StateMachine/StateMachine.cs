using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Core.StateMachine
{
    public class StateMachine : IStateMachine
    {
        private readonly Dictionary<string, IState> _states = new();
        private readonly List<Transition> _transitions = new();
        private readonly Queue<string> _triggerQueue = new();
        public StateContext Context { get; } = new();
        private IState _current;

        public void AddState(IState state) => _states[state.Id] = state;

        public void AddTransition(Transition t) => _transitions.Add(t);

        public void Trigger(string trigger) => _triggerQueue.Enqueue(trigger);

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public async Task Initialize(string startId)
        {
            _current = _states[startId];
            await _current.OnEnter(Context);
        }

        public async Task Tick(float deltaTime)
        {
            Context.Data["deltaTime"] = deltaTime;

            while (_triggerQueue.Count > 0)
            {
                var tri = _triggerQueue.Dequeue();
                var match = _transitions.Find(t => t.From == _current.Id && t.Trigger == tri);
                if (match != null)
                {
                    await TransitionTo(match.To);
                    return;
                }
            }

            var implMatch = _transitions.Find(t => t.From == _current.Id && string.IsNullOrEmpty(t.Trigger));
            if (implMatch != null)
            {
                await TransitionTo(implMatch.To);
                return;
            }
            await _current.OnUpdate(Context);
        }
        public async Task TransitionTo(string toId)
        {
            await _current.OnExit(Context);
            _current = _states[toId];
            await _current.OnEnter(Context);
        }
    }
}