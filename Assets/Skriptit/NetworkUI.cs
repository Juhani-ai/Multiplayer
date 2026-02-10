using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject panelToHide;

    private void Start()
    {
        if (hostButton) hostButton.onClick.AddListener(StartHost);
        if (clientButton) clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        bool ok = NetworkManager.Singleton.StartHost();
        Debug.Log($"StartHost() -> {ok}");

        if (ok) HideUI();
        else Debug.LogError("Host ei lähtenyt. Tarkista NetworkManager/Transport asetukset.");
    }

    private void StartClient()
    {
        bool ok = NetworkManager.Singleton.StartClient();
        Debug.Log($"StartClient() -> {ok}");

        if (ok) HideUI();
        else Debug.LogError("Client ei lähtenyt. Tarkista NetworkManager/Transport asetukset.");
    }

    private void HideUI()
    {
        if (panelToHide) panelToHide.SetActive(false);
        if (hostButton) hostButton.interactable = false;
        if (clientButton) clientButton.interactable = false;
    }
}
