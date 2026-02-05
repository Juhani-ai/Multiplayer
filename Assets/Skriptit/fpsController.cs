using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class NetworkFirstPersonController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float fallMultiplier = 3f;

    [Header("Ground")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = 0.25f;
    [SerializeField] LayerMask groundLayer;

    [Header("Look")]
    [SerializeField] float mouseSensitivity = 0.12f;
    [SerializeField] Transform cameraPivot;
    [SerializeField] Camera playerCamera;
    [SerializeField] AudioListener audioListener;

    Rigidbody rb;

    float yaw, pitch;
    Vector2 moveInput;
    bool jumpQueued;
    bool grounded;
    bool finished;

    Transform spawnPoint;

    // ---- platform ----
    Rigidbody platformRb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public override void OnNetworkSpawn()
    {
        spawnPoint = GameObject.FindGameObjectWithTag("Spawn")?.transform;

        if (!IsOwner)
        {
            playerCamera.enabled = false;
            audioListener.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Respawn();
    }

    void Update()
    {
        if (!IsOwner || finished) return;

        ReadMovement();
        ReadMouse();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        if (!IsOwner || finished) return;

        grounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundLayer
        );

        Vector3 velocity = rb.linearVelocity;

        // ---- horisontaalinen liike ----
        Vector3 inputDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        velocity.x = inputDir.x * moveSpeed;
        velocity.z = inputDir.z * moveSpeed;

        // ---- HYPPY ----
        if (jumpQueued && grounded)
        {
            velocity.y = jumpForce;
        }

        // ---- nopeutettu lasku ----
        if (velocity.y < 0f)
        {
            velocity.y += Physics.gravity.y * fallMultiplier * Time.fixedDeltaTime;
        }

        // ---- PLATFORMIN VELOCITY (TÄRINÄ POIS) ----
        if (grounded && platformRb != null)
        {
            velocity += platformRb.linearVelocity;
        }

        rb.linearVelocity = velocity;
        jumpQueued = false;

        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));
    }

    // ---------- COLLISIONS ----------
    void OnCollisionStay(Collision col)
    {
        if (!IsOwner) return;

        if (col.collider.CompareTag("Platform"))
        {
            platformRb = col.rigidbody;
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (col.collider.CompareTag("Platform"))
        {
            platformRb = null;
        }
    }

    // ---------- INPUT ----------
    void ReadMovement()
    {
        float x = 0, y = 0;

        if (Keyboard.current.leftArrowKey.isPressed)  x -= 1;
        if (Keyboard.current.rightArrowKey.isPressed) x += 1;
        if (Keyboard.current.upArrowKey.isPressed)    y += 1;
        if (Keyboard.current.downArrowKey.isPressed)  y -= 1;

        moveInput = new Vector2(x, y);
    }

    void ReadMouse()
    {
        Vector2 d = Mouse.current.delta.ReadValue();
        yaw += d.x * mouseSensitivity;
        pitch -= d.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    // ---------- TRIGGERS ----------
    void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("Goal"))
        {
            finished = true;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            GameManager.Instance?.Finish();
        }

        if (other.CompareTag("Pit"))
        {
            Respawn();
        }
    }

    void Respawn()
    {
        finished = false;
        rb.isKinematic = false;

        Vector3 pos = spawnPoint ? spawnPoint.position : Vector3.up * 5f;
        rb.linearVelocity = Vector3.zero;
        rb.position = pos;

        jumpQueued = false;
        platformRb = null;
    }

    public void ForceRespawn() => Respawn();
}
