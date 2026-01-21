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
    private Vector3 velocity = Vector3.zero;

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
        SetGrounded();
        Move();
        Jump();
        Look();
        ApplyVelocity();
    }

    private void Move()
    {
        // Get input for movement (WASD keys)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Calculate movement direction relative to player's current forward direction
        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;

        // Prevent faster diagonal movement by clamping the desired horizontal direction to length 1.
        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // no crouch, sprint functionality yet

        // Preserve current vertical velocity from physics; we only author horizontal movement here.
        float y = rb.velocity.y;

        Vector3 targetHorizontalVelocity = moveDirection * moveSpeed;
        Vector3 currentHorizontalVelocity = new(rb.velocity.x, 0f, rb.velocity.z);

        // When there's no input, decelerate smoothly toward zero; otherwise go to target speed.
        Vector3 newHorizontalVelocity = targetHorizontalVelocity;
        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            newHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, moveDecelerationRate * Time.deltaTime);
        }

        velocity = new(newHorizontalVelocity.x, y, newHorizontalVelocity.z);
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Replace vertical velocity with a jump impulse.
            velocity = new(velocity.x, jumpForce, velocity.z);
        }
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

    private void ApplyVelocity()
    {
        rb.velocity = 50f * Time.fixedDeltaTime * velocity; // Scale velocity for FixedUpdate timing
    }
}