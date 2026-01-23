using UnityEngine;

public class RotatePlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 45f; // astetta sekunnissa

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
