using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    public enum RotationDirection
    {
        Clockwise,
        CounterClockwise
    }

    [Header("Rotation Settings")]
    public float rotationSpeed = 45f; // astetta sekunnissa
    public RotationDirection direction = RotationDirection.Clockwise;

    void Update()
    {
        float dir = (direction == RotationDirection.Clockwise) ? -1f : 1f;
        transform.Rotate(0f, 0f, dir * rotationSpeed * Time.deltaTime);
    }
}
