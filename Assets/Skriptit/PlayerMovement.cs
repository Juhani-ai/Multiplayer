using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpImpulse = 5f;
    [SerializeField] private float groundCheckExtra = 0.08f;

    private Rigidbody rb;
    private Collider col;

    private Vector2 moveInput;
    private bool jumpQueued;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        // Perusvakaus (ei pakko, mutta auttaa “tärinään/jumiin”)
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public override void OnNetworkSpawn()
    {
        // Vain omistaja simuloi omaa rigidbodyaan (helpoin malli tähän tehtävään).
        rb.isKinematic = !IsOwner;

        if (!IsOwner)
        {
            // Varmuuden vuoksi: ei-omistajan RB ei “sekoile”
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Liike
        Vector3 planar = new Vector3(moveInput.x, 0f, moveInput.y);
        if (planar.sqrMagnitude > 1f) planar.Normalize();

        Vector3 v = rb.linearVelocity;
        Vector3 targetPlanar = planar * moveSpeed;
        rb.linearVelocity = new Vector3(targetPlanar.x, v.y, targetPlanar.z);

        // Hyppy
        if (jumpQueued)
        {
            jumpQueued = false;

            if (IsGrounded())
            {
                // tee hypystä “siisti”, ei kummaa pomppua
                if (rb.linearVelocity.y < 0f)
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

                rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
            }
        }
    }

    private bool IsGrounded()
    {
        // Raycast colliderin alareunasta
        Bounds b = col.bounds;
        Vector3 origin = new Vector3(b.center.x, b.min.y + 0.02f, b.center.z);
        float dist = groundCheckExtra + 0.04f;

        return Physics.Raycast(origin, Vector3.down, dist, ~0, QueryTriggerInteraction.Ignore);
    }
}
