// GameManager.cs
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsPlaying => isPlaying.Value;

    private readonly NetworkVariable<bool> isPlaying = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) isPlaying.Value = true;
    }

    public void ReachGoal()
    {
        if (!IsSpawned) return;

        if (IsServer) isPlaying.Value = false;
        else ReachGoalRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void ReachGoalRpc()
    {
        isPlaying.Value = false;
    }

    // Jos joskus haluat respawnin takaisin: tätä voi kutsua suoraan pelaajalta.
    public void RequestRespawn(NetworkPlayerController player)
    {
        if (player == null) return;
        if (!IsSpawned) return;

        if (IsServer) player.ServerRespawn();
        else RequestRespawnRpc(player.NetworkObjectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestRespawnRpc(ulong playerNetworkObjectId)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var no))
            return;

        var player = no.GetComponent<NetworkPlayerController>();
        if (player == null) return;

        player.ServerRespawn();
    }
}
