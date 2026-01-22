using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerControllerSource : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float jumpForce = 3f;
    [SerializeField]
    private float cameraSensitivity = 2f;

    [Header("Source-like Movement")]
    [SerializeField]
    private float groundAcceleration = 60f;
    [SerializeField]
    private float airAcceleration = 15f;
    [SerializeField]
    private float friction = 6f;
    [SerializeField]
    private float stopSpeed = 2f;
    [SerializeField]
    private float bhopFrictionGraceTime = 0.075f;

    [Header("Speed Limits")]
    [SerializeField]
    private float maxHorizontalSpeed = 8f;
    [SerializeField]
    private float airSpeedCap = 5f;

    [Header("Jumping")]
    [SerializeField]
    private float jumpBufferTime = 0.1f;
    [SerializeField]
    private float coyoteTime = 0.1f;
    [SerializeField]
    private bool autoBhop;

    [Header("Ground Check")]
    [SerializeField]
    private float groundCheckRadius = 0.1f;
    [SerializeField]
    private float groundCheckDistance = 0.2f;

    // Private state variables
    private bool isGrounded = true;
    private bool wasGrounded;
    private float xRotation = 0f;
    private Vector3 desiredMoveDirection = Vector3.zero;
    private bool jumpQueued;
    private float jumpQueuedAt;
    private float lastGroundedAt;
    private float lastLandedAt;
    private bool jumpHeld;
    private bool jumpedSinceGrounded;

    // Cached components
    private new Camera camera;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        camera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center of screen
    }

    void Update()
    {
        Look();
        ReadMovementInput();
        QueueJump();

        Debug.Log($"Speed: {rb.velocity.magnitude:F2}");

        #if UNITY_EDITOR // strictly debug 'r' restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
        #endif
    }

    void FixedUpdate() // Unity physics updates here at a fixed interval
    {
        wasGrounded = isGrounded;
        SetGrounded();

        if (isGrounded)
        {
            jumpedSinceGrounded = false;
        }

        if (!wasGrounded && isGrounded)
        {
            lastLandedAt = Time.time;
        }

        if (isGrounded)
        {
            lastGroundedAt = Time.time;
        }

        ApplyMovement();
    }

    private void ReadMovementInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

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
        jumpHeld = Input.GetButton("Jump");
        bool wantJump = autoBhop ? jumpHeld : Input.GetButtonDown("Jump");

        if (wantJump)
        {
            jumpQueued = true;
            jumpQueuedAt = Time.time;
        }
    }

    private void ApplyMovement()
    {
        float dt = Time.fixedDeltaTime;

        Vector3 velocity = rb.velocity;
        Vector3 horizontalVelocity = new(velocity.x, 0f, velocity.z);

        if (isGrounded && ShouldApplyFriction())
        {
            ApplyFriction(ref horizontalVelocity, dt);
        }

        float wishSpeed = moveSpeed;
        float accel = isGrounded ? groundAcceleration : airAcceleration;
        if (!isGrounded)
        {
            wishSpeed = Mathf.Min(wishSpeed, airSpeedCap);
        }

        if (desiredMoveDirection.sqrMagnitude > 0.0001f)
        {
            Accelerate(ref horizontalVelocity, desiredMoveDirection, wishSpeed, accel, dt);
        }

        // Keep ground movement tight: don't allow turning input to accumulate extra sideways speed.
        // Skip clamping when a hop is queued so bhop speed retention still works.
        if (isGrounded && !jumpQueued)
        {
            float horizontalSpeed = horizontalVelocity.magnitude;
            if (horizontalSpeed > moveSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * moveSpeed;
            }
        }

        // Hard cap: never allow horizontal speed beyond this value (ground/air/bhop).
        float cappedSpeed = Mathf.Max(0f, maxHorizontalSpeed);
        if (cappedSpeed > 0f)
        {
            float horizontalSpeed = horizontalVelocity.magnitude;
            if (horizontalSpeed > cappedSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * cappedSpeed;
            }
        }

        float newY = velocity.y;
        if (ShouldJump())
        {
            newY = jumpForce;
            jumpQueued = false;
            jumpedSinceGrounded = true;
        }
        else if (jumpQueued && (Time.time - jumpQueuedAt) > jumpBufferTime)
        {
            jumpQueued = false;
        }

        rb.velocity = new(horizontalVelocity.x, newY, horizontalVelocity.z);
    }

    private bool ShouldApplyFriction()
    {
        // If the player is going to hop immediately after landing, don't instantly bleed speed.
        if (jumpQueued && (Time.time - jumpQueuedAt) <= jumpBufferTime)
        {
            if ((Time.time - lastLandedAt) <= bhopFrictionGraceTime)
            {
                return false;
            }
        }

        return true;
    }

    private bool ShouldJump()
    {
        if (!jumpQueued)
        {
            return false;
        }

        // Prevent double-jumps caused by re-queueing during coyote/buffer windows.
        if (jumpedSinceGrounded)
        {
            return false;
        }

        bool withinBuffer = (Time.time - jumpQueuedAt) <= jumpBufferTime;
        if (!withinBuffer)
        {
            return false;
        }

        return isGrounded || (Time.time - lastGroundedAt) <= coyoteTime;
    }

    private void ApplyFriction(ref Vector3 horizontalVelocity, float dt)
    {
        float speed = horizontalVelocity.magnitude;
        if (speed < 0.0001f)
        {
            horizontalVelocity = Vector3.zero;
            return;
        }

        float control = Mathf.Max(speed, stopSpeed);
        float drop = control * friction * dt;
        float newSpeed = Mathf.Max(0f, speed - drop);

        if (newSpeed != speed)
        {
            horizontalVelocity *= newSpeed / speed;
        }
    }

    private void Accelerate(ref Vector3 horizontalVelocity, Vector3 wishDir, float wishSpeed, float accel, float dt)
    {
        float currentSpeed = Vector3.Dot(horizontalVelocity, wishDir);
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0f)
        {
            return;
        }

        float accelSpeed = accel * dt * wishSpeed;
        if (accelSpeed > addSpeed)
        {
            accelSpeed = addSpeed;
        }

        horizontalVelocity += wishDir * accelSpeed;
    }

    private void Look()
    {
        // Get mouse input for looking around
        float mouseX = Input.GetAxis("Mouse X") * cameraSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * cameraSensitivity;

        // Rotate the camera up and down (inverting the Y axis)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent over-rotation

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Rotate camera
        transform.Rotate(Vector3.up * mouseX); // Rotate player
    }
    
    private void SetGrounded()
    {
        isGrounded = Physics.SphereCast(
            transform.position,
            groundCheckRadius,
            Vector3.down,
            out RaycastHit _,
            groundCheckDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );
    }
}