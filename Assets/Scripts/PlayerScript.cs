using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public GameObject spawnPoint;

    private int characterIndex;
    private int index;

    private const string textFileName = "PlayerNames";

    void Start()
    {
        // ✅ Clear registry each time scene starts
        PlayerRegistry.Clear();

        // ✅ Spawn main player
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);

        GameObject mainCharacter = Instantiate(
            playerPrefabs[characterIndex],
            spawnPoint.transform.position,
            Quaternion.identity
        );

        mainCharacter.GetComponent<NameScript>().SetName(
            PlayerPrefs.GetString("PlayerName", "Džonijs Dūū")
        );

        // ✅ Register main player for turn system
        PlayerRegistry.Register(mainCharacter.transform);

        // ✅ Spawn other players
        int playerCount = PlayerPrefs.GetInt("PlayerCount", 2); // default 2 if missing
        string[] nameArray = ReadLinesFromFile(textFileName);

        for (int i = 0; i < playerCount - 1; i++)
        {
            spawnPoint.transform.position += new Vector3(0.2f, 0, 0.08f);

            index = Random.Range(0, playerPrefabs.Length);

            GameObject otherPlayer = Instantiate(
                playerPrefabs[index],
                spawnPoint.transform.position,
                Quaternion.identity
            );

            if (nameArray.Length > 0)
            {
                otherPlayer.GetComponent<NameScript>().SetName(
                    nameArray[Random.Range(0, nameArray.Length)]
                );
            }
            else
            {
                otherPlayer.GetComponent<NameScript>().SetName("Player " + (i + 2));
            }

            // ✅ Register other player for turn system
            PlayerRegistry.Register(otherPlayer.transform);
        }
    }

    string[] ReadLinesFromFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if (textAsset != null)
        {
            return textAsset.text.Split(new[] { '\r', '\n' },
                System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Debug.LogWarning("File not found: " + fileName);
            return new string[0];
        }
    }
}
