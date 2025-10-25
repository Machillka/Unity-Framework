using System.Collections.Generic;

namespace Framework.Core.FSM
{
    public class Transition
    {
        public Transition(IState to, IGuard condition)
        {
            To = to;
            Conditions = new List<IGuard>
                {
                    condition
                };
        }

        public IState To;
        public List<IGuard> Conditions;
        public bool CanTransit(StateContext ctx)
        {
            foreach (var c in Conditions)
            {
                if (!c.Evaluate(ctx))
                    return false;
            }
            return true;
        }
    }
}