using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Behaviour;
using System.Collections;



#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class StageEntity : MonoBehaviour
{
    public enum Type { NULL, PLANE, CLOUD, BLIMP }
    public enum State { NULL, SPAWN, GAME, DESPAWN }
    public class Data
    {
        public int entityId { get; private set; } = -1;
        public StageEntity.Type type { get; private set; } = Type.NULL;

        // ---- Rules ----
        public bool respawnOnExit { get; private set; } = true;

        // ---- Collider ----
        public float colliderHeight { get; private set; } = 10;
        public float colliderRadius { get; private set; } = 5;

        // ---- Speed ----
        public float moveSpeed { get; private set; } = 10;
        public float rotationSpeed { get; private set; } = 5;

        // ---- Stats ----
        public float windResistance { get; private set; } = 0.2f;

        // ---- Gameplay ----
        public float lifeSpan { get; private set; } = -1;

        public Data() { }
        public Data(StageEntityPreset preset)
        {
            entityId = preset.GetInstanceID();
            type = preset.type;
            colliderHeight = preset.colliderHeight;
            colliderRadius = preset.colliderRadius;
            moveSpeed = preset.moveSpeed;
            rotationSpeed = preset.rotationSpeed;
            windResistance = preset.windResistance;
            respawnOnExit = preset.respawnOnExit;
            lifeSpan = preset.lifeSpan;
        }
    }
    public class StateMachine : FiniteStateMachine<State>
    {
        public StageEntity entity { get; protected set; }
        public StateMachine() { }
        public StateMachine(StageEntity entity)
        {
            this.entity = entity;

            AddState(new SpawnState(this, State.SPAWN));
            AddState(new GameState(this, State.GAME));
            AddState(new DespawnState(this, State.DESPAWN));

            GoToState(State.SPAWN);
        }

        public void GoToStateWithDelay(State state, float delay)
        {
            entity.StartCoroutine(StateChangeRoutine(state, delay));
        }

        IEnumerator StateChangeRoutine(State state, float delay)
        {
            yield return new WaitForSeconds(delay);
            GoToState(state);
        }



        /// <summary>
        /// This is the basic state for the entity when it is spawned.
        /// The entity will remain in this state until it enters the stage bounds.
        /// </summary>
        public class SpawnState : FiniteState<State>
        {
            StageEntity entity;
            public SpawnState(StateMachine stateMachine, State stateType) : base(stateMachine, stateType)
            {
                entity = stateMachine.entity;
            }

            public override void Enter() { }
            public override void Execute()
            {
                // Check if the entity has entered the stage bounds
                if (!entity.IsInSpawnBounds() && entity.IsInStageBounds())
                {
                    entity.stateMachine.GoToStateWithDelay(State.GAME, 1);
                }

                // Check if the entity is in the game bounds
                if (!entity.IsInStageBounds() && !entity.IsInSpawnBounds())
                {
                    entity.stateMachine.GoToState(State.DESPAWN);
                }
            }
            public override void Exit() { }
        }

        public class GameState : FiniteState<State>
        {
            StageEntity entity;
            public GameState(StateMachine stateMachine, State stateType) : base(stateMachine, stateType)
            {
                entity = stateMachine.entity;
            }

            public override void Enter() { }
            public override void Execute()
            {
                // Check if the entity has exited the stage bounds
                if (entity.IsInSpawnBounds() && !entity.IsInStageBounds())
                {
                    entity.stateMachine.GoToState(State.DESPAWN);
                }


                // Check if the entity is in the game bounds
                if (!entity.IsInStageBounds() && !entity.IsInSpawnBounds())
                {
                    entity.stateMachine.GoToState(State.DESPAWN);
                }
            }

            public override void Exit() { }
        }

        public class DespawnState : FiniteState<State>
        {
            StageEntity entity;
            public DespawnState(StateMachine stateMachine, State stateType) : base(stateMachine, stateType)
            {
                entity = stateMachine.entity;
            }
            public override void Enter()
            {
                // Call the OnStageExit method of the entity
                bool respawn = entity.data.respawnOnExit;
                entity.OnStageExit(respawn);

                // If the entity is set to respawn, go back to the spawn state
                if (respawn)
                {
                    entity.stateMachine.GoToState(State.SPAWN);
                }
            }

            public override void Execute() { }
            public override void Exit() { }
        }
    }

    // ==== Public Properties ================================== ))
    public Vector3 currentPosition
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Quaternion currentRotation
    {
        get => transform.rotation;
        set => transform.rotation = currentRotation;
    }

    public Data data
    {
        get
        {
            if (_data == null)
            {
                if (_preset != null) { _data = new Data(_preset); }
                else { _data = new Data(); }
            }
            return _data;
        }
    }

    public StageEntityPreset preset
    {
        get => _preset;
        set => LoadPreset(value);
    }

    public StateMachine stateMachine
    {
        get
        {
            if (_stateMachine == null)
            {
                _stateMachine = new StateMachine(this);
            }
            return _stateMachine;
        }
    }

    // ==== Private Properties =================================  ))
    StateMachine _stateMachine;
    Data _data;

    // ==== Protected Properties ================================= ))
    protected StageManager stageManager => StageManager.Instance;
    protected Rigidbody rb => GetComponent<Rigidbody>();
    protected CapsuleCollider col => GetComponent<CapsuleCollider>();
    protected int id => GetInstanceID();

    // ==== Serialized Fields =================================== ))
    [Expandable, SerializeField] protected StageEntityPreset _preset;
    private void LoadPreset(StageEntityPreset preset)
    {
        if (preset == null) return;
        _preset = preset;
        _data = new Data(preset);
    }

    [Space(10), HorizontalLine(), Header("Live Data")]
    [SerializeField, ShowOnly] protected State _currentState;
    [SerializeField, ShowOnly] protected float _curr_moveSpeed;
    [SerializeField, ShowOnly] protected float _curr_moveSpeed_offset; // The current offset value for the movement speed
    [SerializeField, ShowOnly] protected float _curr_rotAngle; // The current rotation angle of the entity
    [SerializeField, ShowOnly] protected float _target_rotAngle;
    public void SetTargetRotation(float angle, bool instant = false)
    {
        _target_rotAngle = angle;

        if (instant)
        {
            _curr_rotAngle = angle;
            SetRotation(angle);
        }
    }

    // ======================== [[ UNITY METHODS ]] ======================== >>

    public virtual void Start() { Initialize(); }
    public virtual void FixedUpdate()
    {
        UpdateMovement();

        // Update the state machine
        _stateMachine.Step();
    }

    public virtual void OnDrawGizmos()
    {
        Vector3 entityPos = currentPosition;

        // Get the target position from _rotationDirection using pythagorean theorem
        Vector3 targetPos = CalculateTargetPosition(entityPos, _target_rotAngle, data.moveSpeed * 2);

        // Draw the target position and the line to it
        Gizmos.color = Color.red;
        Gizmos.DrawLine(entityPos, targetPos);
        Gizmos.DrawCube(targetPos, data.colliderRadius * 0.5f * Vector3.one);

        // Draw the current velocity
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(entityPos, entityPos + rb.velocity);
    }

    // ======================== [[ BASE METHODS ]] ======================== >>

    /// <summary>
    /// Initialize the object with the given settings & assign the entity to the stage
    /// </summary>
    public virtual void Initialize()
    {
        Debug.Log("Initializing " + gameObject.name);

        // Load the preset data
        if (_preset != null) { LoadPreset(_preset); }

        // Create the state machine
        _stateMachine = new StateMachine(this);
        _currentState = _stateMachine.CurrentState;
        _stateMachine.OnStateChanged += (state) =>
        {
            _currentState = state;
        };


        // Assign the collider settings
        col.height = data.colliderHeight;
        col.radius = data.colliderRadius;
        col.direction = 2; // Set to the Z axis , inline with the forward direction of the object
        col.center = Vector3.zero;



        // Destroy this object after the lifespan
        if (Application.isPlaying && data.lifeSpan > 0)
        {
            Destroy(gameObject, data.lifeSpan);
        }
    }

    protected virtual void UpdateMovement()
    {
        // << FORCE >> ---------------- >>
        // Assign the general thrust velocity of the entity
        Vector3 thrustVelocity = transform.forward * (data.moveSpeed + _curr_moveSpeed_offset);

        // Calculate the current wind velocity
        float windDirection = StageManager.WindDirection;
        float windIntensity = StageManager.WindIntensity;
        float windResistance = data.windResistance;

        // Calculate the Quaternion for the wind direction
        Vector3 windVelocity = Quaternion.AngleAxis(windDirection, Vector3.up) * Vector3.forward;
        windVelocity *= windIntensity; // Multiply by the wind intensity
        windVelocity -= (windVelocity * windResistance); // Subtract the wind resistance

        // Assign the calculated velocity to the rigidbody
        rb.velocity = thrustVelocity + windVelocity;

        // << ROTATION >> ---------------- >>
        // Store the current and target rotation values in Euler Angles
        Vector3 vec3_currentRotation = currentRotation.eulerAngles;
        Vector3 vec3_targetRotation = new Vector3(vec3_currentRotation.x, _target_rotAngle, vec3_currentRotation.z);

        // Convert to Quaternions
        Quaternion q_currentRotation = currentRotation;
        Quaternion q_targetRotation = Quaternion.Euler(vec3_targetRotation);

        // Slerp the current rotation to the target rotation
        transform.rotation = Quaternion.Slerp(q_currentRotation, q_targetRotation, data.rotationSpeed * Time.fixedDeltaTime);

	// Update the current rotation angle
	_curr_rotAngle = currentRotation.eulerAngles.y;
    }

    protected virtual void ResetMovement()
    {
        _curr_moveSpeed_offset = 0;

        // Set the target rotation to the current rotation
        _target_rotAngle = currentRotation.eulerAngles.y;
    }

    protected void SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    protected bool IsInStageBounds()
    {
        return stageManager.IsColliderInArea(col, StageManager.AreaType.STAGE);
    }

    protected bool IsInSpawnBounds()
    {
        return stageManager.IsColliderInArea(col, StageManager.AreaType.SPAWN_AREA);
    }

    /// <summary>
    /// Called when the object is out of bounds
    /// </summary>
    protected virtual void OnStageExit(bool respawn)
    {
        if (respawn)
        {
            Vector3 antipodalPoint = StageManager.Instance.GetAntipodalPoint(currentPosition);
            transform.position = antipodalPoint;
            return;
        }

        // Destroy the object by default
        if (Application.isPlaying)
        {
            Debug.Log("Destroying " + gameObject.name);
            Destroy(gameObject);
        }
    }

    protected Vector3 CalculateTargetPosition(Vector3 center, float yRotation, float magnitude)
    {
        // Convert Y-axis rotation to radians
        float radians = yRotation * Mathf.Deg2Rad;

        // Calculate the direction vector components using the Pythagorean theorem (cos and sin for XZ plane)
        float xComponent = magnitude * Mathf.Sin(radians);
        float zComponent = magnitude * Mathf.Cos(radians);

        // Add the direction components to the center point to get the final position
        Vector3 position = new Vector3(center.x + xComponent, center.y, center.z + zComponent);

        return position;
    }


}


#if UNITY_EDITOR
[CustomEditor(typeof(StageEntity), true)]
public class StageEntityCustomEditor : Editor
{
    SerializedObject _serializedObject;
    StageEntity _script;
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(target);
        _script = (StageEntity)target;
        _script.Initialize();
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                _script.Initialize(); // << Assign the new values
            }
        }
    }
}
#endif