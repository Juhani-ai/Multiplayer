using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 5f;
    public float moveSpeed = 2f;

    private Vector3 startPos;
    private bool goingForward = true;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.position = startPos + Vector3.right * offset;
    }
}
