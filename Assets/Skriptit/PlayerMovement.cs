using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float walkSpeed = 5f;
    public float runSpeed = 10f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Jos EI omistaja → disable kamera + audiolistener
        if (!IsOwner)
        {
            if (cameraTransform != null && cameraTransform.TryGetComponent<AudioListener>(out var listener))
                listener.enabled = false;

            if (cameraTransform != null && cameraTransform.TryGetComponent<Camera>(out var cam))
                cam.enabled = false;

            return; // Skipataan kaikki muu
        }

        // Jos on omistaja, lukitaan kursori
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null)
        {
            Debug.LogWarning("cameraTransform not assigned in prefab!");
        }
    }

    void Update()
    {
        if (!IsOwner || controller == null) return;

        // ESC vapauttaa hiiren
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Liike
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(h, 0f, v).normalized;
        Vector3 moveDirection = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * inputDirection;

        float speed = Input.GetKey(KeyCode.B) ? runSpeed : walkSpeed;
        controller.Move(moveDirection * speed * Time.deltaTime);

        // Hyppy
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        // Painovoima
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
