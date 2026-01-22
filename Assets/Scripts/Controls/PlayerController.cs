using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;
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
        SetGrounded();
        ApplyMovement();
        jumpQueued = false;
    }

    private void ReadMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

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
        if (Input.GetButtonDown("Jump"))
        {
            jumpQueued = true;
        }
    }

    private void ApplyMovement()
    {
        Vector3 currentVelocity = rb.velocity;
        Vector3 currentHorizontalVelocity = new(currentVelocity.x, 0f, currentVelocity.z);
        Vector3 targetHorizontalVelocity = desiredMoveDirection * moveSpeed;

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
        isGrounded = Physics.SphereCast(transform.position, 0.1f, Vector3.down, out RaycastHit _, 2f);
    }
}