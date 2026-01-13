using Unity.Netcode;
using UnityEngine;

public class MouseLook : NetworkBehaviour
{
    [Header("Assign these")]
    [SerializeField] private Rigidbody playerRb;       // rootin Rigidbody
    [SerializeField] private Transform cameraPivot;    // CameraPivot
    [SerializeField] private float mouseSensitivity = 150f;

    private float xRotation;
    private bool cursorLocked = true;

    private void Awake()
    {
        if (playerRb == null) playerRb = GetComponentInParent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) enabled = false;
        else LockCursor(true);
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Esc togglaa hiiren lukon -> voit klikata UI:ta
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLocked = !cursorLocked;
            LockCursor(cursorLocked);
        }

        if (!cursorLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -85f, 85f);

        // Pitch vain kameralle
        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Yaw Rigidbodylle siististi
        if (playerRb != null)
        {
            Quaternion yawRot = Quaternion.Euler(0f, mouseX * 10f, 0f);
            playerRb.MoveRotation(playerRb.rotation * yawRot);
        }
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
