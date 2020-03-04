using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCamera : MonoBehaviour
{
    private Vector2 rotation;
    private float cameraZoomOffsetX;
    private float cameraZoomOffsetZ;

    [Header("Player Transform")]
    [SerializeField] private Transform player = null;
    [Header("General Settings")]
    [SerializeField] private bool useRightClickToRotateCamera = false;
    [SerializeField] private bool cameraControlsPlayerRotation = false;
    [SerializeField] private bool useRightClickToRotatePlayer = false;
    [Header("Camera rotation")]
    [SerializeField] private float cameraRotationSpeed = 4.0f;
    [SerializeField] private float verticalRotationLock = 60.0f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(6, 0, 6);
    [Header("Camera zoom")]
    [SerializeField] private float zoomSpeed = 5.0f;
    [SerializeField] private float zoomMinDistance = 2.0f;
    [SerializeField] private float zoomMaxDistance = 8.0f;
    [Header("Camera Movement")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float cameraMoveLerpSpeed = 0.1f;

    private void Start()
    {
        cameraZoomOffsetX = cameraOffset.x;
        cameraZoomOffsetZ = cameraOffset.z;
    }

    private void LateUpdate()
    {
        if (!useRightClickToRotateCamera || (useRightClickToRotateCamera && Input.GetMouseButton(1)))
        {
            SetRotation();
        }

        SetCameraZoom();

        Vector3 newCameraLocation = GetCameraLocation();

        transform.position = Vector3.Lerp(transform.position, newCameraLocation, cameraMoveLerpSpeed);
        transform.LookAt(player);
    }

    private void SetRotation()
    {
        rotation.y += Input.GetAxis("Mouse X") * cameraRotationSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * cameraRotationSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -verticalRotationLock, verticalRotationLock);
    }

    private void SetCameraZoom()
    {
        cameraZoomOffsetX += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        cameraZoomOffsetZ += -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        cameraZoomOffsetX = Mathf.Clamp(cameraZoomOffsetX, zoomMinDistance, zoomMaxDistance);
        cameraZoomOffsetZ = Mathf.Clamp(cameraZoomOffsetZ, zoomMinDistance, zoomMaxDistance);

        cameraOffset = new Vector3(cameraZoomOffsetX, cameraOffset.y, cameraZoomOffsetZ);
    }

    private Vector3 GetCameraLocation()
    {
        if(cameraControlsPlayerRotation || (useRightClickToRotatePlayer && Input.GetMouseButton(1)))
        {
            player.rotation = Quaternion.Euler(0, rotation.y, 0);
        }
        Vector3 newLocation = player.position - (Quaternion.Euler(rotation.x, rotation.y, 0) * cameraOffset);

        newLocation = CheckForCameraObstruction(newLocation);

        return newLocation;
    }

    private Vector3 CheckForCameraObstruction(Vector3 cameraLocation)
    {
        RaycastHit hit;

        if (Physics.Linecast(player.position, cameraLocation, out hit))
        {
            Debug.Log(hit.collider.transform.name);
            cameraLocation = hit.point;
        }

        return cameraLocation;
    }

    public bool UseCameraRotation()
    {
        return !useRightClickToRotatePlayer;
    }
}