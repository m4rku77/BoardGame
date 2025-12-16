using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedBoardMoveOnDice : MonoBehaviour
{
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

    [SerializeField] private float jumpSpeedMultiplier = 2.5f; // faster travel for jumps

    private readonly List<Transform> waypoints = new List<Transform>();

    // Board index: 0 = P2, 1 = P3, ...
    private readonly Dictionary<Transform, int> playerIndex = new Dictionary<Transform, int>();
    private readonly Dictionary<Transform, bool> enteredBoard = new Dictionary<Transform, bool>();

    private int currentTurn = 0;
    private bool isMoving = false;
    private bool consumedThisLanding = false;

    private void Start()
    {
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

            // Start OFF the board at spawnpoint (no snapping)
            enteredBoard[p] = false;
            playerIndex[p] = 0; // first board tile is P2 (index 0)
        }
    }

    private void Update()
    {
        if (dice == null || waypoints.Count == 0) return;
        if (PlayerRegistry.Players.Count == 0) return;
        if (isMoving) return;

        // don't move until a real roll happened
        if (!dice.firstThrow) return;

        // reset consumed when dice not landed
        if (!dice.isLanded)
        {
            consumedThisLanding = false;
            return;
        }

        // consume landing once
        if (dice.isLanded && !consumedThisLanding)
        {
            consumedThisLanding = true;

            int steps = ParseDiceNumber(dice.diceFaceNum);
            if (steps > 0)
                StartCoroutine(MoveCurrentPlayerSteps(steps));
        }
    }

    private IEnumerator MoveCurrentPlayerSteps(int steps)
    {
        isMoving = true;

        Transform player = PlayerRegistry.Players[currentTurn];
        if (player == null)
        {
            isMoving = false;
            yield break;
        }

        if (!enteredBoard.ContainsKey(player))
        {
            enteredBoard[player] = false;
            playerIndex[player] = 0;
        }

        // 1) Spawn -> P2 counts as 1 move
        if (!enteredBoard[player] && steps > 0)
        {
            yield return MoveToTarget(player, waypoints[0]); // P2
            enteredBoard[player] = true;
            playerIndex[player] = 0;
            steps -= 1;
            yield return new WaitForSeconds(stepPause);
        }

        // 2) Normal dice steps along the board
        for (int i = 0; i < steps; i++)
        {
            int idx = playerIndex[player];
            if (idx >= waypoints.Count - 1) break;

            idx++;
            playerIndex[player] = idx;

            yield return MoveToTarget(player, waypoints[idx]);
            yield return new WaitForSeconds(stepPause);
        }

        // 3) Jump tiles (snakes/ladders) AFTER movement ends
        if (enteredBoard[player])
        {
            int landedTile = IndexToTile(playerIndex[player]);

            if (TryGetJumpDestination(landedTile, out int destTile) && destTile != landedTile)
            {
                int fromIdx = playerIndex[player];
                int toIdx = Mathf.Clamp(TileToIndex(destTile), 0, waypoints.Count - 1);

                // fastest path on a linear board = walk forward/backward
                yield return MoveAlongBoardFast(player, fromIdx, toIdx);

                playerIndex[player] = toIdx;
            }
        }

        isMoving = false;

        // next player's turn
        currentTurn = (currentTurn + 1) % PlayerRegistry.Players.Count;

        // prepare for next roll
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
        while (Vector3.Distance(player.position, target.position) > arriveDistance)
        {
            player.position = Vector3.MoveTowards(
                player.position,
                target.position,
                (moveSpeed * speedMult) * Time.deltaTime
            );

            // Always face camera so name is visible
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

    // Converts tile number (like 4) to waypoint list index (0 = P2)
    private int TileToIndex(int tileNumber) => tileNumber - firstWaypointIndex;

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
}
