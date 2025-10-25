using UnityEngine;
using Framework.Core.FSM;
using NUnit.Framework;

namespace Tests
{
    public class TimerGuard : IGuard
    {
        private readonly string _key;
        private readonly float _threshold;

        public TimerGuard(string key, float threshold)
        {
            _key = key;
            _threshold = threshold;
        }
        public bool Evaluate(StateContext ctx) => ctx.Get<float>(_key) >= _threshold;
    }

    public class IdleState : BaseState
    {
        public IdleState(string id) : base(id) { }

        public override void OnEnter(StateContext ctx)
        {
            Debug.Log("Enter Idle");
            ctx.Set("time", 0f);
        }
        public override void OnExcute(StateContext ctx, float deltaTime) => ctx.Set("time", ctx.Get<float>("time") + deltaTime);
        public override void OnExit(StateContext ctx) { Debug.Log("Exit Idle"); }
    }

    public class MoveState : BaseState
    {
        public bool Entered { get; private set; } = false;
        public MoveState(string id) : base(id) { }

        public override void OnEnter(StateContext ctx)
        {
            Debug.Log("Enter Move");
            Entered = true;
        }
        public override void OnExcute(StateContext ctx, float deltaTime) { /* 移动等 */ }
        public override void OnExit(StateContext ctx) => Debug.Log("Exit Move");
    }
    public class StateMachineTest
    {

        [Test]
        public void RegisterAndGetStateById_Works()
        {
            var sm = new StateMachine();

            var moveState = new MoveState("player move");
            Debug.Log(moveState.Id);
            var idleState = new IdleState("player idle");
            sm.AddState(idleState);
            sm.AddState(moveState);

            Assert.AreSame(sm.GetStateByID("player idle"), idleState);
            Assert.AreSame(sm.GetStateByID("player move"), moveState);
        }

        [Test]
        public void StartById_Works()
        {
            var sm = new StateMachine();
            var idleState = new IdleState("idle");
            sm.AddState(idleState);
            sm.StartById("idle");

            var cur = GetCurrentStateForTest(sm);
            Assert.IsInstanceOf<IdleState>(cur);
        }

        [Test]
        public void IdleToMove_ByTimerTransition_Works()
        {
            Debug.Log("Test FSM Transition");
            var fsm = new StateMachine();
            var idle = new IdleState("idle");
            var move = new MoveState("move");

            fsm.AddState(idle);
            fsm.AddState(move);
            fsm.AddTransition(idle, move, new TimerGuard("time", 0.9f));
            fsm.Start(idle);

            fsm.OnExcute(0.4f);
            Assert.IsInstanceOf<IdleState>(GetCurrentStateForTest(fsm));
            fsm.OnExcute(0.4f);
            Assert.IsInstanceOf<IdleState>(GetCurrentStateForTest(fsm));
            fsm.OnExcute(0.4f);
            Assert.IsInstanceOf<MoveState>(GetCurrentStateForTest(fsm));
            var mv = (MoveState)GetCurrentStateForTest(fsm);
            Assert.IsTrue(mv.Entered);
        }

        private object GetCurrentStateForTest(StateMachine fsm)
        {
            // 通过反射查找 _current 字段
            var fi = typeof(StateMachine).GetField("_current", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return fi.GetValue(fsm);
        }
    }



}