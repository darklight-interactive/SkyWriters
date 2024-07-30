using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Darklight.UnityExt.Input;
using Darklight.UnityExt.Editor;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEditor.VersionControl;

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlaneController : MonoBehaviour
{
    PlayerInput playerInput => GetComponent<PlayerInput>();
    Rigidbody rb => GetComponent<Rigidbody>();
    new Collider collider => GetComponent<Collider>();

    [SerializeField] Transform _planeBody;

    [SerializeField, Range(0, 1000)] float _moveSpeed = 10;
    [SerializeField, Range(0, 100)] float _rotationSpeed = 10;
    [SerializeField, Range(0, 500)] float _speedChangeMagnitude = 10;

    private float _rotationOffset;
    private float _speedOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Freeze the Y position of the plane at the stage's Y position
        transform.position = new Vector3(transform.position.x, StageManager.Instance.transform.position.y, transform.position.z);
        rb.constraints = RigidbodyConstraints.FreezePositionY;

        // Subscribe to the move input events
        //UniversalInputManager.OnMoveInput += SetMovement;
        //UniversalInputManager.OnMoveInputCanceled += ResetMovement;
        playerInput.actions["MoveInput"].performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        playerInput.actions["MoveInput"].canceled += ctx => ResetMovement();

        CreateContrails();
    }

    void FixedUpdate()
    {
        UpdateMovement();

        // Check if the plane is out of bounds
        if (StageManager.Instance.IsColliderInStage(this.collider) == false)
        {
            Vector3 newSpawnPoint = StageManager.Instance.GetAntipodalPoint(this.transform.position);
            this.transform.position = newSpawnPoint;
        }
    }

    #region ======================= [[ SPAWN ]] =======================


    #endregion

    #region ======================= [[ MOVEMENT ]] =======================
    void SetMovement(Vector2 moveInput)
    {
        // Store the move direction on the XZ plane
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
        rb.velocity = direction * _moveSpeed;

        // Set the target rotation value based on the direction of the x input
        _rotationOffset = direction.x * -90;

        // Set the speed offset based on the direction of the z input
        // Clamp the speed offset to the speed change magnitude
        _speedOffset = Mathf.Clamp(direction.z * _speedChangeMagnitude, -_speedChangeMagnitude / 2, _speedChangeMagnitude);
    }

    void UpdateMovement()
    {
        // Set the velocity of the plane to move in the current forward direction
        rb.velocity = transform.forward * (_moveSpeed + _speedOffset);

        // Slerp the current rotation to the target rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y + _rotationOffset, currentRotation.eulerAngles.z);

        // Lerp the current rotation to the target rotation
        Quaternion lerpedTargetRotation = Quaternion.Lerp(currentRotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Slerp(currentRotation, lerpedTargetRotation, _rotationSpeed * Time.fixedDeltaTime);

        // Rotate the plane body on the Z axis based on the current rotation
        Quaternion targetZRotation = Quaternion.Euler(0, 0, _rotationOffset / 2);
        _planeBody.localRotation = Quaternion.Slerp(_planeBody.localRotation, targetZRotation, _rotationSpeed * Time.fixedDeltaTime);
    }

    void ResetMovement()
    {
        _rotationOffset = 0;
        _speedOffset = 0;
    }
    #endregion

    #region ======================= [[ CONTRAILS ]] ============== 

    [Header("Contrails")]
    [SerializeField] GameObject _contrailPrefab;
    [SerializeField] float _contrailWingspan = 5f;
    [SerializeField] float _contrailScale = 10f;
    [SerializeField] Gradient _contrailGradient;

    // Contrail particle system references
    ParticleSystem _leftContrail;
    ParticleSystem _rightContrail;

    void CreateContrails()
    {
        // Calculate the positions of the contrails
        Vector3 planeCenter = transform.position;
        Vector3 leftContrailPos = planeCenter + Vector3.left * _contrailWingspan;
        Vector3 rightContrailPos = planeCenter + Vector3.right * _contrailWingspan;

        // Create the contrail game objects
        GameObject leftContrail = Instantiate(_contrailPrefab, leftContrailPos, Quaternion.identity);
        GameObject rightContrail = Instantiate(_contrailPrefab, rightContrailPos, Quaternion.identity);

        // Set the contrail parents
        leftContrail.transform.SetParent(transform);
        rightContrail.transform.SetParent(transform);

        // Set the contrail scales
        leftContrail.transform.localScale = Vector3.one * _contrailScale;
        rightContrail.transform.localScale = Vector3.one * _contrailScale;

        // Save the particle system references
        _leftContrail = leftContrail.GetComponent<ParticleSystem>();
        _rightContrail = rightContrail.GetComponent<ParticleSystem>();

        // Set the contrail colors
        SetColorOverLifetime(_leftContrail, _contrailGradient);
        SetColorOverLifetime(_rightContrail, _contrailGradient);
    }

    void SetColorOverLifetime(ParticleSystem ps, Gradient colorGradient)
    {
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = colorGradient;
    }

    /// <summary>
    /// Create a new contrail with the given gradient color
    /// </summary>
    /// <param name="gradient"></param>
    public void CreateNewContrail(Gradient gradient)
    {
        // Stop the current contrails
        _leftContrail.Stop();
        _rightContrail.Stop();

        // Start coroutine to check and destroy old contrails
        StartCoroutine(CheckAndDestroyContrail(_leftContrail));
        StartCoroutine(CheckAndDestroyContrail(_rightContrail));

        // Instantiate and configure the new left contrail
        _leftContrail = Instantiate(_leftContrail, _leftContrail.transform.position, _leftContrail.transform.rotation, transform);
        _leftContrail.transform.localScale = Vector3.one * _contrailScale;
        _leftContrail.gameObject.name = "Left Contrail";
        SetColorOverLifetime(_leftContrail, gradient);

        // Instantiate and configure the new right contrail
        _rightContrail = Instantiate(_rightContrail, _rightContrail.transform.position, _rightContrail.transform.rotation, transform);
        _rightContrail.transform.localScale = Vector3.one * _contrailScale;
        _rightContrail.gameObject.name = "Right Contrail";
        SetColorOverLifetime(_rightContrail, gradient);
    }

    private IEnumerator CheckAndDestroyContrail(ParticleSystem contrail)
    {
        // Wait until the particle system has no more particles
        while (contrail.IsAlive(true))
        {
            yield return null;
        }

        // Destroy the particle system game object
        Destroy(contrail.gameObject);
    }
    #endregion 

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(transform.position + Vector3.right * _contrailWingspan, 1f);
        Gizmos.DrawSphere(transform.position + Vector3.left * _contrailWingspan, 1f);
    }
}
