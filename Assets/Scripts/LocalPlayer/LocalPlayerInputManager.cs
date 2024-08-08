using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Darklight.UnityExt.Behaviour;
using Darklight.UnityExt.Editor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[RequireComponent(typeof(PlayerInputManager))]
public class LocalPlayerInputManager : MonoBehaviourSingleton<LocalPlayerInputManager>
{

    // Data ===================================== >>>>
    [SerializeField, ShowOnly] int _maxPlayerCount = 8;
    [SerializeField] List<LocalPlayerInputData> _inputData = new LocalPlayerInputData[16].ToList();

    // References ===================================== >>>>
    public PlayerInputManager inputManager => GetComponent<PlayerInputManager>();
    public List<InputDevice> allDevices => _inputData.Select(x => x.device).ToList();
    public int currentPlayerCount => _inputData.Count;

    // Events ===================================== >>>>
    public delegate void InputDataEvent(LocalPlayerInputData data);
    public event InputDataEvent OnAddLocalPlayerInput;
    public event InputDataEvent OnRemoveLocalPlayerInput;

    // Methods ===================================== >>>>
    public override void Initialize()
    {
        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;
    }

    void Update()
    {
        foreach (LocalPlayerInputData data in _inputData)
        {
            if (data == null || data.playerInput == null) continue;
            data.UpdateData();
        }
    }

    /// <summary>
    /// Called when a player joins the game.
    /// This method is called by the PlayerInputManager event : onPlayerJoined.
    /// </summary>
    /// <param name="playerInput">
    ///     The PlayerInput object of the player that joined.
    /// </param>
    void OnPlayerJoined(PlayerInput playerInput)
    {
        HandleNewInput(playerInput);
    }

    /// <summary>
    /// Called when a player leaves the game.
    /// This method is called by the PlayerInputManager event : onPlayerLeft.
    /// </summary>
    /// <param name="playerInput">
    ///     The PlayerInput object of the player that left.
    /// </param>
    void OnPlayerLeft(PlayerInput playerInput)
    {
        //RemovePlayerInput(playerInput);
    }

    void HandleNewInput(PlayerInput playerInput)
    {
        LocalPlayerInputData newData = new LocalPlayerInputData(playerInput);

        // Check if the max players are reached        
        if (_inputData.Count >= _maxPlayerCount)
        {
            Debug.Log($"{Prefix} Max players reached! >> Cannot connect [ {newData.GetDeviceInfo()} ]");
            return;
        }

        _inputData.Add(newData);
        OnAddLocalPlayerInput?.Invoke(newData);
    }

    public void RemoveDuplicateData(LocalPlayerInputData data)
    {
        if (data == null) return;

        List<LocalPlayerInputData> duplicates = new();
        foreach (LocalPlayerInputData item in _inputData)
        {
            if (IsDataDuplicate(data, item))
            {
                duplicates.Add(item);
            }
        }


        foreach (LocalPlayerInputData duplicate in duplicates)
        {
            _inputData.Remove(duplicate);
            Destroy(duplicate.playerInput.gameObject);
            OnRemoveLocalPlayerInput?.Invoke(duplicate);
        }
    }

    bool IsDataDuplicate(LocalPlayerInputData data1, LocalPlayerInputData data2)
    {
        // False if data is null or the device is the same ID
        if (data1 == null || data2 == null) return false;
        if (data1 == data2 || data1.deviceID == data2.deviceID) return false;

        bool sameType = data1.deviceType == data2.deviceType;
        bool sameCreatedTime = Math.Abs(data1.createdTime - data2.createdTime) < 0.1f;
        bool sameLastUpdateTime = Math.Abs(data1.device_lastUpdateTime - data2.device_lastUpdateTime) < 0.1f;
        bool sameLastInputTime = Math.Abs(data1.input_lastUpdateTime - data2.input_lastUpdateTime) < 0.1f;
        if (sameType && (sameCreatedTime || sameLastUpdateTime || sameLastInputTime))
        {
            return true;
        }
        return false;
    }

    public void RumbleGamepad(Gamepad gamepad, float intensity, float duration)
    {
        StartCoroutine(RumbleGamepadCoroutine(gamepad, intensity, duration));
    }

    IEnumerator RumbleGamepadCoroutine(Gamepad gamepad, float intensity, float duration)
    {
        gamepad.SetMotorSpeeds(intensity, intensity);
        yield return new WaitForSeconds(duration);
        gamepad.SetMotorSpeeds(0, 0);
    }
}
