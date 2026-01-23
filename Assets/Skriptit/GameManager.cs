using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject finishText;

    private bool gameFinished = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (finishText)
            finishText.SetActive(false);
    }

    public void FinishGame(NetworkFirstPersonController player)
    {
        if (gameFinished) return;
        gameFinished = true;

        if (finishText)
            finishText.SetActive(true);

        player.DisableMovement();
    }
}
