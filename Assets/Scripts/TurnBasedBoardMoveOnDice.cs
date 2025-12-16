using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedBoardMoveOnDice : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DiceRollScript dice;

    [Header("Waypoints")]
    [SerializeField] private string waypointPrefix = "P";
    [SerializeField] private int firstWaypointIndex = 2;
    [SerializeField] private int lastWaypointIndex = 120;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float rotateSpeed = 720f;
    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private float stepPause = 0.05f;

    private readonly List<Transform> waypoints = new List<Transform>();

    // ✅ Each player keeps their own board position
    private readonly Dictionary<Transform, int> playerIndex = new Dictionary<Transform, int>();

    private int currentTurn = 0;
    private bool isMoving = false;
    private bool consumedThisLanding = false;

    private void Start()
    {
        BuildWaypointList();
        StartCoroutine(InitPlayersRoutine());
    }

    private IEnumerator InitPlayersRoutine()
    {
        // wait until players are spawned & registered
        while (PlayerRegistry.Players.Count == 0)
            yield return null;

        // init each player's current tile (closest)
        foreach (var p in PlayerRegistry.Players)
        {
            if (p == null) continue;
            playerIndex[p] = FindClosestWaypointIndex(p.position);
        }

        // optional: print whose turn it is
        Debug.Log($"Turn 1: {PlayerRegistry.Players[currentTurn].name}");
    }

    private void Update()
    {
        if (dice == null || waypoints.Count == 0) return;
        if (PlayerRegistry.Players.Count == 0) return;

        // ✅ do not move unless user has rolled at least once
        if (!dice.firstThrow) return;

        // reset landing consumption when dice not landed
        if (!dice.isLanded)
        {
            consumedThisLanding = false;
            return;
        }

        // consume landing once
        if (dice.isLanded && !consumedThisLanding && !isMoving)
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

        if (!playerIndex.ContainsKey(player))
            playerIndex[player] = FindClosestWaypointIndex(player.position);

        for (int i = 0; i < steps; i++)
        {
            int idx = playerIndex[player];
            if (idx >= waypoints.Count - 1) break;

            idx++;
            playerIndex[player] = idx;

            Transform target = waypoints[idx];

            while (Vector3.Distance(player.position, target.position) > arriveDistance)
            {
                player.position = Vector3.MoveTowards(player.position, target.position, moveSpeed * Time.deltaTime);

                Vector3 dir = target.position - player.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion lookRot = Quaternion.LookRotation(dir);
                    player.rotation = Quaternion.RotateTowards(player.rotation, lookRot, rotateSpeed * Time.deltaTime);
                }

                yield return null;
            }

            yield return new WaitForSeconds(stepPause);
        }

        isMoving = false;

        // ✅ next turn
        currentTurn = (currentTurn + 1) % PlayerRegistry.Players.Count;
        Debug.Log($"Next turn: {PlayerRegistry.Players[currentTurn].name}");

        // ✅ prepare for next roll (VERY IMPORTANT)
        dice.ResetDice();
        consumedThisLanding = false;
    }

    private int ParseDiceNumber(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;

        // if your sides are named "Side1" etc, SideDetect already strips it.
        if (int.TryParse(s, out int val))
            return Mathf.Clamp(val, 1, 6);

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

    private int FindClosestWaypointIndex(Vector3 pos)
    {
        int best = 0;
        float bestDist = float.MaxValue;

        for (int i = 0; i < waypoints.Count; i++)
        {
            float d = Vector3.Distance(pos, waypoints[i].position);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }
}
