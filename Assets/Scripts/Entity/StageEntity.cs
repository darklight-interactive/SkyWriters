using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;
using Darklight.UnityExt.Behaviour;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class StageEntity : MonoBehaviour
{
    /// <summary>
    /// An enum to represent the entity's class type
    /// </summary>
    public enum Class { NULL, PLANE, CLOUD, BLIMP }

    /// <summary>
    /// An enum to represent the entity's state
    /// </summary>
    public enum State { NULL, SPAWN, GAME, DESPAWN }

    #region -------- << DATA CLASS >>
    [System.Serializable]
    public class Data
    {
        [Header("---- Identifiers ----")]
        [SerializeField] Class _entityClass = Class.NULL;
        public Type entityType => EntityRegistry.GetTypeFromClass(_entityClass);
        public Class entityClass => _entityClass;

        [Header("---- Rules ----")]
        [SerializeField] bool _respawnOnExit = true;
        public bool respawnOnExit => _respawnOnExit;

        [Header("---- Collider ----")]
        [SerializeField] float _colliderHeight = 10f;
        [SerializeField] float _colliderRadius = 5f;
        public float colliderHeight => _colliderHeight;
        public float colliderRadius => _colliderRadius;

        [Header("---- Movement ----")]
        [SerializeField] float _moveSpeed = 10f;
        [SerializeField] float _rotationSpeed = 5f;
        public float moveSpeed => _moveSpeed;
        public float rotationSpeed => _rotationSpeed;

        [Header("---- Stats ----")]
        [SerializeField] float _windResistance = 0.2f;
        public float windResistance => _windResistance;

        [Header("---- Gameplay ----")]
        [SerializeField] float _lifeSpan = -1f;
        public float lifeSpan => _lifeSpan;

        [Header("---- VFX ----")]
        public VFX_ColorDataObject _startColor;

        // ---- Constructors ----
        public Data() { }
        public Data(Data originData)
        {
            _entityClass = originData.entityClass;

            _respawnOnExit = originData.respawnOnExit;
            _colliderHeight = originData.colliderHeight;
            _colliderRadius = originData.colliderRadius;
            _moveSpeed = originData.moveSpeed;
            _rotationSpeed = originData.rotationSpeed;
            _windResistance = originData.windResistance;
            _lifeSpan = originData.lifeSpan;

            _startColor = originData._startColor;
        }
    }
    #endregion

    #region -------- << STATE MACHINE >>
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

                // Check if the entity has exited the game bounds
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
    #endregion

    // ==== Private Properties =================================  ))
    StateMachine _stateMachine;
    Data _entityData;
    Class _entityClass;


    #region -------- ( References )
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

    public Data data
    {
        get
        {
            if (_entityData == null)
            {
                if (_settings != null) { _entityData = new Data(_settings.data); }
                else { _entityData = new Data(); }
            }
            return _entityData;
        }
    }

    public Class entityClass
    {
        get
        {
            if (_settings != null) { _entityClass = _settings.data.entityClass; }
            return _entityClass;
        }
    }

    public Vector3 position
    {
        get => transform.position;
        set => transform.position = value;
    }

    public Quaternion rotation
    {
        get => transform.rotation;
        set => transform.rotation = rotation;
    }

    public Vector3 velocity
    {
        get => rb.velocity;
        set => rb.velocity = value;
    }
    #endregion

    // ==== Public Events ======================================  ))
    public delegate void ColliderTriggerEvent(Collider other);
    public event ColliderTriggerEvent OnTriggerEntered;
    public event ColliderTriggerEvent OnTriggerExited;

    // ==== Protected Properties ================================= ))
    protected Stage stageManager => Stage.Instance;
    protected Rigidbody rb => GetComponent<Rigidbody>();
    protected CapsuleCollider col => GetComponent<CapsuleCollider>();
    protected int id => GetInstanceID();
    protected EntitySettings settings => _settings;

    // ==== Serialized Fields =================================== ))
    [Header("Live Data")]
    [SerializeField] protected VFX_ColorDataObject currentColorDataObject;
    [SerializeField, ShowOnly] protected State currentState; // The current state of the entity
    [SerializeField, ShowOnly] protected float currSpeed; // The current speed of the entity
    [SerializeField, ShowOnly] protected float currSpeedMultiplier = 1; // The current speed multiplier of the entity
    [SerializeField, ShowOnly] protected float curr_rotAngle; // The current rotation angle of the entity
    [SerializeField, ShowOnly] protected float target_rotAngle; // The target rotation angle of the entity

    [HorizontalLine(), Header("Entity Settings")]
    [Expandable, SerializeField] protected EntitySettings _settings;

    #region ======================== [[ UNITY METHODS ]] ======================== >>

    public virtual void Start() { Initialize(); }
    public virtual void FixedUpdate()
    {
        UpdateMovement();

        // Update the state machine
        if (_stateMachine != null) _stateMachine.Step();
    }

    void OnTriggerEnter(Collider other) { OnTriggerEntered?.Invoke(other); }
    void OnTriggerExit(Collider other) { OnTriggerExited?.Invoke(other); }

    public virtual void OnDrawGizmos()
    {
        Vector3 entityPos = position;

        // Get the target position from _rotationDirection using pythagorean theorem
        Vector3 targetPos = CalculateTargetPosition(entityPos, target_rotAngle, data.moveSpeed * 2);

        // Draw the target position and the line to it
        Gizmos.color = Color.red;
        Gizmos.DrawLine(entityPos, targetPos);
        Gizmos.DrawCube(targetPos, data.colliderRadius * 0.1f * Vector3.one);

        // Draw the current velocity
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(entityPos, entityPos + rb.velocity);
    }
    #endregion

    #region ======================== [[ ENTITY METHODS ]] ======================== >>
    /// <summary>
    /// Initialize the object with the given settings & assign the entity to the stage
    /// </summary>
    public virtual void Initialize(EntitySettings settings = null)
    {
        if (settings != null)
            LoadSettings(settings);

        DestroyAllParticles();

        // Set up entity for play mode
        if (Application.isPlaying)
        {
            // Create the state machine
            _stateMachine = new StateMachine(this);
            currentState = _stateMachine.CurrentState;
            _stateMachine.OnStateChanged += (state) =>
            {
                currentState = state;
            };

            // Destroy this object after the lifespan
            if (data.lifeSpan > 0)
                Destroy(gameObject, data.lifeSpan);
        }
    }

    public virtual void LoadSettings(EntitySettings settings)
    {
        if (settings == null) return;
        _settings = settings;
        _entityData = new Data(settings.data);

        // Set the collider values
        col.height = data.colliderHeight;
        col.radius = data.colliderRadius;
        col.direction = 2; // Set to the Z axis , inline with the forward direction of the object
        col.center = Vector3.zero;
    }

    public virtual void ReloadSettings()
    {
        if (_settings == null)
        {
            Debug.LogError("No settings found for " + gameObject.name, this);
            return;
        }

        LoadSettings(_settings);
    }

    protected virtual void UpdateMovement()
    {
        // << FORCE >> ---------------- >>
        // Assign the general thrust velocity of the entity
        Vector3 thrustVelocity = transform.forward * (data.moveSpeed * currSpeedMultiplier);
        currSpeed = thrustVelocity.magnitude; // << Update the current speed

        // Calculate the current wind velocity
        float windDirection = Stage.Settings.windDirection;
        float windIntensity = Stage.Settings.windIntensity;
        float windResistance = data.windResistance;

        // Calculate the Quaternion for the wind direction
        Vector3 windVelocity = Quaternion.AngleAxis(windDirection, Vector3.up) * Vector3.forward;
        windVelocity *= windIntensity; // Multiply by the wind intensity
        windVelocity -= (windVelocity * windResistance); // Subtract the wind resistance

        // Assign the calculated velocity to the rigidbody
        velocity = thrustVelocity + windVelocity;

        // << ROTATION >> ---------------- >>
        // Store the current and target rotation values in Euler Angles
        Vector3 vec3_currentRotation = rotation.eulerAngles;
        Vector3 vec3_targetRotation = new Vector3(vec3_currentRotation.x, target_rotAngle, vec3_currentRotation.z);

        // Convert to Quaternions
        Quaternion q_currentRotation = rotation;
        Quaternion q_targetRotation = Quaternion.Euler(vec3_targetRotation);

        // Slerp the current rotation to the target rotation
        transform.rotation = Quaternion.Slerp(q_currentRotation, q_targetRotation, data.rotationSpeed * Time.fixedDeltaTime);

        // Update the current rotation angle
        curr_rotAngle = rotation.eulerAngles.y;
    }

    protected virtual void ResetMovement()
    {
        currSpeedMultiplier = 0;

        // Set the target rotation to the current rotation
        target_rotAngle = rotation.eulerAngles.y;
    }
    #endregion

    public void SetTargetRotation(Vector3 targetPosition, bool instant = false)
    {
        Vector3 direction = targetPosition - position;
        float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        SetTargetRotation(angle, instant);
    }

    public void SetTargetRotation(float angle, bool instant = false)
    {
        target_rotAngle = angle;

        if (instant)
        {
            curr_rotAngle = angle;
            SetRotation(angle);
        }
    }

    public void SetTargetRotation(float offsetAngle)
    {
        target_rotAngle = curr_rotAngle + offsetAngle;
    }

    protected void SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    protected bool IsInStageBounds()
    {
        return Stage.IsPositionWithinRadius(position, Stage.Settings.stageRadius);
    }

    protected bool IsInSpawnBounds()
    {
        return Stage.IsPositionWithinRadiusRange(position, Stage.Settings.stageRadius, Stage.Settings.spawnRadius);
    }

    /// <summary>
    /// Called when the object is out of bounds
    /// </summary>
    protected virtual void OnStageExit(bool respawn)
    {
        if (respawn)
        {
            Vector3 antipodalPoint = Shape2DUtility.CalculateAntipodalPoint(stageManager.stageCenter, position);
            transform.position = antipodalPoint;
            return;
        }

        // Destroy the object by default
        if (Application.isPlaying)
        {
            //Debug.Log("Destroying " + gameObject.name);
            EntityRegistry.RemoveFromRegistry(this);
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

    public void DestroyAllParticles()
    {
        // Get all the VFX_ParticleSystemHandler objects in children
        VFX_ParticleSystemHandler[] particles = GetComponentsInChildren<VFX_ParticleSystemHandler>();
        for (int i = 0; i < particles.Length; i++)
        {
            if (Application.isPlaying)
                Destroy(particles[i].gameObject);
            else
                DestroyImmediate(particles[i].gameObject);
        }
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
    }

    public override void OnInspectorGUI()
    {
        _serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        // << HORIZONTAL BUTTONS >> ---------------- >>
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Initialize"))
        {
            _script.Initialize();
        }
        if (GUILayout.Button("Reload Settings"))
        {
            _script.ReloadSettings();
        }
        EditorGUILayout.EndHorizontal();

        // << DRAW DEFAULT INSPECTOR >> ---------------- >>
        EditorGUILayout.Space();
        CustomInspectorGUI.DrawDefaultInspectorWithoutSelfReference(_serializedObject);

        // << APPLY CHANGES >> ---------------- >>
        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.ReloadSettings();
        }
    }
}
#endif