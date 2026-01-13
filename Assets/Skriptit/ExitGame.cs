using UnityEngine;
using Unity.Netcode;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        ShutdownNetwork();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnApplicationQuit()
    {
        ShutdownNetwork();
    }

    private void OnDestroy()
    {
        ShutdownNetwork();
    }

    private void ShutdownNetwork()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.IsListening)
            nm.Shutdown();
    }
}
