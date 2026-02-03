using UnityEngine;

public class PitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<NetworkFirstPersonController>();
        if (player == null) return;
        if (!player.IsOwner) return;

        player.ForceRespawn();
    }
}
