using System;
using System.Collections;

using Darklight.UnityExt.Editor;
using Darklight.UnityExt.FMODExt;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlaneEntity : StageEntity
{
    [SerializeField] PlayerInputData _playerInputData;
    [ShowOnly, SerializeField] bool _isAutopilot = true;
    public bool IsAutopilot => _isAutopilot;
    [SerializeField] Transform _planeBody;
    [SerializeField, Range(0, 500)] float _speedChangeMagnitude = 10;


    [Header("Audio")]
    [SerializeField] EventReference _humEvent;
    private EventInstance _humInstance;

    public override void Initialize()
    {
        base.Initialize();
        if (_playerInputData == null)
        {
            ActivateAutopilot();
        }

        // If the contrails are not set, create them
        if (Application.isPlaying)
        {
            CreateContrails();
        }

        // Create the hum sound event instance
        _humInstance = FMODUnity.RuntimeManager.CreateInstance(_humEvent);
        _humInstance.setParameterByName("PlaneSpeed", 1f);
        _humInstance.start();
    }

    public void Update()
    {
        /*
        if (_humInstance.isValid())
        {
            _humInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            _humInstance.setParameterByName("PlaneSpeed", 0.5f);
        }
        */
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(transform.position + Vector3.right * _contrailWingspan, 1f);
        Gizmos.DrawSphere(transform.position + Vector3.left * _contrailWingspan, 1f);
    }

    #region ======================= [[ INPUT HANDLING ]] =======================

    public void AssignPlayerInput(PlayerInputData input)
    {
        _playerInputData = input;
        DeactivateAutopilot();

        Debug.Log($"Player input assigned to plane: {_playerInputData.GetInfo()}", this);
    }

    #endregion


    #region ======================= [[ MOVEMENT CONTROLLER ]] =======================
    void ApplyMovementInput(Vector2 moveInput)
    {
        // Set the target rotation value based on the direction of the x input
        float horz_inputDirection = moveInput.x * -90;
        _target_rotAngle = currentRotation.eulerAngles.y + horz_inputDirection;

        // Set the speed offset based on the direction of the z input
        // Clamp the speed offset to the speed change magnitude
        _curr_moveSpeed_offset = Mathf.Clamp(moveInput.y * _speedChangeMagnitude, -_speedChangeMagnitude / 4, _speedChangeMagnitude);
    }

    protected override void UpdateMovement()
    {
        if (_playerInputData != null && !_isAutopilot)
        {
            Vector2 moveInput = _playerInputData.ReadMoveInput();
            ApplyMovementInput(moveInput);
        }

        base.UpdateMovement();

        // Rotate the plane body on the Z axis based on the current rotation
        //Quaternion targetZRotation = Quaternion.Euler(0, 0, _curr_rotAngle / 2);
        //_planeBody.localRotation = Quaternion.Slerp(_planeBody.localRotation, targetZRotation, data.rotationSpeed * Time.fixedDeltaTime);
    }

    protected override void ResetMovement()
    {
        base.ResetMovement();
    }

    float GetSpeedPercentage()
    {
        return _curr_moveSpeed / data.moveSpeed;
    }

    #endregion

    #region ======================= [[ AUTOPILOT CONTROL ROUTINE ]] =======================
    private Coroutine _autopilotRoutine;
    public IEnumerator AutopilotRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            ApplyMovementInput(new Vector2(0.5f, 0));

            yield return new WaitForSeconds(1f);
            ApplyMovementInput(new Vector2(1, 1));
        }
    }

    public void ActivateAutopilot()
    {
        if (_isAutopilot) return;
        _isAutopilot = true;
        _autopilotRoutine = StartCoroutine(AutopilotRoutine());
    }

    public void DeactivateAutopilot()
    {
        if (!_isAutopilot) return;
        if (_autopilotRoutine != null)
        {
            StopCoroutine(_autopilotRoutine);
            _autopilotRoutine = null;
        }
        _isAutopilot = false;
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

    void CreateContrails(bool destroyOld = false)
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
}
