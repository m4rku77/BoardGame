using UnityEngine;
using System.Reflection;

public class AutoWinTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("AUTO WIN TEST START");

        var game = FindFirstObjectByType<TurnBasedBoardMoveOnDice>();

        if (game == null)
        {
            Debug.LogError("TurnBasedBoardMoveOnDice NOT FOUND");
            return;
        }

        Debug.Log("Found TurnBasedBoardMoveOnDice on: " + game.gameObject.name);

        // Grab the private method: FinishGame(Transform)
        MethodInfo m = typeof(TurnBasedBoardMoveOnDice).GetMethod(
            "FinishGame",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (m == null)
        {
            Debug.LogError("FinishGame method NOT FOUND (name/signature mismatch).");
            return;
        }

        // Use any transform as winner (just for testing WinScreen)
        Transform fakeWinner = game.transform;

        Debug.Log("Calling FinishGame via reflection now...");
        m.Invoke(game, new object[] { fakeWinner });
    }
}
