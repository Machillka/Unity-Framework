using System;
using System.Threading.Tasks;

namespace Framework.Core.StateMachine
{
    public interface IStateMachine : IDisposable
    {
        void AddState(IState state);
        void AddTransition(Transition t);
        void Trigger(string trigger);
        Task Initialize(string startId);
        Task Tick(float deltaTime);
    }
}