using System;
using System.Collections;
using System.Collections.Generic;
using Darklight.UnityExt.Editor;
using Darklight.UnityExt.FMODExt;
using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneEntity : StageEntity
{
    const int MAX_CONTRAIL_COLORS = 3;

    VFX_ParticleSystemHandler _leftContrail;
    VFX_ParticleSystemHandler _rightContrail;


    [Header("Local Player Input Data")]
    [SerializeField] LocalPlayerInputData _input;

    [Header("Autopilot")]
    [ShowOnly, SerializeField] bool _isAutopilot = true;
    public bool IsAutopilot => _isAutopilot;

    [SerializeField] Transform _planeBody;

    [Header("Movement")]
    [SerializeField, ShowOnly] float _currSpeedPercentage = 0f;

    [Header("Audio")]
    [SerializeField] EventReference _humEvent;
    private EventInstance _humInstance;


    [Header("Contrails")]
    [SerializeField] float _contrailWingspan;

    [Space(10)]
    [SerializeField] List<VFX_ColorDataObject> _contrailColors;

    public override void Initialize(EntitySettings settings)
    {
        base.Initialize(settings);

        if (Application.isPlaying)
        {
            if (_input == null) ActivateAutopilot();

            // Create the hum sound event instance
            _humInstance = FMODUnity.RuntimeManager.CreateInstance(_humEvent);
            _humInstance.setParameterByName("PlaneSpeed", 0f);
            _humInstance.start();

            ParticleSystem burstParticles = VFX_Manager.Instance.cloudBurstParticles;
            VFX_ParticleSystemHandler burstHandler = VFX_Manager.CreateParticleSystemHandler(burstParticles);

            GenerateNewContrails();
        }
    }

    public override void ReloadSettings()
    {
        base.ReloadSettings();
    }

    public void Update()
    {
        if (_humInstance.isValid())
        {
            _humInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
            _humInstance.setParameterByName("PlaneSpeed", GetSpeedPercentage());
        }
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(transform.position + Vector3.right * _contrailWingspan, 1f);
        Gizmos.DrawSphere(transform.position + Vector3.left * _contrailWingspan, 1f);
    }

    #region ======================= [[ INPUT HANDLING ]] =======================
    public void AssignPlayerInput(LocalPlayerInputData inputData)
    {
        _input = inputData;
        this.name = $"({inputData.playerName}) : PlaneEntity";

        DeactivateAutopilot();
    }
    #endregion


    #region ======================= [[ MOVEMENT CONTROLLER ]] =======================
    Vector2 GetMovementInput()
    {
        if (_input != null)
        {
            PlayerInput playerInput = _input.playerInput;
            return playerInput.actions["MoveInput"].ReadValue<Vector2>();
        }
        return Vector2.zero;
    }

    float GetSpeedPercentage()
    {
        PlaneEntitySettings settings = (PlaneEntitySettings)base.settings;

        float currentSpeed = velocity.magnitude;
        float maxSpeed = data.moveSpeed * settings.speedMultiplier_fast;
        _currSpeedPercentage = currentSpeed / maxSpeed;
        return _currSpeedPercentage;
    }

    void ApplyMovementInput(Vector2 moveInput)
    {
        // Set the target rotation value based on the direction of the x input
        float horz_inputDirection = moveInput.x * -90;
        target_rotAngle = rotation.eulerAngles.y + horz_inputDirection;

        PlaneEntitySettings settings = (PlaneEntitySettings)base.settings;

        // Set the speed offset based on the direction of the z input
        if (moveInput.y > 0.5f)
        {
            currSpeedMultiplier = Mathf.Lerp(currSpeedMultiplier, settings.speedMultiplier_fast, settings.accelerationSpeed * Time.fixedDeltaTime);
        }
        else if (moveInput.y < -0.5f)
        {
            currSpeedMultiplier = Mathf.Lerp(currSpeedMultiplier, settings.speedMultiplier_slow, settings.accelerationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            currSpeedMultiplier = Mathf.Lerp(currSpeedMultiplier, 1f, settings.accelerationSpeed * Time.fixedDeltaTime);
        }


    }

    protected override void UpdateMovement()
    {
        if (_input != null && !_isAutopilot)
        {
            Vector2 moveInput = GetMovementInput();
            ApplyMovementInput(moveInput);
        }

        base.UpdateMovement();
        PlaneEntitySettings settings = (PlaneEntitySettings)base.settings;

        // Rotate the plane body on the Z axis based on the current rotation
        Quaternion targetZRotation = Quaternion.Euler(0, 0, curr_rotAngle / 2);
        _planeBody.localRotation = Quaternion.Slerp(_planeBody.localRotation, targetZRotation, settings.accelerationSpeed * Time.fixedDeltaTime);
    }

    protected override void ResetMovement()
    {
        base.ResetMovement();
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
    public void CollectNewColor(VFX_ColorDataObject newColor)
    {
        if (_input.device is Gamepad)
            LocalPlayerInputManager.Instance.RumbleGamepad((Gamepad)_input.device, 0.2f, 0.2f);

        if (_contrailColors.Count >= MAX_CONTRAIL_COLORS - 1)
        {
            // Remove after the max number of colors
            _contrailColors.RemoveRange(MAX_CONTRAIL_COLORS - 1, 1);
        }

        // Add the new color to the front of the list
        _contrailColors.Insert(0, newColor);
        GenerateNewContrails();
    }

    void GenerateNewContrails()
    {
        // Calculate the positions of the contrails
        Vector3 planeCenter = transform.position;
        Vector3 leftContrailPos = planeCenter + Vector3.left * _contrailWingspan;
        Vector3 rightContrailPos = planeCenter + Vector3.right * _contrailWingspan;

        // Create the contrails
        ParticleSystem contrailParticles = VFX_Manager.Instance.contrailParticles;
        if (_leftContrail != null)
            _leftContrail.StopAndDestroyOnComplete();
        _leftContrail = VFX_Manager.CreateParticleSystemHandler(contrailParticles, leftContrailPos, transform);

        if (_rightContrail != null)
            _rightContrail.StopAndDestroyOnComplete();
        _rightContrail = VFX_Manager.CreateParticleSystemHandler(contrailParticles, rightContrailPos, transform);

        // Set the gradient for the contrails
        Gradient _contrailGradient = VFX_Manager.CreateGradient(_contrailColors.ToArray());
        _leftContrail.ApplyGradient(_contrailGradient);
        _rightContrail.ApplyGradient(_contrailGradient);
    }

    #endregion 
}
