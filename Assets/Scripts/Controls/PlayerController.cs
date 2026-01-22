using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;
    [Header("Sprint")]
    [SerializeField]
    private float sprintMultiplier = 1.5f;
    [SerializeField]
    private bool sprintOnlyOnGround = true;

    [Header("Input Actions (Project-wide)")]
    [SerializeField]
    private string moveActionName = "Move";
    [SerializeField]
    private string lookActionName = "Look";
    [SerializeField]
    private string jumpActionName = "Jump";
    [SerializeField]
    private string sprintActionName = "Sprint";
    [SerializeField]
    private string restartActionName = "Restart";

    [Header("Input")]
    [SerializeField]
    private float moveInputSharpness = 20f;

    [SerializeField]
    private float jumpForce = 7f;
    [SerializeField]
    private float cameraSensitivity = 2f;
    [SerializeField]
    private float moveDecelerationRate = 10f;

    private bool isGrounded = true;
    private float xRotation = 0f;
    private Vector3 desiredMoveDirection = Vector3.zero;
    private bool jumpQueued;
    private bool sprintHeld;
    private Vector2 moveInput;
    private float pendingYaw = 0f;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction restartAction;

    private Camera playerCamera;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main;

        // Smooth visual motion between physics ticks.
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Helps reduce jitter/tunneling when moving at speed.
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center of screen
    }

    void OnEnable()
    {
        BindActions();
    }

    void Update()
    {
        Look();
        ApplyLookRotation();
        ReadMovementInput();
        ReadSprintInput();
        QueueJump();

        #if UNITY_EDITOR // strictly debug 'r' restart
        if (restartAction != null && restartAction.WasPressedThisFrame())
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        #endif
    }

    private void ReadSprintInput()
    {
        sprintHeld = sprintAction != null && sprintAction.IsPressed();
    }

    void FixedUpdate() // Unity physics updates here at a fixed interval
    {
        SetGrounded();
        ApplyMovement();
        jumpQueued = false;
    }

    private void ReadMovementInput()
    {
        Vector2 targetMove = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        targetMove = Vector2.ClampMagnitude(targetMove, 1f);
        // Match the old Input Manager's "GetAxis" feel (smoothed).
        moveInput = Vector2.MoveTowards(moveInput, targetMove, moveInputSharpness * Time.deltaTime);

        float moveX = moveInput.x;
        float moveZ = moveInput.y;

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Prevent faster diagonal movement.
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        desiredMoveDirection = moveDirection;
    }

    private void QueueJump()
    {
        if (jumpAction != null && jumpAction.WasPressedThisFrame())
        {
            jumpQueued = true;
        }
    }

    private void ApplyMovement()
    {
        Vector3 currentVelocity = rb.velocity;
        Vector3 currentHorizontalVelocity = new(currentVelocity.x, 0f, currentVelocity.z);

        float currentMoveSpeed = moveSpeed;
        if (sprintHeld && (!sprintOnlyOnGround || isGrounded))
        {
            currentMoveSpeed *= sprintMultiplier;
        }

        Vector3 targetHorizontalVelocity = desiredMoveDirection * currentMoveSpeed;

        // When there's no input, decelerate smoothly toward zero; otherwise go to target speed.
        Vector3 newHorizontalVelocity = targetHorizontalVelocity;
        if (desiredMoveDirection.sqrMagnitude < 0.0001f)
        {
            newHorizontalVelocity = Vector3.MoveTowards(
                currentHorizontalVelocity,
                Vector3.zero,
                moveDecelerationRate * Time.fixedDeltaTime
            );
        }

        float newY = currentVelocity.y;
        if (jumpQueued && isGrounded)
        {
            // Replace vertical velocity with a jump impulse.
            newY = jumpForce;
        }

        rb.velocity = new(newHorizontalVelocity.x, newY, newHorizontalVelocity.z);
    }

    private void Look()
    {
        Vector2 look = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        float lookX = look.x * cameraSensitivity;
        float lookY = look.y * cameraSensitivity;

        // Rotate the camera up and down (inverting the Y axis)
        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent over-rotation

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate camera
        // Apply yaw in FixedUpdate using Rigidbody so visuals match physics.
        pendingYaw += lookX;
    }

    private void ApplyLookRotation()
    {
        if (Mathf.Abs(pendingYaw) < 0.0001f)
        {
            return;
        }

        // Camera is a child of the Rigidbody-driven player, so rotate via Rigidbody.
        // With interpolation enabled, Unity will smooth this between physics ticks.
        Quaternion delta = Quaternion.Euler(0f, pendingYaw, 0f);
        rb.MoveRotation(rb.rotation * delta);
        pendingYaw = 0f;
    }

    private void BindActions()
    {
        moveAction = FindProjectAction(moveActionName);
        lookAction = FindProjectAction(lookActionName);
        jumpAction = FindProjectAction(jumpActionName);
        sprintAction = FindProjectAction(sprintActionName);
        restartAction = FindProjectAction(restartActionName);
    }

    private static InputAction FindProjectAction(string actionName)
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return null;
        }

        if (InputSystem.actions == null)
        {
            return null;
        }

        return InputSystem.actions.FindAction(actionName, throwIfNotFound: false);
    }
    
    private void SetGrounded()
    {
        isGrounded = Physics.SphereCast(transform.position, 0.1f, Vector3.down, out RaycastHit _, 2f);
    }
}