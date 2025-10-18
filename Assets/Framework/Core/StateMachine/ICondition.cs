namespace Framework.Core.FSM
{
    public interface ITransitionCondition
    {
        public bool Evaluate();
    }
}