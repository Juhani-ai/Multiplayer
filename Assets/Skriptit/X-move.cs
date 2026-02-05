using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovePlatform : MonoBehaviour
{
    public enum MoveDirection { Left, Right }

    [Header("Movement")]
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public MoveDirection startDirection = MoveDirection.Right;

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

    void Start()
    {
        startPos = rb.position;

        // 👉 TÄMÄ ON SE OIKEA KOHTA
        moveDir = (startDirection == MoveDirection.Right)
            ? Vector3.right
            : Vector3.left;

        direction = 1;       // edestakainen liike
        travelled = 0f;
    }

    void FixedUpdate()
    {
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
        rb.MovePosition(targetPos);
    }
}
