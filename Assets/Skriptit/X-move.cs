using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovePlatform : MonoBehaviour
{
    public enum MoveDirection { Left, Right }

    [Header("Movement")]
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public MoveDirection startDirection = MoveDirection.Right;

    // ✅ TÄMÄ PUUTTUI: fpsController.cs käyttää tätä
    public Vector3 DeltaThisFrame { get; private set; }

    Rigidbody rb;
    Vector3 startPos;
    Vector3 moveDir;

    float travelled;
    int direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void OnEnable()
    {
        DeltaThisFrame = Vector3.zero;
    }

    void Start()
    {
        startPos = rb.position;

        moveDir = (startDirection == MoveDirection.Right)
            ? Vector3.right
            : Vector3.left;

        direction = 1;
        travelled = 0f;

        DeltaThisFrame = Vector3.zero;
    }

    void FixedUpdate()
    {
        // ❗ KUN PELI EI OLE KÄYNNISSÄ → EI LIIKUTETA
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            DeltaThisFrame = Vector3.zero;
            return;
        }

        Vector3 before = rb.position;

        float step = moveSpeed * Time.fixedDeltaTime;
        travelled += step * direction;

        if (travelled >= moveDistance)
        {
            travelled = moveDistance;
            direction = -1;
        }
        else if (travelled <= 0f)
        {
            travelled = 0f;
            direction = 1;
        }

        Vector3 targetPos = startPos + moveDir * travelled;

        // ✅ Tämä on se delta, jota pelaaja voi käyttää "pysyäkseen kyydissä"
        DeltaThisFrame = targetPos - before;

        rb.MovePosition(targetPos);
    }
}
