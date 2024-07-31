using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class StagePlayerData
{
    public PlayerInput playerInput { get; private set; }
    public int deviceId { get; private set; }
    public string deviceName { get; private set; }
    public int playerId { get; private set; }

    public StagePlayerData(PlayerInput playerInput, int playerIndex = 0)
    {
        this.playerInput = playerInput;
        this.deviceId = playerInput.devices[0].deviceId;
        this.deviceName = playerInput.devices[0].name;
        this.playerId = playerIndex;
    }

    public Vector2 ReadMoveInput()
    {
        return playerInput.actions["MoveInput"].ReadValue<Vector2>();
    }

    public string GetInfo()
    {
        return $"Player {playerId} - {deviceName} : {deviceId}";
    }
}