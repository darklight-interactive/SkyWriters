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
        foreach (LocalPlayerInputData playerInputData in _inputData)
        {
            if (playerInputData == null) continue;
            playerInputData.UpdateData();
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
        RemovePlayerInput(playerInput);
    }

    void HandleNewInput(PlayerInput playerInput)
    {
        LocalPlayerInputData newData = new LocalPlayerInputData(playerInput);
        if (IsDataDuplicate(newData))
        {
            Debug.Log($"{Prefix} Device is a duplicate! >> Cannot connect [ {newData.GetDeviceInfo()} ]");
            Destroy(playerInput.gameObject);
            return;
        }

        // Check if the max players are reached        
        if (_inputData.Count >= _maxPlayerCount)
        {
            Debug.Log($"{Prefix} Max players reached! >> Cannot connect [ {newData.GetDeviceInfo()} ]");
            return;
        }

        _inputData.Add(newData);
        OnAddLocalPlayerInput?.Invoke(newData);
    }

    void RemovePlayerInput(PlayerInput playerInput)
    {
        _inputData.RemoveAll(x => x.playerInput == playerInput);
        Destroy(playerInput.gameObject);
    }

    bool IsDataDuplicate(LocalPlayerInputData data)
    {
        if (data == null) return false;
        for (int i = 0; i < _inputData.Count; i++)
        {
            LocalPlayerInputData playerInputData = _inputData[i];
            if (playerInputData == null) continue;

            // Check if the device is a duplicate type and created at the same time
            if (playerInputData.deviceType == data.deviceType &&
                MathF.Abs(playerInputData.createdTime - data.createdTime) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

}
