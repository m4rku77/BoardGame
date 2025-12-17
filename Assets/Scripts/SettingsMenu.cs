using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject leaderboardPanel;

    [Header("Main menu objects")]
    public GameObject mainButtons; // empty object holding menu buttons
    public GameObject name;
    public GameObject dice;

    [Header("Fade Overlay (optional)")]
    public GameObject fadeOverlay;         // the black overlay GameObject (Image)




    void Start()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
    }

    // ---------- SETTINGS ----------

    public void OpenSettings()
    {
        DisableFadeForMenuPanels();

        HideMainMenu();
        if (leaderboardPanel) leaderboardPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel) settingsPanel.SetActive(false);

        // keep fade disabled for main menu UI switching
        DisableFadeForMenuPanels();

        ShowMainMenu();
    }


    // ---------- LEADERBOARD ----------

    public void OpenLeaderboard()
    {
        DisableFadeForMenuPanels();

        HideMainMenu();
        if (settingsPanel) settingsPanel.SetActive(false);
        if (leaderboardPanel) leaderboardPanel.SetActive(true);
    }

    public void CloseLeaderboard()
    {
        if (leaderboardPanel) leaderboardPanel.SetActive(false);

        // keep fade disabled for main menu UI switching
        DisableFadeForMenuPanels();

        ShowMainMenu();
    }

    // ---------- FADE CONTROL ----------

    void DisableFadeForMenuPanels()
    {
        // Option A: disable overlay object entirely
        if (fadeOverlay) fadeOverlay.SetActive(false);

  
    }

    // Call this only when you actually want fades (e.g., before loading a scene)
    public void EnableFadeForSceneTransition()
    {
        if (fadeOverlay) fadeOverlay.SetActive(true);
   
    }

    // ---------- HELPERS ----------

    void HideMainMenu()
    {
        if (mainButtons) mainButtons.SetActive(false);
        if (name) name.SetActive(false);
        if (dice) dice.SetActive(false);
    }

    void ShowMainMenu()
    {
        if (mainButtons) mainButtons.SetActive(true);
        if (name) name.SetActive(true);
        if (dice) dice.SetActive(true);
    }

    // ---------- RESOLUTION ----------

    public void SetResolution1920x1080()
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.FullScreenWindow);
    }

    public void SetResolution1280x1024()
    {
        Screen.SetResolution(1280, 1024, FullScreenMode.FullScreenWindow);
    }

    public void SetResolution16by10()
    {
        Screen.SetResolution(1680, 1050, FullScreenMode.FullScreenWindow);
    }

    // ---------- MUSIC ----------



}
