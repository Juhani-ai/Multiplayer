// PitTrigger.cs
using UnityEngine;

public class PitTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<NetworkFirstPersonController>();
        if (player == null || !player.IsOwner) return;

        // Tämä on nyt "idioottivarma": löytyy varmasti
        player.ForceRespawn();
    }
}
