using UnityEngine;

public class ARBoxRotationController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 0.5f;
    public bool enableXAxisRotation = true;
    public bool enableYAxisRotation = true;
    public bool enableZAxisRotation = false;
    
    [Header("Rotation Limits (Optional)")]
    public bool limitRotation = false;
    public float maxXRotation = 90f;
    public float maxYRotation = 180f;
    public float maxZRotation = 90f;
    
    [Header("Visual Feedback")]
    public bool showRotationGizmo = true;
    public Color highlightColor = Color.red;
    public float outlineWidth = 0.02f;
    
    [Header("Smooth Rotation")]
    public bool enableSmoothRotation = true;
    public float smoothingSpeed = 5f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;

    // Private variables
    private Camera arCamera;
    private bool isSelected = false;
    private bool isRotating = false;
    private Vector2 lastTouchPosition;
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Renderer boxRenderer;
    private LaserRedirector laserRedirector;
    
        // Remove unused variables and methods
    
    // Touch handling
    private int touchId = -1;

    void Start()
    {
        // Get components
        arCamera = Camera.main;
        if (arCamera == null)
            arCamera = FindObjectOfType<Camera>();
            
        boxRenderer = GetComponent<Renderer>();
        laserRedirector = GetComponent<LaserRedirector>();
        
        // Create outline box for highlighting
        CreateOutlineBox();
            
        // LaserRedirector reference for debugging if needed
        if (laserRedirector != null)
        {
            if (showDebugInfo)
                Debug.Log($"LaserRedirector found on {gameObject.name}");
        }
        
        // Remove the CreateOutlineBox method and CreateCubeMesh method
        // Clean up the code
        
        // Initialize rotation tracking
        currentRotation = transform.eulerAngles;
        targetRotation = currentRotation;
        
        // Add collider if missing (needed for touch detection)
        if (GetComponent<Collider>() == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            if (showDebugInfo)
                Debug.Log($"Added BoxCollider to {gameObject.name} for touch detection");
        }
    }

    void Update()
    {
        HandleTouchInput();
        
        if (enableSmoothRotation && isRotating)
        {
            ApplySmoothRotation();
        }
    }

    void HandleTouchInput()
    {
        // Handle touch input for mobile devices
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                HandleTouch(touch);
            }
        }
        // Handle mouse input for testing in editor
        else if (Input.GetMouseButtonDown(0))
        {
            HandleMouseDown();
        }
        else if (Input.GetMouseButton(0) && isSelected)
        {
            HandleMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            HandleMouseUp();
        }
    }

    void HandleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                HandleTouchStart(touch);
                break;
                
            case TouchPhase.Moved:
                if (touchId == touch.fingerId)
                    HandleTouchMove(touch);
                break;
                
            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                if (touchId == touch.fingerId)
                    HandleTouchEnd(touch);
                break;
        }
    }

    void HandleTouchStart(Touch touch)
    {
        if (IsObjectTouched(touch.position))
        {
            SelectObject();
            lastTouchPosition = touch.position;
            touchId = touch.fingerId;
        }
    }

    void HandleTouchMove(Touch touch)
    {
        if (!isSelected) return;
        
        Vector2 deltaMovement = touch.position - lastTouchPosition;
        CalculateRotation(deltaMovement);
        lastTouchPosition = touch.position;
    }

    void HandleTouchEnd(Touch touch)
    {
        if (touchId == touch.fingerId)
        {
            DeselectObject();
            touchId = -1;
        }
    }

    // Mouse handling for editor testing
    void HandleMouseDown()
    {
        if (IsObjectTouched(Input.mousePosition))
        {
            SelectObject();
            lastTouchPosition = Input.mousePosition;
        }
    }

    void HandleMouseDrag()
    {
        Vector2 currentMousePos = Input.mousePosition;
        Vector2 deltaMovement = currentMousePos - lastTouchPosition;
        CalculateRotation(deltaMovement);
        lastTouchPosition = currentMousePos;
    }

    void HandleMouseUp()
    {
        DeselectObject();
    }

    bool IsObjectTouched(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (showDebugInfo)
                    Debug.Log($"Touch detected on {gameObject.name}");
                return true;
            }
        }
        return false;
    }

    void CreateOutlineBox()
    {
        // No need to create extra objects - we'll use Gizmos for highlighting
        // This keeps the hierarchy clean and avoids material issues
    }

    Mesh CreateCubeMesh()
    {
        // Create a simple cube mesh if none exists
        GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mesh cubeMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(tempCube);
        return cubeMesh;
    }

    void SelectObject()
    {
        isSelected = true;
        isRotating = true;
        
        if (showDebugInfo)
            Debug.Log($"Selected {gameObject.name} for rotation");
    }

    void DeselectObject()
    {
        isSelected = false;
        isRotating = false;
        
        if (showDebugInfo)
            Debug.Log($"Deselected {gameObject.name}");
    }

    void CalculateRotation(Vector2 deltaMovement)
    {
        if (!isSelected) return;
        
        // Calculate rotation based on drag direction
        float xRotationDelta = 0f;
        float yRotationDelta = 0f;
        float zRotationDelta = 0f;
        
        if (enableYAxisRotation)
        {
            // Horizontal drag rotates around Y-axis
            yRotationDelta = deltaMovement.x * rotationSpeed;
        }
        
        if (enableXAxisRotation)
        {
            // Vertical drag rotates around X-axis (inverted for natural feel)
            xRotationDelta = -deltaMovement.y * rotationSpeed;
        }
        
        if (enableZAxisRotation)
        {
            // Could add Z-axis rotation with modifier key or gesture
            zRotationDelta = 0f;
        }
        
        // Apply rotation
        Vector3 rotationDelta = new Vector3(xRotationDelta, yRotationDelta, zRotationDelta);
        
        if (enableSmoothRotation)
        {
            targetRotation += rotationDelta;
            
            // Apply rotation limits if enabled
            if (limitRotation)
            {
                targetRotation = ApplyRotationLimits(targetRotation);
            }
        }
        else
        {
            // Direct rotation
            transform.Rotate(rotationDelta, Space.World);
            currentRotation = transform.eulerAngles;
            
            // Apply rotation limits if enabled
            if (limitRotation)
            {
                currentRotation = ApplyRotationLimits(currentRotation);
                transform.eulerAngles = currentRotation;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Rotation Delta: {rotationDelta}, Current: {currentRotation}");
        }
    }

    void ApplySmoothRotation()
    {
        if (!isRotating) return;
        
        // Smoothly interpolate to target rotation
        Quaternion targetQuaternion = Quaternion.Euler(targetRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, Time.deltaTime * smoothingSpeed);
        
        currentRotation = transform.eulerAngles;
    }

    Vector3 ApplyRotationLimits(Vector3 rotation)
    {
        // Normalize angles to -180 to 180 range for proper limiting
        rotation.x = NormalizeAngle(rotation.x);
        rotation.y = NormalizeAngle(rotation.y);
        rotation.z = NormalizeAngle(rotation.z);
        
        // Apply limits
        rotation.x = Mathf.Clamp(rotation.x, -maxXRotation, maxXRotation);
        rotation.y = Mathf.Clamp(rotation.y, -maxYRotation, maxYRotation);
        rotation.z = Mathf.Clamp(rotation.z, -maxZRotation, maxZRotation);
        
        return rotation;
    }

    float NormalizeAngle(float angle)
    {
        while (angle > 180f)
            angle -= 360f;
        while (angle < -180f)
            angle += 360f;
        return angle;
    }

    void OnDrawGizmos()
    {
        if (isSelected)
        {
            // Draw red outline when selected
            Gizmos.color = highlightColor;
            Gizmos.DrawWireCube(transform.position, transform.lossyScale * (1f + outlineWidth));
        }
        
        if (showRotationGizmo && isSelected)
        {
            // Draw rotation axes
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up * 0.5f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 0.5f);
        }
    }

    // Public methods for external control
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0f, speed);
    }

    public void EnableAxis(bool x, bool y, bool z)
    {
        enableXAxisRotation = x;
        enableYAxisRotation = y;
        enableZAxisRotation = z;
    }

    public void SetRotationLimits(bool enable, float maxX = 90f, float maxY = 180f, float maxZ = 90f)
    {
        limitRotation = enable;
        maxXRotation = maxX;
        maxYRotation = maxY;
        maxZRotation = maxZ;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.identity;
        currentRotation = Vector3.zero;
        targetRotation = Vector3.zero;
    }

    public bool IsCurrentlyRotating()
    {
        return isRotating;
    }
}