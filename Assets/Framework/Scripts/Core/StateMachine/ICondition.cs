namespace Framework.Core.FSM
{
    public interface IGuard
    {
        public bool Evaluate(StateContext ctx);
    }
}