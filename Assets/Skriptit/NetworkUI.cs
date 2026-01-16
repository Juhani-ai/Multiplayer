using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private GameObject panelToHide; // laita tähän UI-paneeli (optional)

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton puuttuu scenestä.");
            return;
        }

        if (hostButton != null)
            hostButton.onClick.AddListener(StartHost);

        if (clientButton != null)
            clientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
            AfterStart();
    }

    private void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
            AfterStart();
    }

    private void AfterStart()
    {
        if (hostButton != null) hostButton.interactable = false;
        if (clientButton != null) clientButton.interactable = false;
        if (panelToHide != null) panelToHide.SetActive(false);
    }
}
