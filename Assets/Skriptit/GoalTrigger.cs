// GoalTrigger.cs
using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<NetworkFirstPersonController>();
        if (player == null || !player.IsOwner) return;

        GameManager.Instance?.Finish();
    }
}
