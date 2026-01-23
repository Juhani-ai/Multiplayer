using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<NetworkFirstPersonController>();
        if (player != null && player.IsOwner)
        {
            GameManager.Instance.FinishGame(player);
        }
    }
}
