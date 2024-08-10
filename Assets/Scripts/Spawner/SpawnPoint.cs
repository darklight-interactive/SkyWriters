using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class SpawnPoint
{
    #region ================ [[ STATE MACHINE ]] ================
    public enum State { DISABLED, AVAILABLE, SPAWNING }
    public class StateMachine : FiniteStateMachine<State>
    {
        public Spawner spawner;
        public SpawnPoint spawnPoint;
        public StateMachine(Spawner spawner, SpawnPoint spawnPoint)
        {
            this.spawner = spawner;
            this.spawnPoint = spawnPoint;
            possibleStates = new Dictionary<State, FiniteState<State>>
            {
                { State.DISABLED, new DisabledState(this, State.DISABLED) },
                { State.AVAILABLE, new AvailableState(this, State.AVAILABLE) },
                { State.SPAWNING, new SpawningState(this, State.SPAWNING) },
            };

            // Set the initial state
            GoToState(State.DISABLED);
        }

        public void GoToStateWithDelay(State state, float delay)
        {
            spawner.GoToStateWitDelay(spawnPoint, state, delay);
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
            StateMachine _stateMachine;
            public SpawningState(StateMachine stateMachine, State stateType) : base(stateMachine, stateType)
            {
                _stateMachine = stateMachine;
            }
            public override void Enter()
            {
                _stateMachine.GoToStateWithDelay(State.AVAILABLE, _stateMachine.spawner.spawnDelay);
            }
            public override void Execute() { }
            public override void Exit() { }
        }
    }
    StateMachine _stateMachine;
    public SpawnPoint.State CurrentState => _stateMachine.CurrentState;
    public void GoToState(State state) => _stateMachine.GoToState(state);


    #endregion

    // ---------- Data ----------
    Spawner _spawner;
    [SerializeField, ShowOnly] int _index;
    [SerializeField, ShowOnly] Vector3 _position;

    // ---------- References ----------
    public int index => _index;
    public Vector3 position => _position;

    // ---------- Constructor ----------
    public SpawnPoint(Spawner spawner, int index, Vector3 position) : base()
    {
        this._spawner = spawner;
        this._index = index;
        this._position = position;

        // Initialize the state machine
        _stateMachine = new StateMachine(_spawner, this);
        _stateMachine.GoToState(_spawner.spawnPoint_defaultState);
    }

    public Color GetColor()
    {
        if (_stateMachine == null)
            return Color.white;

        switch (_stateMachine.CurrentState)
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
