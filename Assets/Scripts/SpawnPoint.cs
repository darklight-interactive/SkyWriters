using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using UnityEngine;

[System.Serializable]
public class SpawnPoint : FiniteStateMachine<SpawnPoint.State>
{
    public enum State { DISABLED, WAITING, SPAWNING, COUNTDOWN }
    [SerializeField, ShowOnly] State _currentState;
    [SerializeField, ShowOnly] int _index;
    public int index => _index;
    [SerializeField, ShowOnly] Vector3 _position;
    public Vector3 position => _position;
    public SpawnPoint(int index, Vector3 position)
    {
        this._index = index;
        this._position = position;

        // Add the states tp the FSM
        AddState(new DisabledState(this, State.DISABLED));
        AddState(new WaitingState(this, State.WAITING));
        AddState(new SpawningState(this, State.SPAWNING));
        AddState(new CountdownState(this, State.COUNTDOWN));

        // Set the initial state
        GoToState(State.WAITING);
        _currentState = State.WAITING;

        // Listen for state changes
        OnStateChanged += (state) =>
        {
            _currentState = state;
        };
    }

    public Color GetColor()
    {
        switch (_currentState)
        {
            case State.DISABLED:
                return Color.black;
            case State.WAITING:
                return Color.yellow;
            case State.SPAWNING:
                return Color.green;
            case State.COUNTDOWN:
                return Color.blue;
            default:
                return Color.white;
        }
    }

    class DisabledState : FiniteState<State>
    {
        public DisabledState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
        public override void Enter() { }
        public override void Execute() { }
        public override void Exit() { }
    }

    class WaitingState : FiniteState<State>
    {
        public WaitingState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
        public override void Enter() { }
        public override void Execute() { }
        public override void Exit() { }
    }

    class SpawningState : FiniteState<State>
    {
        public SpawningState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
        public override void Enter() { }
        public override void Execute() { }
        public override void Exit() { }
    }

    class CountdownState : FiniteState<State>
    {
        public CountdownState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
        public override void Enter() { }
        public override void Execute() { }
        public override void Exit() { }
    }

}
