// NetworkUI.cs
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
        if (hostButton)
            hostButton.onClick.AddListener(StartHost);

        if (clientButton)
            clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        HideUI();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        HideUI();
    }

    private void HideUI()
    {
        if (panelToHide) panelToHide.SetActive(false);
        if (hostButton) hostButton.interactable = false;
        if (clientButton) clientButton.interactable = false;
    }
}
