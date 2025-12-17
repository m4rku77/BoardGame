using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsMenuScript : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Dropdown ResDown;
    [SerializeField] private Slider Musicbar;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource; // drag your Music AudioSource here

    private Resolution[] resolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 }
    };

    private void Start()
    {
        SetupResolutionDropdown();
        LoadSettings();
    }

    private void SetupResolutionDropdown()
    {
        ResDown.ClearOptions();
        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);

            if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height)
                currentIndex = i;
        }

        ResDown.AddOptions(options);
        ResDown.value = currentIndex;
        ResDown.RefreshShownValue();
    }

    public void OnResolutionChanged(int index)
    {
        var r = resolutions[index];
        bool fullscreen = Screen.fullScreen;

        Screen.SetResolution(r.width, r.height, fullscreen);

        PlayerPrefs.SetInt("ResIndex", index);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        // value should be 0..1
        if (musicSource != null)
        {
            musicSource.volume = value;

            if (!musicSource.isPlaying && value > 0.001f)
                musicSource.Play();
        }

        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        if (isFullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Music
        float volume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        Musicbar.value = volume;
        OnMusicVolumeChanged(volume);

        // Fullscreen
        if (fullscreenToggle != null)
        {
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = fullscreen;
            OnFullscreenToggle(fullscreen);
        }

        // Resolution
        int resIndex = PlayerPrefs.GetInt("ResIndex", 2);
        ResDown.value = Mathf.Clamp(resIndex, 0, resolutions.Length - 1);
        ResDown.RefreshShownValue();
        OnResolutionChanged(ResDown.value);
    }
}
