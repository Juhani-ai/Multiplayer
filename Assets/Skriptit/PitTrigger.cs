// PitTrigger.cs
using UnityEngine;

public class PitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<NetworkPlayerController>();
        if (player == null) return;

        GameManager.Instance?.RequestRespawn(player);
    }
}
