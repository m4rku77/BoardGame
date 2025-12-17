using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TurnBasedBoardMoveOnDice : MonoBehaviour
{
    [Header("DEBUG")]
    [SerializeField] private bool enableDebugWinKey = true;
    [SerializeField] private KeyCode debugWinKey = KeyCode.F9;


    [Header("Win Screen")]
    [SerializeField] private GameObject winScreen;

    [SerializeField] private WinScreenUI winUI;
    [SerializeField] private LeaderboardManagerTXT leaderboardTXT;


    [Header("Game Finish / Leaderboard")]
    [SerializeField] private bool endGameWhenReachLastTile = true;
    [SerializeField] private int maxLeaderboardEntries = 10;

    private float gameStartTime;
    private int totalDiceThrows = 0;   // moves = dice throws
    private int totalTilesMoved = 0;   // optional
    private int baseScore = 0;         // if 0 we calculate automatically

    [Header("Camera Follow")]
    [SerializeField] private CameraFollow cameraFollow;

    [Header("References")]
    [SerializeField] private DiceRollScript dice;

    [Header("Waypoints")]
    [SerializeField] private string waypointPrefix = "P";
    [SerializeField] private int firstWaypointIndex = 2;   // P2 is index 0
    [SerializeField] private int lastWaypointIndex = 120;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private float stepPause = 0.05f;

    [Header("Face Camera")]
    [SerializeField] private bool faceCameraAlways = true;
    [SerializeField] private Camera faceCamera; // drag Main Camera here (or leave null)
    [SerializeField] private float faceCameraRotateSpeed = 720f;
    [SerializeField] private float faceCameraYawOffset = 180f; // fixes reversed model/name

    [System.Serializable]
    public class JumpLink
    {
        public int from; // tile number like 4
        public int to;   // tile number like 25
    }

    [Header("Jumps (good/bad tiles)")]
    [SerializeField]
    private List<JumpLink> jumps = new List<JumpLink>()
    {
        new JumpLink{ from = 4, to = 25 },
        new JumpLink{ from = 8, to = 28 },
        new JumpLink{ from = 15, to = 7 },
        new JumpLink{ from = 18, to = 3 },
        new JumpLink{ from = 22, to = 20 },
        new JumpLink{ from = 107, to = 23 },
        new JumpLink{ from = 26, to = 45 },
        new JumpLink{ from = 32, to = 10 },
        new JumpLink{ from = 34, to = 97 },
        new JumpLink{ from = 49, to = 70 },
        new JumpLink{ from = 64, to = 78 },
        new JumpLink{ from = 71, to = 69 },
        new JumpLink{ from = 83, to = 103 },
        new JumpLink{ from = 111, to = 91 },
        new JumpLink{ from = 93, to = 114 },
        new JumpLink{ from = 100, to = 82 },
        new JumpLink{ from = 105, to = 117 },
        new JumpLink{ from = 118, to = 102 },
    };

    [SerializeField] private float jumpSpeedMultiplier = 2.5f;

    private readonly List<Transform> waypoints = new List<Transform>();

    private readonly Dictionary<Transform, int> playerIndex = new Dictionary<Transform, int>(); // 0=P2
    private readonly Dictionary<Transform, bool> enteredBoard = new Dictionary<Transform, bool>();

    private int currentTurn = 0;
    private bool isMoving = false;
    private bool consumedThisLanding = false;
    private bool gameFinished = false;

    // ---------- Leaderboard JSON (saved to persistentDataPath) ----------
    [Serializable]
    public class Entry
    {
        public string name;
        public float timeSeconds;
        public int moves;
        public int score;
        public int tilesMoved;
        public string date;
    }
    [Serializable]
    public class EntryList
    {
        public List<Entry> entries = new List<Entry>();
    }

    private string savePath;

    private void Start()
    {


        if (winUI != null) winUI.gameObject.SetActive(false);


        gameStartTime = Time.time;
        savePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");

        BuildWaypointList();
        StartCoroutine(InitPlayersRoutine());

        if (faceCamera == null) faceCamera = Camera.main;
    }

    private IEnumerator InitPlayersRoutine()
    {
        while (PlayerRegistry.Players.Count == 0)
            yield return null;

        foreach (var p in PlayerRegistry.Players)
        {
            if (p == null) continue;
            enteredBoard[p] = false;
            playerIndex[p] = 0;
        }
    }

    private void Update()
    {
        if (enableDebugWinKey && Input.GetKeyDown(debugWinKey) && !gameFinished)
        {
            DebugForceWin();
            return;
        }


        if (gameFinished) return;
        if (dice == null || waypoints.Count == 0) return;
        if (PlayerRegistry.Players.Count == 0) return;
        if (isMoving) return;

        if (!dice.firstThrow) return;

        if (!dice.isLanded)
        {
            consumedThisLanding = false;
            return;
        }

        if (dice.isLanded && !consumedThisLanding)
        {
            consumedThisLanding = true;

            int steps = ParseDiceNumber(dice.diceFaceNum);

            // ✅ FIX: only start if steps > 0
            if (steps > 0)
            {
                totalDiceThrows++;
                StartCoroutine(MoveCurrentPlayerSteps(steps));
            }
        }
    }

    private void DebugForceWin()
    {
        Debug.Log("DEBUG: Force Finish Game");

        gameFinished = true;
        isMoving = false;

        // choose current player or first player safely
        Transform winner = null;
        if (PlayerRegistry.Players.Count > 0)
            winner = PlayerRegistry.Players[currentTurn];

        FinishGame(winner);
    }


    private IEnumerator MoveCurrentPlayerSteps(int steps)
    {
        isMoving = true;
        int rolledSteps = steps;

        Transform player = PlayerRegistry.Players[currentTurn];

        if (player == null)
        {
            isMoving = false;
            yield break;
        }

        if (cameraFollow != null)
            cameraFollow.FocusOn(player);

        if (!enteredBoard.ContainsKey(player))
        {
            enteredBoard[player] = false;
            playerIndex[player] = 0;
        }

        // Spawn -> P2 counts as 1 move
        if (!enteredBoard[player] && steps > 0)
        {
            yield return MoveToTarget(player, waypoints[0]); // P2
            enteredBoard[player] = true;
            playerIndex[player] = 0;
            steps -= 1;
            yield return new WaitForSeconds(stepPause);
        }

        // Normal steps
        yield return MoveWithBounceBack(player, steps);


        totalTilesMoved += rolledSteps;

        // Jump tiles
        if (enteredBoard[player])
        {
            int landedTile = IndexToTile(playerIndex[player]);

            if (TryGetJumpDestination(landedTile, out int destTile) && destTile != landedTile)
            {
                int fromIdx = playerIndex[player];
                int toIdx = Mathf.Clamp(TileToIndex(destTile), 0, waypoints.Count - 1);

                yield return MoveAlongBoardFast(player, fromIdx, toIdx);
                playerIndex[player] = toIdx;
            }
        }

        // ✅ WIN CHECK (after jump too)
        if (endGameWhenReachLastTile && enteredBoard[player])
        {
            int lastIndex = waypoints.Count - 1; // P120
            if (playerIndex[player] >= lastIndex)
            {
                FinishGame(player);
                if (cameraFollow != null) cameraFollow.ResetFocus();
                isMoving = false;
                gameFinished = true;
                yield break;
            }
        }

        if (cameraFollow != null)
            cameraFollow.ResetFocus();

        isMoving = false;

        // next player's turn
        currentTurn = (currentTurn + 1) % PlayerRegistry.Players.Count;

        dice.ResetDice();
        consumedThisLanding = false;
    }

    private IEnumerator MoveAlongBoardFast(Transform player, int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) yield break;

        int dir = (toIndex > fromIndex) ? 1 : -1;
        int i = fromIndex;

        while (i != toIndex)
        {
            i += dir;
            yield return MoveToTarget(player, waypoints[i], jumpSpeedMultiplier);
            yield return new WaitForSeconds(stepPause);
        }
    }

    private IEnumerator MoveToTarget(Transform player, Transform target, float speedMult = 1f)
    {
        SetIdleWalk(player, true);

        while (Vector3.Distance(player.position, target.position) > arriveDistance)
        {
            player.position = Vector3.MoveTowards(
                player.position,
                target.position,
                (moveSpeed * speedMult) * Time.deltaTime
            );

            if (faceCameraAlways)
            {
                if (faceCamera == null) faceCamera = Camera.main;
                if (faceCamera != null)
                {
                    Vector3 toCam = faceCamera.transform.position - player.position;
                    toCam.y = 0f;

                    if (toCam.sqrMagnitude > 0.001f)
                    {
                        Quaternion look =
                            Quaternion.LookRotation(toCam.normalized, Vector3.up) *
                            Quaternion.Euler(0f, faceCameraYawOffset, 0f);

                        player.rotation = Quaternion.RotateTowards(
                            player.rotation,
                            look,
                            faceCameraRotateSpeed * Time.deltaTime
                        );
                    }
                }
            }

            yield return null;
        }

        SetIdleWalk(player, false);
    }

    private Animator GetAnimator(Transform player)
    {
        var anim = player.GetComponent<Animator>();
        if (anim != null) return anim;
        return player.GetComponentInChildren<Animator>();
    }

    private void SetIdleWalk(Transform player, bool walking)
    {
        var anim = GetAnimator(player);
        if (anim == null) return;
        anim.SetBool("walk", walking);
        anim.SetBool("idle", !walking);
    }

    private bool TryGetJumpDestination(int tileNumber, out int destTileNumber)
    {
        for (int i = 0; i < jumps.Count; i++)
        {
            if (jumps[i].from == tileNumber)
            {
                destTileNumber = jumps[i].to;
                return true;
            }
        }
        destTileNumber = tileNumber;
        return false;
    }

    private int TileToIndex(int tileNumber) => tileNumber - firstWaypointIndex; // tile 2 -> 0
    private int IndexToTile(int index) => index + firstWaypointIndex;

    private int ParseDiceNumber(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        if (int.TryParse(s, out int val)) return Mathf.Clamp(val, 1, 6);
        return 0;
    }

    private void BuildWaypointList()
    {
        waypoints.Clear();
        for (int i = firstWaypointIndex; i <= lastWaypointIndex; i++)
        {
            GameObject go = GameObject.Find(waypointPrefix + i);
            if (go != null) waypoints.Add(go.transform);
            else Debug.LogWarning("Missing waypoint: " + waypointPrefix + i);
        }
    }
    private void FinishGame(Transform winner)
    {
        float timeSeconds = Time.time - gameStartTime;

        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        int moves = totalDiceThrows;

        int finalScore = baseScore;
        if (finalScore == 0)
            finalScore = Mathf.Max(0, 10000 - Mathf.RoundToInt(timeSeconds * 10f) - moves * 50);

        SaveToLeaderboardFile(playerName, timeSeconds, moves, finalScore, totalTilesMoved);

        // ✅ ALWAYS enable the root panel first (WinScreen in your hierarchy)
        if (winScreen != null)
        {
            winScreen.SetActive(true);
            winScreen.transform.SetAsLastSibling();
        }

        // ✅ Then update/enable the WinUI component (child)
        if (winUI != null)
        {
            winUI.gameObject.SetActive(true);
            winUI.Show(timeSeconds, moves, finalScore);
            winUI.transform.SetAsLastSibling();
        }

        // ✅ Force UI to rebuild this frame
        Canvas.ForceUpdateCanvases();

        // ✅ Freeze after the frame is rendered
        StartCoroutine(FreezeAfterRendered());
    }



    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SampleScene");
    }


    private void SaveToLeaderboardFile(string playerName, float timeSeconds, int moves, int score, int tilesMoved)
    {
        EntryList data = LoadLeaderboardFile();

        data.entries.Add(new Entry
        {
            name = playerName,
            timeSeconds = timeSeconds,
            moves = moves,
            score = score,
            tilesMoved = tilesMoved,
            date = DateTime.Now.ToString("yyyy-MM-dd")
        });

        // Sort: score desc, time asc, moves asc
        data.entries.Sort((a, b) =>
        {
            int s = b.score.CompareTo(a.score);
            if (s != 0) return s;

            int t = a.timeSeconds.CompareTo(b.timeSeconds);
            if (t != 0) return t;

            return a.moves.CompareTo(b.moves);
        });

        if (data.entries.Count > maxLeaderboardEntries)
            data.entries.RemoveRange(maxLeaderboardEntries, data.entries.Count - maxLeaderboardEntries);

        try
        {
            File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to save leaderboard: " + ex.Message);
        }
    }

    private IEnumerator MoveWithBounceBack(Transform player, int steps)
    {
        int currentIdx = playerIndex[player];
        int lastIdx = waypoints.Count - 1;

        int targetIdx = currentIdx + steps;

        // CASE 1: normal move (no overflow)
        if (targetIdx <= lastIdx)
        {
            for (int i = 0; i < steps; i++)
            {
                currentIdx++;
                playerIndex[player] = currentIdx;
                yield return MoveToTarget(player, waypoints[currentIdx]);
                yield return new WaitForSeconds(stepPause);
            }
            yield break;
        }

        // CASE 2: overflow → bounce back
        int stepsToEnd = lastIdx - currentIdx;
        int overflow = targetIdx - lastIdx;

        // move forward to last tile
        for (int i = 0; i < stepsToEnd; i++)
        {
            currentIdx++;
            playerIndex[player] = currentIdx;
            yield return MoveToTarget(player, waypoints[currentIdx]);
            yield return new WaitForSeconds(stepPause);
        }

        // bounce backwards
        for (int i = 0; i < overflow; i++)
        {
            currentIdx--;
            playerIndex[player] = currentIdx;
            yield return MoveToTarget(player, waypoints[currentIdx]);
            yield return new WaitForSeconds(stepPause);
        }
    }
    private IEnumerator FreezeAfterRendered()
    {
        yield return new WaitForEndOfFrame();  // ensures it actually renders
        Time.timeScale = 0f;
    }


    private EntryList LoadLeaderboardFile()
    {
        try
        {
            if (!File.Exists(savePath))
                return new EntryList();

            string json = File.ReadAllText(savePath);
            var data = JsonUtility.FromJson<EntryList>(json);
            return data ?? new EntryList();
        }
        catch
        {
            return new EntryList();
        }
    }
}
