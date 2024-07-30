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
    public Vector3 position
    {
        get => transform.position;
        set => transform.position = value;
    }


    // ==== Private Properties ================================= ))
    StageManager _stageManager => StageManager.Instance;
    Rigidbody _rb => GetComponent<Rigidbody>();
    CapsuleCollider _collider => GetComponent<CapsuleCollider>();


    // ==== Serialized Fields ================================== >>

    // ---- Collider ----
    [SerializeField] private float _colliderHeight = 10.0f;
    [SerializeField] private float _colliderRadius = 5.0f;

    // ---- Movement ----
    [SerializeField, Range(-360, 360)] private float _rotationDirection = 0;
    [SerializeField] private float _moveSpeed = 10.0f;
    [SerializeField] private float _rotationSpeed = 10.0f;

    // ======================== [[ UNITY METHODS ]] ======================== >>

    public virtual void Start() { Initialize(); }
    public virtual void Update() { UpdateMovement(); }

    public virtual void OnDrawGizmosSelected()
    {
        Vector3 entityPos = position;

        // Get the target position from _rotationDirection using pythagorean theorem
        Vector3 targetPos = CalculateTargetPosition(entityPos, _rotationDirection, _moveSpeed * 5);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(entityPos, targetPos);
        Gizmos.DrawCube(targetPos, _colliderRadius * 0.5f * Vector3.one);
    }

    // ======================== [[ BASE METHODS ]] ======================== >>
    public virtual void Initialize()
    {
        // Assign the collider settings
        _collider.height = _colliderHeight;
        _collider.radius = _colliderRadius;
        _collider.direction = 2; // Set to the Z axis , inline with the forward direction of the object
        _collider.center = Vector3.zero;

        // Assign the height of the object to the stage height
        StageManager.AssignEntityToStage(this, _colliderHeight);

        // Assign the rotation value
        Vector3 rotation = transform.rotation.eulerAngles;
        rotation.y = _rotationDirection;
        transform.rotation = Quaternion.Euler(rotation);
    }

    protected virtual void UpdateMovement()
    {
        // << FORCE >> ---------------- >>
        // Assign the general thrust velocity of the entity
        Vector3 thrustVelocity = transform.forward * _moveSpeed;
        _rb.velocity = thrustVelocity;

        // << ROTATION >> ---------------- >>
        // Store the current and target rotation values in Euler Angles
        Vector3 vec3_currentRotation = transform.rotation.eulerAngles;
        Vector3 vec3_targetRotation = new Vector3(vec3_currentRotation.x, _rotationDirection, vec3_currentRotation.z);

        // Convert to Quaternions
        Quaternion q_currentRotation = Quaternion.Euler(vec3_currentRotation);
        Quaternion q_targetRotation = Quaternion.Euler(vec3_targetRotation);

        // Slerp the current rotation to the target rotation
        Quaternion lerpedRotation = Quaternion.Slerp(q_currentRotation, q_targetRotation, _rotationSpeed * Time.fixedDeltaTime);

        // Assign the new rotation value
        transform.rotation = lerpedRotation;
    }

    protected virtual void OnStageExit()
    {
        // Destroy the object by default
        if (Application.isPlaying)
        {
            Debug.Log("Destroying " + gameObject.name);
            Destroy(gameObject);
        }
    }

    Vector3 CalculateTargetPosition(Vector3 center, float yRotation, float magnitude)
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