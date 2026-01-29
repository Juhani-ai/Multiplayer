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
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager puuttuu");
            return;
        }

        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
                AfterStart();
            });
        }

        if (clientButton != null)
        {
            clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
                AfterStart();
            });
        }
    }

    private void AfterStart()
    {
        if (hostButton) hostButton.interactable = false;
        if (clientButton) clientButton.interactable = false;
        if (panelToHide) panelToHide.SetActive(false);
    }
}
