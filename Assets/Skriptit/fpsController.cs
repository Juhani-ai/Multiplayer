using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class NetworkFirstPersonController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private AudioSource jumpAudio;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    [Header("Movement Bob")]
    [SerializeField] private Transform visualMesh;
    [SerializeField] private float bobSpeed = 8f;
    [SerializeField] private float bobAmount = 0.05f;

    private float bobTimer;
    private Vector3 meshStartLocalPos;

    private Rigidbody rb;
    private float yaw;
    private float pitch;
    private Vector2 moveInput;
    private bool jumpQueued;
    private bool movementEnabled = true;

    private float respawnY = -20f;
    private Vector3 spawnPoint = new Vector3(0f, 5f, 0f);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (visualMesh)
        meshStartLocalPos = visualMesh.localPosition;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (playerCamera) playerCamera.enabled = false;
            if (audioListener) audioListener.enabled = false;
            enabled = false;
            return;
        }

        GameObject spawn = GameObject.FindGameObjectWithTag("Spawn");
        if (spawn != null)
        {
            rb.position = spawn.transform.position;
            rb.linearVelocity = Vector3.zero;
        }

        yaw = transform.eulerAngles.y;
        pitch = 0f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!movementEnabled) return;

        ReadArrowKeys();
        ReadMouse();

        if (Mouse.current.leftButton.wasPressedThisFrame)
            jumpQueued = true;

        // Respawn jos tippuu
        if (transform.position.y < respawnY)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.MovePosition(spawnPoint);
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!movementEnabled) return;


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

            if (jumpAudio && !jumpAudio.isPlaying)
            jumpAudio.Play();
        }

        ApplyMovementBob();
    }

    private void ApplyMovementBob()
    {
        if (!visualMesh) return;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        if (horizontalVelocity.magnitude > 0.1f)
        {
            bobTimer += Time.fixedDeltaTime * bobSpeed;

            float bobY = Mathf.Sin(bobTimer) * bobAmount;
            float bobX = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;

            visualMesh.localPosition = meshStartLocalPos + new Vector3(bobX, bobY, 0f);
        }
        else
        {
            bobTimer = 0f;
            visualMesh.localPosition = Vector3.Lerp(visualMesh.localPosition, meshStartLocalPos, Time.fixedDeltaTime * 8f);
        }
    }

    private void ReadArrowKeys()
    {
        float x = 0f;
        float y = 0f;

        if (Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) x += 1f;
        if (Keyboard.current.upArrowKey.isPressed) y += 1f;
        if (Keyboard.current.downArrowKey.isPressed) y -= 1f;

        moveInput = new Vector2(x, y);
    }

    private void ReadMouse()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        if (cameraPivot)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    public void DisableMovement()
    {
        movementEnabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
