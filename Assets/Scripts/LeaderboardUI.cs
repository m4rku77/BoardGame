using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private LeaderboardManagerTXT manager;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject rowPrefab;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        // Clear old rows
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        var entries = manager.LoadEntries();
        Debug.Log("Entries loaded: " + entries.Count);

        foreach (var e in entries)
        {
            var row = Instantiate(rowPrefab, content);

            row.transform.Find("NameText").GetComponent<TMPro.TMP_Text>().text = e.name;
            row.transform.Find("TimeText").GetComponent<TMPro.TMP_Text>().text = FormatTime(e.timeSeconds);
            row.transform.Find("MovesText").GetComponent<TMPro.TMP_Text>().text = e.moves.ToString();
            row.transform.Find("ScoreText").GetComponent<TMPro.TMP_Text>().text = e.score.ToString();
        }
    }

    private string FormatTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60f);
        int s = Mathf.FloorToInt(seconds % 60f);
        return $"{m:00}:{s:00}";
    }
}

