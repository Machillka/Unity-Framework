namespace Framework.Core.FSM
{
    public interface IState
    {
        string Id { get; }
        public void OnEnter();
        public void OnExcute();
        public void OnExit();
    }
}