using Darklight.UnityExt.Editor;
using NaughtyAttributes;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
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
    [SerializeField, Range(0, 360)] private float _rotationDirection = 0;
    [SerializeField] private float _moveSpeed = 10.0f;
    [SerializeField] private float _rotationSpeed = 10.0f;

    // ======================== [[ UNITY METHODS ]] ======================== >>

    [Button]
    public void SetEditorValues()
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

    protected virtual void SetMovement(Vector2 direction)
    {
        // Set the target rotation value based on the direction of the x input
        _rotationDirection = direction.x;
    }

    protected virtual void UpdateMovement()
    {
        // Assign the general thrust velocity of the entity
        Vector3 thrustVelocity = transform.forward * _moveSpeed;
        _rb.velocity = thrustVelocity;

        // << ROTATION >>
        // Slerp the current rotation to the target rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = currentRotation * Quaternion.Euler(0, _rotationDirection, 0);
        Quaternion lerpedRotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = lerpedRotation;
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

        DrawDefaultInspector();

        if (EditorGUI.EndChangeCheck())
        {
            _serializedObject.ApplyModifiedProperties();
            _script.SetEditorValues(); // << Assign the new values
        }
    }
}
#endif