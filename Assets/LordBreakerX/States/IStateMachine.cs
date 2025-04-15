namespace LordBreakerX.States
{
    public interface IStateMachine
    {
        public void RegisterState(BaseState state);

        public void RegisterState(IState state);
        public bool IsCurrentState(IState state);
        public void ChangeState(IState state);
    }
}
