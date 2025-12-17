using TMPro;
using UnityEngine;

public class WinScreenUI : MonoBehaviour
{
    [Header("Assign these Text (TMP) objects")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text scoreText;

    public void Show(float timeSeconds, int moves, int score)
    {
        // format time as mm:ss
        int mins = Mathf.FloorToInt(timeSeconds / 60f);
        int secs = Mathf.FloorToInt(timeSeconds % 60f);

        if (timeText != null) timeText.text = $"Time: {mins:00}:{secs:00}";
        if (movesText != null) movesText.text = $"Moves: {moves}";
        if (scoreText != null) scoreText.text = $"Score: {score}";

        gameObject.SetActive(true);
    }
}
