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
    [SerializeField] private AudioSource musicSource;

    [Header("Defaults")]
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.8f;
    [SerializeField] private bool overrideSavedZeroVolume = true; // if saved volume is 0, use default instead

    private bool isLoading = false;

    private Resolution[] resolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1600, height = 900 },
        new Resolution { width = 1920, height = 1080 },
        new Resolution { width = 2560, height = 1440 }
    };

    private void Start()
    {
        isLoading = true;

        SetupResolutionDropdown();
        LoadSettings();

        isLoading = false;
    }

    private void SetupResolutionDropdown()
    {
        if (ResDown == null) return;

        ResDown.ClearOptions();
        var options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            options.Add($"{resolutions[i].width} x {resolutions[i].height}");

            if (Screen.width == resolutions[i].width && Screen.height == resolutions[i].height)
                currentIndex = i;
        }

        ResDown.AddOptions(options);
        ResDown.value = currentIndex;
        ResDown.RefreshShownValue();
    }

    public void OnResolutionChanged(int index)
    {
        if (isLoading) return;
        if (index < 0 || index >= resolutions.Length) return;

        var r = resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen);

        PlayerPrefs.SetInt("ResIndex", index);
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        value = Mathf.Clamp01(value);

        if (musicSource == null) return;

        // Only change volume — DO NOT stop music
        musicSource.volume = value;

        // Ensure music is playing if volume > 0
        if (!musicSource.isPlaying && value > 0.01f)
        {
            if (musicSource.clip != null)
                musicSource.Play();
        }

        // Save only after loading phase
        if (!isLoading)
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            PlayerPrefs.Save();
        }
    }


    public void OnFullscreenToggle(bool isFullscreen)
    {
        if (isLoading) return;

        Screen.fullScreenMode = isFullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        // Re-apply resolution with the new fullscreen mode
        if (ResDown != null)
            ApplyResolution(ResDown.value, isFullscreen);
    }

    private void ApplyResolution(int index, bool fullscreen)
    {
        index = Mathf.Clamp(index, 0, resolutions.Length - 1);
        var r = resolutions[index];
        Screen.SetResolution(r.width, r.height, fullscreen);
    }

    private void LoadSettings()
    {
        // ----- Music -----
        float volume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        volume = Mathf.Clamp01(volume);

        // If player once saved 0, we can ignore it and use default
        if (overrideSavedZeroVolume && volume < 0.01f)
            volume = defaultMusicVolume;

        if (Musicbar != null) Musicbar.value = volume;
        OnMusicVolumeChanged(volume); // apply + (re)start music safely

        // ----- Fullscreen -----
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle != null) fullscreenToggle.isOn = fullscreen;

        Screen.fullScreenMode = fullscreen
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;

        // ----- Resolution -----
        int resIndex = PlayerPrefs.GetInt("ResIndex", 2);
        resIndex = Mathf.Clamp(resIndex, 0, resolutions.Length - 1);

        if (ResDown != null)
        {
            ResDown.value = resIndex;
            ResDown.RefreshShownValue();
        }

        ApplyResolution(resIndex, fullscreen);
    }

    // Optional: hook this to a "Reset Audio" button
    public void ResetMusicToDefault()
    {
        PlayerPrefs.DeleteKey("MusicVolume");
        PlayerPrefs.Save();

        if (Musicbar != null) Musicbar.value = defaultMusicVolume;
        OnMusicVolumeChanged(defaultMusicVolume);
    }
}
