using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class Player : MonoBehaviour
{
    CharacterController characterController;
    Camera mainCamera;
    ThirdPersonCamera thirdPersonCamera;

    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float jumpSpeed = 8.0f;
    [SerializeField] private float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        mainCamera = Camera.main;
        thirdPersonCamera = mainCamera.GetComponent<ThirdPersonCamera>();
    }

    void Update()
    {
        if (characterController.isGrounded)
        {
            SetMoveDirection();
        }

        moveDirection.y -= gravity * Time.deltaTime;
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void SetMoveDirection()
    {
        if (thirdPersonCamera && thirdPersonCamera.UseCameraRotation())
        {
            SetMoveDirectionUsingCameraRotation();
        }
        else
        {
            SetMoveDirectionUsingPlayerRotation();
        }

        if (Input.GetButton("Jump"))
        {
            moveDirection.y = jumpSpeed;
        }
    }

    private void SetMoveDirectionUsingCameraRotation()
    {
        float desiredAngle = mainCamera.transform.eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(0, desiredAngle, 0);

        Vector3 forward = (rotation * Vector3.forward) * Input.GetAxis("Vertical");
        Vector3 right = (rotation * Vector3.right) * Input.GetAxis("Horizontal");

        moveDirection = forward + right;
        moveDirection *= speed;
    }

    private void SetMoveDirectionUsingPlayerRotation()
    {
        Quaternion rotation = transform.rotation;
        Vector3 forward = (rotation * Vector3.right) * Input.GetAxis("Vertical");
        Vector3 right = (rotation * Vector3.back) * Input.GetAxis("Horizontal");

        moveDirection = forward + right;
        moveDirection *= speed;
    }
}