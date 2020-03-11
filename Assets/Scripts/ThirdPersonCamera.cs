using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField]private Transform target = null;

    [SerializeField]private float targetHeight = 1.7f;
    [SerializeField]private float distance = 5.0f;
    [SerializeField]private float offsetFromWall = 0.1f;

    [SerializeField]private float maxDistance = 20.0f;
    [SerializeField]private float minDistance = 0.6f;
    [SerializeField]private float speedDistance = 5.0f;

    [SerializeField]private float xSpeed = 200.0f;
    [SerializeField]private float ySpeed = 200.0f;

    [SerializeField]private int yMinLimit = -40;
    [SerializeField]private int yMaxLimit = 80;

    [SerializeField]private int zoomRate = 50;

    [SerializeField]private float rotationDampening = 3.0f;
    [SerializeField]private float zoomDampening = 5.0f;

    [SerializeField]private LayerMask collisionLayers = -1;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private float currentDistance;
    private float desiredDistance;
    private float correctedDistance;

    private Vector2 movementInput;
    private Vector2 mouseInput;

    private float scrollWheelInput;

    private bool rightMouseButtonDown;
    private bool leftMouseButtonDown;

    private readonly string horizontalMovementName = "Horizontal";
    private readonly string verticalMovementName = "Vertical";

    private readonly string mouseXName = "Mouse X";
    private readonly string mouseYName = "Mouse Y";

    private readonly string scrollWheelAxisName = "Mouse ScrollWheel";

    private Vector3 targetOffset;
    private Quaternion newCameraRotation;
    private Vector3 newCameraPosition;
    private Vector3 trueTargetPosition;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        xDeg = angles.x;
        yDeg = angles.y;

        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;
    }

    private void Update()
    {
        UpdateInput();
    }

    private void LateUpdate()
    {

        if(!target)
        {
            return;
        }

        if (leftMouseButtonDown || rightMouseButtonDown)
        {
            SetDegreesBasedOnMouseMovement();
        }
        else if (movementInput.x != 0 || movementInput.y != 0)
        {
            RotateBackToBehindPlayer();
        }

        CalculateDesiredDistance();
        CalculateNewCameraRotation();
        CalculateDesiredPosition();

        bool isCorrected = SetCorrectedDistanceOnCameraObstruction();

        CalculateCurrentDistance(isCorrected);
        CalculateDesiredPositionBasedOnCurrentDistance();

        transform.rotation = newCameraRotation;
        transform.position = newCameraPosition;
    }

    private void UpdateInput()
    {
        movementInput.Set(Input.GetAxis(horizontalMovementName), Input.GetAxis(verticalMovementName));
        mouseInput.Set(Input.GetAxis(mouseXName), Input.GetAxis(mouseYName));
        scrollWheelInput = Input.GetAxis(scrollWheelAxisName);
        leftMouseButtonDown = Input.GetMouseButton(0);
        rightMouseButtonDown = Input.GetMouseButton(1);
    }

    private void SetDegreesBasedOnMouseMovement()
    {
        xDeg += mouseInput.x * xSpeed * 0.02f;
        yDeg -= mouseInput.y * ySpeed * 0.02f;
    }

    private void RotateBackToBehindPlayer()
    {
        float targetRotationAngle = target.eulerAngles.y;
        float currentRotationAngle = transform.eulerAngles.y;

        xDeg = Mathf.LerpAngle(currentRotationAngle, targetRotationAngle, rotationDampening * Time.deltaTime);
    }

    private void CalculateDesiredDistance()
    {
        desiredDistance -= scrollWheelInput * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance) * speedDistance;
        desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
        correctedDistance = desiredDistance;
    }

    private void CalculateNewCameraRotation()
    {
        yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
        newCameraRotation = Quaternion.Euler(yDeg, xDeg, 0);
    }
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
        {
            angle += 360;
        }
        if (angle > 360)
        {
            angle -= 360;
        }
        return Mathf.Clamp(angle, min, max);
    }

    private void CalculateDesiredPosition()
    {
        targetOffset = new Vector3(0, -targetHeight, 0);
        newCameraPosition = target.position - (newCameraRotation * Vector3.forward * desiredDistance + targetOffset);

        trueTargetPosition = target.position - targetOffset;
    }

    private bool SetCorrectedDistanceOnCameraObstruction()
    {
        if (Physics.Linecast(trueTargetPosition, newCameraPosition, out RaycastHit collisionHit, collisionLayers.value))
        {
            correctedDistance = Vector3.Distance(trueTargetPosition, collisionHit.point) - offsetFromWall;
            return true;
        }

        return false;
    }

    private void CalculateCurrentDistance(bool isCorrected)
    {
        // For smoothing purposes lerp distance only if either distance wasn't 
        // corrected, or corrected distance is more than current distance
        currentDistance = !isCorrected || correctedDistance > currentDistance 
            ? Mathf.Lerp(currentDistance, correctedDistance, Time.deltaTime * zoomDampening) 
            : correctedDistance;

        // keep within limits
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
    private void CalculateDesiredPositionBasedOnCurrentDistance()
    {
        newCameraPosition = target.position - (newCameraRotation * Vector3.forward * currentDistance + targetOffset);
    }
}