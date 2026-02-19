using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// TEMPORARY: Test Rigidbody no-clip free-fly controller.
/// Attach to the same player object when you need fast level traversal/testing.
/// Toggle with F8. Moves with project Move input, rises with Jump, falls with Left Ctrl.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class TemporaryRigidbodyNoClip : MonoBehaviour
{
    [Header("TEMPORARY NO-CLIP")]
    [SerializeField]
    private bool enabledAtStart = false;
    [SerializeField]
    private Key toggleKey = Key.F8;
    [SerializeField]
    private float flySpeed = 10f;
    [SerializeField]
    private float flySprintMultiplier = 2f;
    [SerializeField]
    private string moveActionName = "Move";
    [SerializeField]
    private string jumpActionName = "Jump";
    [SerializeField]
    private string sprintActionName = "Sprint";

    private Rigidbody rb;
    private Collider[] colliders;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private bool noClipEnabled;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(includeInactive: false);

        moveAction = ControlUtil.FindProjectAction(moveActionName);
        jumpAction = ControlUtil.FindProjectAction(jumpActionName);
        sprintAction = ControlUtil.FindProjectAction(sprintActionName);

        SetNoClip(enabledAtStart);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            SetNoClip(!noClipEnabled);
        }

        if (!noClipEnabled)
        {
            return;
        }

        Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        Vector3 wishDir = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
        wishDir = Vector3.ClampMagnitude(wishDir, 1f);

        float vertical = 0f;
        if (jumpAction != null && jumpAction.IsPressed())
        {
            vertical += 1f;
        }
        if (Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed)
        {
            vertical -= 1f;
        }

        Vector3 velocity = wishDir * flySpeed + (Vector3.up * vertical * flySpeed);
        if (sprintAction != null && sprintAction.IsPressed())
        {
            velocity *= flySprintMultiplier;
        }

        rb.velocity = velocity;
    }

    private void SetNoClip(bool enabled)
    {
        noClipEnabled = enabled;

        if (rb == null)
        {
            return;
        }

        rb.useGravity = !enabled;
        rb.velocity = Vector3.zero;

        if (colliders != null)
        {
            foreach (var col in colliders)
            {
                if (col != null)
                {
                    col.enabled = !enabled;
                }
            }
        }
    }
}