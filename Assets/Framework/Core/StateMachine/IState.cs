using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Core.StateMachine
{
    public interface IState
    {
        string Id { get; set; }
        Task OnEnter(StateContext ctx);
        Task OnExit(StateContext ctx);
        Task OnUpdate(StateContext ctx);
    }

    public class StateContext
    {
        public object Host;
        public Dictionary<string, object> Data = new();
    }


}