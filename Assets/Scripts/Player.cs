using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    CharacterController characterController;
    Camera mainCamera;

    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float jumpSpeed = 8.0f;
    [SerializeField] private float fallMultiplier = 2.6f;

    private Vector3 moveDirection = Vector3.zero;


    private bool jumpRequest;
    private bool holdingRightMouseButton;
    private Vector2 movementInput;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        movementInput.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetButton("Jump") && !jumpRequest && characterController.isGrounded)
        {
            jumpRequest = true;
        }

        holdingRightMouseButton = Input.GetMouseButton(1);
    }

    void FixedUpdate()
    {
        if(holdingRightMouseButton)
        {
            SetPlayerRotation();
        }

        if (characterController.isGrounded)
        {
            SetMoveDirection();
        }

        moveDirection.y += Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        characterController.Move(moveDirection * Time.fixedDeltaTime);
    }

    private void SetMoveDirection()
    {
        SetMoveDirectionUsingPlayerRotation();
        if (jumpRequest)
        {
            moveDirection.y = jumpSpeed;
            jumpRequest = false;
        }
    }

    private void SetMoveDirectionUsingPlayerRotation()
    {
        Quaternion rotation = transform.rotation;
        Vector3 forward = (rotation * Vector3.forward) * movementInput.y;
        Vector3 right = (rotation * Vector3.right) * movementInput.x;

        moveDirection = forward + right;
        moveDirection *= speed;
    }

    private void SetPlayerRotation()
    {
        transform.rotation = Quaternion.Euler(0, mainCamera.transform.rotation.eulerAngles.y, 0);
    }
}