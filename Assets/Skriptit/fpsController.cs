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

    [Header("Movement Bob (optional)")]
    [SerializeField] private Transform visualMesh;
    [SerializeField] private float bobSpeed = 8f;
    [SerializeField] private float bobAmount = 0.05f;

    private Rigidbody rb;

    private float yaw;
    private float pitch;
    private Vector2 moveInput;
    private bool jumpQueued;

    private bool movementEnabled = true;

    private float bobTimer;
    private Vector3 meshStartLocalPos;

    private Transform spawnTransform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (visualMesh != null)
            meshStartLocalPos = visualMesh.localPosition;
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

        // OWNER authority: owner tekee spawnin itse (varmin tapa)
        TeleportToSpawn_Local();

        // Ja ilmoittaa serverille (että serverin tila ei jää eri asentoon)
        SubmitPositionToServerRpc(rb.position, rb.rotation);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (!movementEnabled) return;

        ReadArrowKeys();
        ReadMouse();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (!movementEnabled) return;

        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));

        Vector3 dir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        var v = rb.linearVelocity;
        v.x = dir.x * moveSpeed;
        v.z = dir.z * moveSpeed;
        rb.linearVelocity = v;

        if (jumpQueued)
        {
            jumpQueued = false;
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            if (jumpAudio) jumpAudio.Play();
        }

        ApplyMovementBob();
    }

    public void DisableMovement()
    {
        movementEnabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Respawn()
    {
        if (!IsOwner) return;

        TeleportToSpawn_Local();
        SubmitPositionToServerRpc(rb.position, rb.rotation);
    }

    private void TeleportToSpawn_Local()
    {
        Vector3 target = spawnTransform != null ? spawnTransform.position : new Vector3(0f, 5f, 0f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = target;
        transform.position = target;
    }

    [ServerRpc]
    private void SubmitPositionToServerRpc(Vector3 pos, Quaternion rot)
    {
        // server asettaa oman tilansa samaksi (ei pakoteta muille clientille)
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = pos;
        transform.position = pos;

        rb.rotation = rot;
        transform.rotation = rot;
    }

    private void ReadArrowKeys()
    {
        if (Keyboard.current == null) return;

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
}
