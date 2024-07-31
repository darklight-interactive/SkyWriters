using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class StageEntity : MonoBehaviour
{
    public enum Type { NULL, PLANE, CLOUD, BLIMP }
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

        // ---- Gameplay ----
        public float lifeSpan { get; private set; } = -1;

        public Data() { }
        public Data(Type type, bool respawnOnExit, float colliderHeight, float colliderRadius, float moveSpeed, float rotationSpeed, float lifeSpan)
        {
            this.type = type;
            this.respawnOnExit = respawnOnExit;
            this.colliderHeight = colliderHeight;
            this.colliderRadius = colliderRadius;
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;
            this.lifeSpan = lifeSpan;
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
            if (_preset != null)
                return _preset.ToData();
            return new Data();
        }
    }


    // ==== Protected Properties ================================= ))
    protected StageManager stageManager => StageManager.Instance;
    protected Rigidbody rb => GetComponent<Rigidbody>();
    protected CapsuleCollider col => GetComponent<CapsuleCollider>();
    protected int id => GetInstanceID();

    // ==== Serialized Fields =================================== ))
    [Expandable, SerializeField] protected StageEntityPreset _preset;

    [Space(10), HorizontalLine(), Header("Live Data")]
    [SerializeField, ShowOnly] protected float _curr_moveSpeed_offset; // The current offset value for the movement speed
    [SerializeField, ShowOnly] protected float _curr_rotAngle; // The current rotation angle of the entity
    [SerializeField, ShowOnly] protected float _target_rotAngle;
    private void LoadPreset(StageEntityPreset preset)
    {
        if (preset == null) return;
        _preset = preset;
    }


    // ======================== [[ UNITY METHODS ]] ======================== >>

    public virtual void Start() { Initialize(); }
    public virtual void FixedUpdate()
    {
        UpdateMovement();

        // Check if the object is out of bounds
        if (!stageManager.IsColliderInArea(col, StageManager.AreaType.STAGE))
        {
            OnStageExit(data.respawnOnExit);
        }
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

        // Assign the collider settings
        col.height = data.colliderHeight;
        col.radius = data.colliderRadius;
        col.direction = 2; // Set to the Z axis , inline with the forward direction of the object
        col.center = Vector3.zero;

        // Assign the height of the object to the stage height
        StageManager.AssignEntityToStage(this);

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
        rb.velocity = thrustVelocity;

        // << ROTATION >> ---------------- >>
        // Store the current and target rotation values in Euler Angles
        Vector3 vec3_currentRotation = currentRotation.eulerAngles;
        Vector3 vec3_targetRotation = new Vector3(vec3_currentRotation.x, _target_rotAngle, vec3_currentRotation.z);

        // Convert to Quaternions
        Quaternion q_currentRotation = currentRotation;
        Quaternion q_targetRotation = Quaternion.Euler(vec3_targetRotation);

        // Slerp the current rotation to the target rotation
        transform.rotation = Quaternion.Slerp(q_currentRotation, q_targetRotation, data.rotationSpeed * Time.fixedDeltaTime);
    }

    protected virtual void ResetMovement()
    {
        _curr_moveSpeed_offset = 0;

        // Set the target rotation to the current rotation
        _target_rotAngle = currentRotation.eulerAngles.y;
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