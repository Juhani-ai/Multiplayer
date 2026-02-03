using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class NetworkFirstPersonController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    private Rigidbody rb;
    private bool movementEnabled = true;
    private float yaw;
    private float pitch;
    private Vector2 moveInput;
    private bool jumpQueued;

    private Transform spawnTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void OnNetworkSpawn()
    {
        var spawnGO = GameObject.FindGameObjectWithTag("Spawn");
        spawnTransform = spawnGO != null ? spawnGO.transform : null;

        if (!IsOwner)
        {
            if (playerCamera) playerCamera.enabled = false;
            if (audioListener) audioListener.enabled = false;
            return;
        }

        yaw = transform.eulerAngles.y;
        pitch = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ForceRespawn();
    }

    private void Update()
    {
        if (!IsOwner || !movementEnabled) return;

        ReadMovement();
        ReadMouse();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !movementEnabled) return;

        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));

        Vector3 dir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        rb.linearVelocity = new Vector3(
            dir.x * moveSpeed,
            rb.linearVelocity.y,
            dir.z * moveSpeed
        );

        if (jumpQueued)
        {
            jumpQueued = false;
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }
    }

    // === RESPawn ===
    public void ForceRespawn()
    {
        Vector3 pos = spawnTransform != null
            ? spawnTransform.position
            : new Vector3(0f, 5f, 0f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        transform.position = pos;

        SubmitPositionToServerRpc(pos);
    }

    [ServerRpc]
    private void SubmitPositionToServerRpc(Vector3 pos)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        transform.position = pos;
    }

    // === INPUT ===
    private void ReadMovement()
    {
        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y -= 1f;

        moveInput = new Vector2(x, y);
    }

    private void ReadMouse()
    {
        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        if (cameraPivot)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void DisableMovement()
    {
        if (!IsOwner) return;

        movementEnabled = false;
        rb.linearVelocity = Vector3.zero;
    }
}
