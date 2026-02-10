// NetworkPlayerController.cs
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class NetworkPlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -30f;     // negatiivinen
    [SerializeField] private float groundSnap = -12f;  // kun grounded -> paina alas, ei leijuntaa

    [Header("Look")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;
    [SerializeField] private float mouseSensitivity = 0.12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController controller;
    private float yVelocity;
    private float yaw;
    private float pitch;

    private Transform spawnT;
    private Vector3 spawnPos;

    // Moving platform sticking
    private Transform groundPlatform;
    private Vector3 lastPlatformPos;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        spawnT = GameObject.FindGameObjectWithTag("Spawn")?.transform;
        spawnPos = spawnT ? spawnT.position : transform.position;

        if (!IsOwner)
        {
            if (playerCamera) playerCamera.enabled = false;
            if (audioListener) audioListener.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        Look();
        Move();
    }

    private void Move()
    {
        Vector3 gcPos = groundCheck ? groundCheck.position : (transform.position + Vector3.down * 0.9f);
        bool grounded = Physics.CheckSphere(gcPos, groundRadius, groundLayer, QueryTriggerInteraction.Ignore);

        ApplyPlatformDelta(gcPos, grounded);

        if (grounded && yVelocity < 0f)
            yVelocity = groundSnap;

        bool jumpPressed =
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

        if (grounded && jumpPressed)
            yVelocity = jumpForce;

        yVelocity += gravity * Time.deltaTime;

        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
        }

        Vector3 move = (transform.right * x + transform.forward * z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        Vector3 velocity = move * moveSpeed;
        velocity.y = yVelocity;

        controller.Move(velocity * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private void ApplyPlatformDelta(Vector3 groundCheckPos, bool grounded)
    {
        if (!grounded)
        {
            groundPlatform = null;
            return;
        }

        // Ray alas -> mikä collider on jalkojen alla
        if (Physics.Raycast(groundCheckPos + Vector3.up * 0.2f, Vector3.down, out RaycastHit hit, 1.2f, groundLayer, QueryTriggerInteraction.Ignore))
        {
            Transform newPlatform = hit.collider.transform;

            if (groundPlatform != newPlatform)
            {
                groundPlatform = newPlatform;
                lastPlatformPos = groundPlatform.position;
                return;
            }

            // Platform liikkui -> siirrä pelaajaa sama delta
            Vector3 delta = groundPlatform.position - lastPlatformPos;
            if (delta.sqrMagnitude > 0f)
                controller.Move(delta);

            lastPlatformPos = groundPlatform.position;
        }
    }

    private void Look()
    {
        if (Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * mouseSensitivity;
        pitch -= delta.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        if (cameraPivot)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    // ===== Server-authoritative respawn (käytetään vain jos joku kutsuu) =====
    public void ServerRespawn()
    {
        if (!IsServer) return;

        if (spawnT == null) spawnT = GameObject.FindGameObjectWithTag("Spawn")?.transform;
        spawnPos = spawnT ? spawnT.position : spawnPos;

        yVelocity = 0f;

        if (TryGetComponent<NetworkTransform>(out var nt))
            nt.Teleport(spawnPos, transform.rotation, transform.localScale);
        else
            transform.position = spawnPos;

        RespawnOwnerRpc(spawnPos);
    }

    [Rpc(SendTo.Owner)]
    private void RespawnOwnerRpc(Vector3 pos)
    {
        yVelocity = 0f;

        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
    }
}
