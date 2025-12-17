using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkUI : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.BeginVertical("box");

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Client"))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    void Start()
    {
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartHost();
            });
        }

        if (clientButton != null)
        {
            clientButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartClient();
            });
        }
    }
}
