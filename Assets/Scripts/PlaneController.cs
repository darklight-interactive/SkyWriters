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

    [SerializeField, Range(0, 1000)] float _moveSpeed = 10;
    [SerializeField, Range(0, 100)] float _rotationSpeed = 10;

    private float _rotationOffset;

    // Start is called before the first frame update
    void Start()
    {
        // Freeze the Y position of the plane
        rb.constraints = RigidbodyConstraints.FreezePositionY;


        // Subscribe to the move input events
        //UniversalInputManager.OnMoveInput += SetMovement;
        //UniversalInputManager.OnMoveInputCanceled += ResetMovement;
        playerInput.actions["MoveInput"].performed += ctx => SetMovement(ctx.ReadValue<Vector2>());
        playerInput.actions["MoveInput"].canceled += ctx => ResetMovement();
    
    }

    void SetMovement(Vector2 moveInput)
    {
        // Store the move direction on the XZ plane
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
        rb.velocity = direction * _moveSpeed;

        // Set the target rotation value based on the direction of the x input
        _rotationOffset = direction.x * -90;
        //Debug.Log($"Target Rotation: {_rotationOffset}");
    }

    void ResetMovement()
    {
        _rotationOffset = 0;
    }

    void FixedUpdate()
    {
        // Set the velocity of the plane to move in the current forward direction
        rb.velocity = transform.forward * _moveSpeed;


        // Slerp the current rotation to the target rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y + _rotationOffset, currentRotation.eulerAngles.z);
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);

        //cameraWraparound.WrapAround(transform);

    }
}
