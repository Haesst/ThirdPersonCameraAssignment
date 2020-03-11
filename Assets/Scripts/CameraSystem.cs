using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSystem : MonoBehaviour
{
    private struct CameraInput
    {
        public string mouseXName;
        public string mouseYName;
        public string mouseScrollName;
        public string inputHorizontal;
        public string inputVertical;

        public float mouseScrollWheel;
        public Vector2 mouseInput;
        public Vector2 movementInput;
        public bool rightMouseButtonDown;

        public void Init(string mouseXName, string mouseYName, string mouseScrollName, string inputHorizontal, string inputVertical)
        {
            this.mouseXName = mouseXName;
            this.mouseYName = mouseYName;
            this.mouseScrollName = mouseScrollName;
            this.inputHorizontal = inputHorizontal;
            this.inputVertical = inputVertical;
        }

        public void ReadInput()
        {
            mouseInput.Set(Input.GetAxis(mouseXName), Input.GetAxis(mouseYName));
            mouseScrollWheel = Input.GetAxis(mouseScrollName);
            rightMouseButtonDown = Input.GetMouseButton(1);

            movementInput.Set(Input.GetAxis(inputHorizontal), Input.GetAxis(inputVertical));
        }
    }

    private readonly Dictionary<CameraType, Vector3> defaultCameraOffsets = null;
    private readonly Dictionary<ThirdPersonCameraType, Vector3> defaultTPCOffsets = null;

    private CameraInput cameraInput;

    private Vector2 rotation;
    private float cameraZoomOffsetX;
    private float cameraZoomOffsetZ;

    private Vector3 desiredLocation;

    [Header("Player Transform")]
    [SerializeField] private Transform player = null;
    public CameraType cameraType;
    public ThirdPersonCameraType thirdPersonCameraType;
    public bool useRightClickToRotateCamera = false;
    [Header("Camera rotation")]
    [SerializeField] private float cameraRotationSpeed = 4.0f;
    [SerializeField] private float verticalRotationMax = 120.0f;
    [SerializeField] private float verticalRotationMin = -30.0f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(6, 0, 6);
    [Header("Camera zoom")]
    [SerializeField] private float zoomSpeed = 5.0f;
    [SerializeField] private float zoomMinDistance = 2.0f;
    [SerializeField] private float zoomMaxDistance = 8.0f;
    [Header("Camera Movement")]
    [Range(0.01f, 0.9f)]
    [SerializeField] private float cameraMoveLerpSpeed = 0.1f;

    CameraSystem()
    {
        defaultCameraOffsets = new Dictionary<CameraType, Vector3>
        {
            { CameraType.FirstPersonCamera, new Vector3(0, 1, 0) },
            { CameraType.FreeModeCamera, new Vector3(0, 1, 0) }
        };

        defaultTPCOffsets = new Dictionary<ThirdPersonCameraType, Vector3>
        {
            { ThirdPersonCameraType.Fixed, new Vector3(-12, 15, -20) },
            { ThirdPersonCameraType.Interactive, new Vector3(0, 3, -7) },
            { ThirdPersonCameraType.Tracking, new Vector3(0, 3, -7) }
        };

        desiredLocation = Vector3.zero;
    }

    private void Start()
    {
        cameraInput = new CameraInput();
        cameraInput.Init("Mouse X", "Mouse Y", "Mouse ScrollWheel", "Horizontal", "Vertical");
        cameraZoomOffsetX = cameraOffset.x;
        cameraZoomOffsetZ = cameraOffset.z;

        rotation.y = player.eulerAngles.y;
    }

    private void Update()
    {
        cameraInput.ReadInput();
    }

    private void FixedUpdate()
    {
        SetCameraLocation();
    }

    private void SetCameraLocation()
    {
        // Calculate new rotation
        if (ShouldSetNewRotation())
        {
            SetRotation();
        }
        // Set zoom
        if(ShouldSetZoom())
        {
            SetCameraZoom();
        }
        // Calculate new location
        SetDesiredLocation();
        // Set new Location with a lerp
        transform.position = Vector3.Lerp(transform.position, desiredLocation, cameraMoveLerpSpeed);
        // look at player
        if(ShouldLookAtPlayer())
        {
            transform.LookAt(player);
        }
    }

    private bool ShouldSetNewRotation()
    {
        return cameraType == CameraType.FirstPersonCamera || (cameraType == CameraType.ThirdPersonCamera && (thirdPersonCameraType == ThirdPersonCameraType.Interactive || thirdPersonCameraType == ThirdPersonCameraType.Tracking));
    }

    private bool ShouldSetZoom()
    {
        return cameraType == CameraType.ThirdPersonCamera && thirdPersonCameraType == ThirdPersonCameraType.Interactive;
    }

    private bool ShouldLookAtPlayer()
    {
        return cameraType == CameraType.ThirdPersonCamera && (thirdPersonCameraType == ThirdPersonCameraType.Interactive || thirdPersonCameraType == ThirdPersonCameraType.Tracking);
    }

    private void SetRotation()
    {
        if (cameraType == CameraType.FirstPersonCamera)
        {
            transform.rotation = player.rotation;
        }
        else
        {
            rotation.y += cameraInput.mouseInput.x * cameraRotationSpeed;
            rotation.x += -cameraInput.mouseInput.y * cameraRotationSpeed;
            rotation.x = Mathf.Clamp(rotation.x, verticalRotationMin, verticalRotationMax);
        }
    }

    private void SetCameraZoom()
    {
        cameraZoomOffsetX += -cameraInput.mouseScrollWheel * zoomSpeed;
        cameraZoomOffsetZ += -cameraInput.mouseScrollWheel * zoomSpeed;

        cameraZoomOffsetX = Mathf.Clamp(cameraZoomOffsetX, zoomMinDistance, zoomMaxDistance);
        cameraZoomOffsetZ = Mathf.Clamp(cameraZoomOffsetZ, zoomMinDistance, zoomMaxDistance);

        cameraOffset = new Vector3(cameraZoomOffsetX, cameraOffset.y, cameraZoomOffsetZ);
    }

    private void SetDesiredLocation()
    {
        if (cameraType == CameraType.ThirdPersonCamera)
        {
            if (thirdPersonCameraType == ThirdPersonCameraType.Interactive)
            {
                Vector3 newLocation = player.position - (Quaternion.Euler(rotation.x, rotation.y, 0) * cameraOffset);
                newLocation = CheckForCameraObstruction(newLocation);

                desiredLocation = newLocation;
            }
            else if(thirdPersonCameraType == ThirdPersonCameraType.Fixed)
            {
                Vector3 playerLocationWithOffset = player.transform.position + (player.transform.forward * cameraOffset.z);
                playerLocationWithOffset.y += cameraOffset.y;

                desiredLocation = playerLocationWithOffset;
            }
            else
            {
                Vector3 playerLocationWithOffset = player.transform.position + (player.transform.forward * cameraOffset.z);
                playerLocationWithOffset.y += cameraOffset.y;

                playerLocationWithOffset = CheckForCameraObstruction(playerLocationWithOffset);
                desiredLocation = playerLocationWithOffset;
            }
        }
        else if(cameraType == CameraType.FirstPersonCamera)
        {
            Vector3 playerLocationWithOffset = player.transform.position + (player.transform.forward * cameraOffset.z);
            playerLocationWithOffset.y += cameraOffset.y;

            desiredLocation = playerLocationWithOffset;
        }
    }

    private Vector3 CheckForCameraObstruction(Vector3 cameraLocation)
    {

        if (Physics.Linecast(player.position, cameraLocation, out RaycastHit hit))
        {
            if(hit.collider.transform.root.transform != player)
            {
                cameraLocation = hit.point;
            }
        }

        return cameraLocation;
    }

    public Vector3 GetDefaultOffset(CameraType cameraType)
    {
        return defaultCameraOffsets[cameraType];
    }

    public Vector3 GetDefaultOffset(ThirdPersonCameraType cameraType)
    {
        return defaultTPCOffsets[cameraType];
    }

    public void ChangeCameraOffset(Vector3 newCameraOffset)
    {
        cameraOffset = newCameraOffset;
    }

    public void UpdateCameraPosition()
    {
        Vector3 playerLocationWithOffset = player.transform.position + (player.transform.forward * cameraOffset.z);
        playerLocationWithOffset.y += cameraOffset.y;

        transform.position = playerLocationWithOffset;
        if (cameraType == CameraType.ThirdPersonCamera)
        {
            transform.LookAt(player);
        }
        else if(cameraType == CameraType.FirstPersonCamera)
        {
            transform.rotation = player.rotation;
        }
    }
}

[System.Serializable]
public enum CameraType
{
    ThirdPersonCamera,
    FirstPersonCamera,
    FreeModeCamera,
}

[System.Serializable]
public enum ThirdPersonCameraType
{
    Fixed,
    Tracking,
    Interactive
}