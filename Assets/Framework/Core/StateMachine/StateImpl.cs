using UnityEngine;
using System.Threading.Tasks;

namespace Framework.Core.StateMachine
{
    public class State : IState
    {
        public string Id { get; set; }
        public string Message;

        public Task OnEnter(StateContext ctx)
        {
            Debug.Log($"Enter {Id}: {Message}");
            return Task.CompletedTask;
        }

        public Task OnExit(StateContext ctx)
        {
            Debug.Log($"Exit {Id}");
            return Task.CompletedTask;
        }

        public Task OnUpdate(StateContext ctx)
        {
            return Task.CompletedTask;
        }
    }
}