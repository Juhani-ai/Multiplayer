using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class NetworkFirstPersonController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 150f;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private AudioListener audioListener;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Fall Reset")]
    [SerializeField] private float fallYLimit = -50f;
    [SerializeField] private float respawnRadius = 10f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 0.5f;

    private Rigidbody rb;

    private float pitch;
    private float yaw;
    private bool cursorLocked = true;

    private Vector2 moveInput;
    private bool jumpQueued;
    private bool respawning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
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
            SetLocalVisuals(false);
            enabled = false;
        }
    }

    private void SetLocalVisuals(bool enabledForOwner)
    {
        if (playerCamera != null) playerCamera.enabled = enabledForOwner;
        if (audioListener != null) audioListener.enabled = enabledForOwner;
        if (fadeCanvas != null) fadeCanvas.alpha = 0f;
    }

    private void Update()
    {
        if (!IsOwner || respawning) return;

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
        if (!IsOwner || respawning) return;

        if (transform.position.y < fallYLimit)
        {
            StartCoroutine(RespawnRoutine());
            return;
        }

        rb.MoveRotation(Quaternion.Euler(0f, yaw, 0f));

        Vector3 dir = (transform.right * moveInput.x + transform.forward * moveInput.y);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 targetVel = dir * moveSpeed;
        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(targetVel.x, currentVel.y, targetVel.z);

        if (jumpQueued)
        {
            jumpQueued = false;
            if (IsGrounded())
            {
                rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;

        LockCursor(false);

        if (fadeCanvas != null)
            yield return Fade(0f, 1f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 randomOffset = new Vector3(
            Random.Range(-respawnRadius, respawnRadius),
            0f,
            Random.Range(-respawnRadius, respawnRadius)
        );

        rb.position = randomOffset;
        rb.rotation = Quaternion.identity;

        yaw = 0f;
        pitch = 0f;

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.identity;

        yield return new WaitForFixedUpdate();

        if (fadeCanvas != null)
            yield return Fade(1f, 0f);

        LockCursor(true);
        respawning = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            fadeCanvas.alpha = a;
            yield return null;
        }
        fadeCanvas.alpha = to;
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
        return Physics.Raycast(
            origin,
            Vector3.down,
            0.1f + groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
