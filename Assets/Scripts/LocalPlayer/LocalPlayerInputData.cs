using System.Collections;
using Darklight.UnityExt.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class LocalPlayerInputData
{
    public enum DEVICE_TYPE { NULL, KEYBOARD, GAMEPAD }

    [ShowOnly, SerializeField] float _createdTime = 0f;
    public float createdTime => _createdTime;

    // ============= Player Info =============
    PlayerInput _playerInput;
    [ShowOnly, SerializeField] int _playerId = -1;
    [ShowOnly, SerializeField] string _playerName = "NULL";
    [ShowOnly, SerializeField] float _input_lastUpdateTime = 0f;
    public PlayerInput playerInput => _playerInput;
    public int playerId => _playerInput.playerIndex;
    public string playerName => _playerName;
    public float input_lastUpdateTime => _input_lastUpdateTime;

    // ============= Device Info =============
    InputDevice _device;
    [ShowOnly, SerializeField] int _deviceID = -1;
    [ShowOnly, SerializeField] string _deviceName = "NULL";
    [ShowOnly, SerializeField] DEVICE_TYPE _deviceType = DEVICE_TYPE.NULL;
    [ShowOnly, SerializeField] double _device_lastUpdateTime = 0f;
    public InputDevice device => _device;
    public int deviceID => _deviceID;
    public string deviceName => _deviceName;
    public DEVICE_TYPE deviceType => _deviceType;
    public double device_lastUpdateTime => _device_lastUpdateTime;

    // ============= Constructor =============
    public LocalPlayerInputData(PlayerInput playerInput)
    {
        _createdTime = Time.time;

        _playerInput = playerInput;
        _playerInput.onActionTriggered += OnActionTriggered;

        UpdateData();
    }

    public void UpdateData()
    {
        _playerId = playerInput.playerIndex;
        _playerName = $"PLAYER{_playerId}";

        _device = playerInput.devices[0];
        _deviceID = device.deviceId;
        _deviceName = device.name;
        _deviceType = GetDeviceType();
        _device_lastUpdateTime = _device.lastUpdateTime;

        // Set the name of the gameobject
        _playerInput.gameObject.name = $"<INPUT> {playerName} : {deviceType}";
    }

    void OnActionTriggered(InputAction.CallbackContext context)
    {
        // Debug.Log($"<INPUT> {playerName} : {deviceType} : {context.action.name} : {context.phase}");
        _input_lastUpdateTime = Time.time;

        LocalPlayerInputManager.Instance.RemoveDuplicateData(this);
    }

    DEVICE_TYPE GetDeviceType()
    {
        if (device is Keyboard)
        {
            return DEVICE_TYPE.KEYBOARD;
        }
        else if (device is Gamepad)
        {
            return DEVICE_TYPE.GAMEPAD;
        }
        else
        {
            return DEVICE_TYPE.NULL;
        }
    }

    public string GetDeviceInfo()
    {
        return $"{deviceName} : {deviceID}";
    }

}