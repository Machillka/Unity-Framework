namespace Framework.Core.FSM
{
    public interface IState
    {
        string Id { get; }
        public void OnEnter(StateContext ctx);
        public void OnExcute(StateContext ctx, float deltaTime);
        public void OnExit(StateContext ctx);
    }

    public abstract class BaseState : IState
    {
        protected BaseState(string id) => Id = id;

        public string Id { get; }

        public virtual void OnEnter(StateContext ctx) { }

        public virtual void OnExcute(StateContext ctx, float deltaTime) { }

        public virtual void OnExit(StateContext ctx) { }
    }
}