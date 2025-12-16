using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuScript : MonoBehaviour
{
    [Header("Pause UI (in THIS gameplay scene)")]
    [SerializeField] private GameObject pausePanel;

    [Header("Scenes (must match Build Settings names)")]
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private string leaderboardSceneName = "Leaderboard";
    [SerializeField] private string settingsSceneName = "Settings";

    public bool IsPaused { get; private set; }
    private float prevTimeScale = 1f;

    private void Start()
    {
        ResumeGame();
        if (pausePanel != null) pausePanel.SetActive(false);
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

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void Continue()
    {
        if (!IsPaused) return;

        ResumeGame();
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void OpenLeaderboard()
    {
        ResumeGame();
        SceneManager.LoadScene(leaderboardSceneName);
    }

    public void OpenSettings()
    {
        ResumeGame();
        SceneManager.LoadScene(settingsSceneName);
    }

    public void OpenMenu()
    {
        ResumeGame();
        SceneManager.LoadScene(menuSceneName);
    }

    private void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
    }
}
