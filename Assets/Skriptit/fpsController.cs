using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetworkFirstPersonController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private Transform cameraPivot;      // CameraPivot
    [SerializeField] private Camera playerCamera;        // Main Camera
    [SerializeField] private AudioListener audioListener;// AudioListener (kamerassa)

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;  // kaikki layerit oletuksena

    private Rigidbody rb;

    private float pitch;
    private float yaw;
    private bool cursorLocked = true;

    private Vector2 moveInput;     // x=left/right, y=forward/back (nuolet)
    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Estää “kierimisen”
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetLocalVisuals(true);
            LockCursor(true);
            yaw = transform.eulerAngles.y;
        }
        else
        {
            // Ei-omistajat: ei kameraa, ei audiota, ei ohjausta
            SetLocalVisuals(false);
            enabled = false;
        }
    }

    private void SetLocalVisuals(bool enabledForOwner)
    {
        if (playerCamera != null) playerCamera.enabled = enabledForOwner;
        if (audioListener != null) audioListener.enabled = enabledForOwner;
    }

    private void Update()
    {
        if (!IsOwner) return;

        // ESC vapauttaa hiiren -> pystyt klikkaamaan UI:ta (Exit tms.)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            LockCursor(cursorLocked);
        }

        ReadArrowKeyMovement();

        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;

        if (!cursorLocked) return;

        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mx;
        pitch -= my;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Käännä pelaajan kroppa yaw:lla
        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));

        // Liike nuolilla suhteessa pelaajan suuntaan (FPS)
        Vector3 dir = (transform.right * moveInput.x + transform.forward * moveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 targetVel = dir * moveSpeed;
        Vector3 currentVel = rb.linearVelocity; // Unity 6: linearVelocity ok
        rb.linearVelocity = new Vector3(targetVel.x, currentVel.y, targetVel.z);

        // Hyppy
        if (jumpQueued)
        {
            jumpQueued = false;
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            }
        }
    }

    private void ReadArrowKeyMovement()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))  x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.UpArrow))    y += 1f;
        if (Input.GetKey(KeyCode.DownArrow))  y -= 1f;

        moveInput = new Vector2(x, y);
    }

    private bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, 0.1f + groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
