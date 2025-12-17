using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenuScript : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private GameObject settingsPanel;

    public bool IsPaused { get; private set; }
    private float prevTimeScale = 1f;

    private void Start()
    {
        ResumeGame();
        HideAll();
    }

    public void GoToMenu()
    {
        // Make sure time is running again
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene("SampleScene");
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused) Continue();
            else Pause();
        }
    }

    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        HideAll();
        pausePanel.SetActive(true);
    }

    public void Continue()
    {
        ResumeGame();
        HideAll();
    }

    public void OpenLeaderboard()
    {
        HideAll();
        leaderboardPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        HideAll();
        settingsPanel.SetActive(true);
    }

    public void BackToPause()
    {
        HideAll();
        pausePanel.SetActive(true);
    }

    private void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
    }

    private void HideAll()
    {
        pausePanel.SetActive(false);
        leaderboardPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }
}
