using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private GameObject finishText;
    private bool finished;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ResetState();
    }

    public void Finish()
    {
        if (finished) return;

        finished = true;

        if (finishText)
            finishText.SetActive(true);
    }

    void Update()
    {
        if (!finished) return;

        if (Input.anyKeyDown)
        {
            Restart();
        }
    }

    private void Restart()
    {
        // TÄRKEÄ: nollaa ensin oma tila
        finished = false;

        // Lataa scene uudelleen (toimii myös Netcodessa)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetState()
    {
        finished = false;

        if (finishText)
            finishText.SetActive(false);
    }
}
