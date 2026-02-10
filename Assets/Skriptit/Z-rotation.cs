using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise
    }

    [Header("Rotation Settings")]
    public float rotationSpeed = 45f;
    public RotationDirection direction = RotationDirection.Clockwise;

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            return;

        float dir = (direction == RotationDirection.Clockwise) ? -1f : 1f;
        transform.Rotate(0f, 0f, dir * rotationSpeed * Time.deltaTime);
    }
}
