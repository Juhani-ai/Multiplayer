using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;

    void Start()
    {
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
            });
        }

        if (clientButton != null)
        {
            clientButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartClient();
            });
        }

        if (serverButton != null)
        {
            serverButton.onClick.AddListener(() =>
            {
                NetworkManager.Singleton.StartHost();
            });
        }

        // Voit lisätä myös serverButtonin tarvittaessa
    }
}
