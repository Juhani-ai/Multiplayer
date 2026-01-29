// ExitGame.cs
using Unity.Netcode;
using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void QuitGame()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
