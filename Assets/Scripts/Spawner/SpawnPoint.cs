using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using UnityEngine;

[System.Serializable]
public class SpawnPoint
{
    #region ================ [[ STATE MACHINE ]] ================
    public enum State { DISABLED, AVAILABLE, SPAWNING }
    public class StateMachine : FiniteStateMachine<State>
    {
        public SpawnPoint spawnPoint;
        public StateMachine(SpawnPoint spawnPoint)
        {
            possibleStates = new Dictionary<State, FiniteState<State>>
            {
                { State.DISABLED, new DisabledState(this, State.DISABLED) },
                { State.AVAILABLE, new AvailableState(this, State.AVAILABLE) },
                { State.SPAWNING, new SpawningState(this, State.SPAWNING) },
            };

            // Set the initial state
            GoToState(State.DISABLED);
        }

        public class DisabledState : FiniteState<State>
        {
            public DisabledState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
            public override void Enter() { }
            public override void Execute() { }
            public override void Exit() { }
        }

        public class AvailableState : FiniteState<State>
        {
            public AvailableState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
            public override void Enter() { }
            public override void Execute() { }
            public override void Exit() { }
        }

        public class SpawningState : FiniteState<State>
        {
            public SpawningState(FiniteStateMachine<State> stateMachine, State stateType) : base(stateMachine, stateType) { }
            public override void Enter() { }
            public override void Execute() { }
            public override void Exit() { }
        }
    }
    StateMachine _stateMachine;
    public StateMachine stateMachine
    {
        get
        {
            if (_stateMachine == null)
                _stateMachine = new StateMachine(this);
            return _stateMachine;
        }
        private set => _stateMachine = value;
    }
    public SpawnPoint.State CurrentState => stateMachine.CurrentState;
    public void GoToState(State state) => stateMachine.GoToState(state);
    #endregion

    // ---------- Data ----------
    [SerializeField, ShowOnly] int _index;
    [SerializeField, ShowOnly] Vector3 _position;

    // ---------- References ----------
    public int index => _index;
    public Vector3 position => _position;

    // ---------- Constructor ----------
    public SpawnPoint(int index, Vector3 position) : base()
    {
        this._index = index;
        this._position = position;

        // Initialize the state machine
        stateMachine = new StateMachine(this);
    }

    public Color GetColor()
    {
        switch (stateMachine.CurrentState)
        {
            case State.DISABLED:
                return Color.grey;
            case State.AVAILABLE:
                return Color.yellow;
            case State.SPAWNING:
                return Color.green;
            default:
                return Color.white;
        }
    }

    public void DrawGizmos(int size)
    {
        Gizmos.color = GetColor();
        Gizmos.DrawSphere(_position, size);
    }
}
