using UnityEngine;
using UnityEngine.UI;

public class MusicToggleSettings : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource musicSource;

    [Header("Toggles")]
    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    [Header("Clips")]
    public AudioClip music1;
    public AudioClip music2;
    public AudioClip music3;

    bool _ignore;

    void Awake()
    {
        // Hook listeners
        toggle1.onValueChanged.AddListener(isOn => { if (isOn) SelectMusic(1); });
        toggle2.onValueChanged.AddListener(isOn => { if (isOn) SelectMusic(2); });
        toggle3.onValueChanged.AddListener(isOn => { if (isOn) SelectMusic(3); });
    }

    void Start()
    {
        // If none selected, pick first by default
        if (!toggle1.isOn && !toggle2.isOn && !toggle3.isOn)
        {
            _ignore = true;
            toggle1.isOn = true;
            _ignore = false;
            SelectMusic(1);
        }
        else
        {
            // play whichever is already selected in the editor
            if (toggle1.isOn) SelectMusic(1);
            else if (toggle2.isOn) SelectMusic(2);
            else if (toggle3.isOn) SelectMusic(3);
        }
    }

    void SelectMusic(int index)
    {
        if (_ignore || musicSource == null) return;

        AudioClip clip = index switch
        {
            1 => music1,
            2 => music2,
            3 => music3,
            _ => null
        };

        if (clip == null) return;

        // Don't restart if it's already playing the same clip
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.Play();
    }
}
