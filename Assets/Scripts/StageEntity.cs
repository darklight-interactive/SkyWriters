using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class StageEntity : MonoBehaviour
{
    // ==== Public Properties ================================== ))
    public enum Type { NULL, PLANE, CLOUD, BLIMP }

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


    // ==== Protected Properties ================================= ))
    protected StageManager _stageManager => StageManager.Instance;
    protected Rigidbody _rb => GetComponent<Rigidbody>();
    protected CapsuleCollider _collider => GetComponent<CapsuleCollider>();

    // ---- Entity Type ----
    [SerializeField, ShowOnly] protected Type _typeKey = Type.NULL;
    [SerializeField, ShowOnly] protected int _entityId = -1;

    // ---- Spawn ----
    [SerializeField] protected bool _respawnOnExit = true;

    // ---- Collider ----
    [SerializeField] protected float _colliderHeight = 10.0f;
    [SerializeField] protected float _colliderRadius = 5.0f;

    // ---- Movement ----
    [SerializeField] protected float _moveSpeed = 10.0f;
    [SerializeField] protected float _moveSpeedAmplifier = 0;

    // ---- Rotation ----
    [SerializeField] protected float _rotationSpeed = 10.0f;
    [SerializeField, Range(-360, 360)] protected float _rotationAngle = 0;
    protected float _target_rotationAngle = 0;

    // ---- Gameplay ----

    [Tooltip("The lifespan of the object in seconds. Set to 0 to disable.")]
    [SerializeField] private float _lifeSpan = -1f;

    // ======================== [[ UNITY METHODS ]] ======================== >>

    public virtual void Start() { Initialize(); }
    public virtual void FixedUpdate()
    {
        UpdateMovement();

        // Check if the object is out of bounds
        if (!_stageManager.IsColliderInArea(_collider, StageManager.AreaType.STAGE))
        {
            OnStageExit(_respawnOnExit);
        }
    }

    public virtual void OnDrawGizmos()
    {
        Vector3 entityPos = currentPosition;

        // Get the target position from _rotationDirection using pythagorean theorem
        Vector3 targetPos = CalculateTargetPosition(entityPos, _target_rotationAngle, _moveSpeed * 2);

        // Draw the target position and the line to it
        Gizmos.color = Color.red;
        Gizmos.DrawLine(entityPos, targetPos);
        Gizmos.DrawCube(targetPos, _colliderRadius * 0.5f * Vector3.one);

        // Draw the current velocity
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(entityPos, entityPos + _rb.velocity);
    }

    // ======================== [[ BASE METHODS ]] ======================== >>

    /// <summary>
    /// Initialize the object with the given settings & assign the entity to the stage
    /// </summary>
    public virtual void Initialize(Type type = Type.NULL)
    {
        // Confirm default type key
        _typeKey = type;


        // Assign the collider settings
        _collider.height = _colliderHeight;
        _collider.radius = _colliderRadius;
        _collider.direction = 2; // Set to the Z axis , inline with the forward direction of the object
        _collider.center = Vector3.zero;

        // Assign the height of the object to the stage height
        StageManager.AssignEntityToStage(this, _colliderHeight);

        // Assign the rotation value
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y = _rotationAngle;
        transform.rotation = Quaternion.Euler(rotation);

        // Destroy this object after the lifespan
        if (Application.isPlaying && _lifeSpan > 0)
        {
            Destroy(gameObject, _lifeSpan);
        }
    }

    protected virtual void UpdateMovement()
    {
        // << FORCE >> ---------------- >>
        // Assign the general thrust velocity of the entity
        Vector3 thrustVelocity = transform.forward * (_moveSpeed + _moveSpeedAmplifier);
        _rb.velocity = thrustVelocity;

        // << ROTATION >> ---------------- >>
        // Store the current and target rotation values in Euler Angles
        Vector3 vec3_currentRotation = currentRotation.eulerAngles;
        Vector3 vec3_targetRotation = new Vector3(vec3_currentRotation.x, _target_rotationAngle, vec3_currentRotation.z);

        // Convert to Quaternions
        Quaternion q_currentRotation = currentRotation;
        Quaternion q_targetRotation = Quaternion.Euler(vec3_targetRotation);

        // Slerp the current rotation to the target rotation
        transform.rotation = Quaternion.Slerp(q_currentRotation, q_targetRotation, _rotationSpeed * Time.fixedDeltaTime);
    }

    protected virtual void ResetMovement()
    {
        _moveSpeedAmplifier = 0;

        // Set the target rotation to the current rotation
        _target_rotationAngle = currentRotation.eulerAngles.y;
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