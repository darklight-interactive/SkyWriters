using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiplayerManager : MonoBehaviour
{
public GameObject playerPrefab; // Assign your player prefab here
    private List<PlaneController> players = new List<PlaneController>();

    private void Start()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
        InitializePlayers();
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void InitializePlayers()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (device is Keyboard || device is Gamepad)
            {
                CreatePlayer(device);
                Debug.Log("Player Device: " + device.displayName);

            }
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            if (device is Keyboard || device is Gamepad)
            {
                CreatePlayer(device);
            }
        }
        else if (change == InputDeviceChange.Removed)
        {
            RemovePlayer(device);
        }
    }

    private void CreatePlayer(InputDevice device)
    {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        PlaneController playerController = player.GetComponent<PlaneController>();

        if (playerController != null)
        {
            playerController.AssignDevice(device);
            players.Add(playerController);
        }
    }

    private void RemovePlayer(InputDevice device)
    {
        PlaneController player = players.Find(p => p.InputDevice == device);
        if (player != null)
        {
            players.Remove(player);
            Destroy(player.gameObject);
        }
    }
}
