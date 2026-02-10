// ExitGame.cs
using Unity.Netcode;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitApplication()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            NetworkManager.Singleton.Shutdown();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
