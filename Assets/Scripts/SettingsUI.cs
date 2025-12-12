using UnityEngine;

public class SettingsUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject settingsPanel;

    [Header("Objects to hide")]
    public GameObject buttons;   // empty object with main menu buttons
    public GameObject name;      // title / logo / name object

    void Start()
    {
        settingsPanel.SetActive(false);
    }

    // SETTINGS BUTTON
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);

        buttons.SetActive(false);
        name.SetActive(false);
    }

    // BACK BUTTON
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);

        buttons.SetActive(true);
        name.SetActive(true);
    }
}
