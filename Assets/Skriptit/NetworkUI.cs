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
        if (hostButton) hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        if (clientButton) clientButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    private void Hide()
    {
        if (panelToHide) panelToHide.SetActive(false);
        if (hostButton) hostButton.interactable = false;
        if (clientButton) clientButton.interactable = false;
    }
}
