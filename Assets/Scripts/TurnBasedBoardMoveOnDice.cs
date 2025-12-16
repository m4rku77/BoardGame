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

    private readonly List<Transform> waypoints = new List<Transform>();

    private int currentTurn = 0;
    private bool isMoving = false;
    private bool consumedThisLanding = false;

    private void Start()
    {
        BuildWaypointList();
    }

    private void Update()
    {
        if (dice == null || waypoints.Count == 0) return;
        if (PlayerRegistry.Players.Count == 0) return;

        // reset "consumed" when dice is not landed
        if (!dice.isLanded)
        {
            consumedThisLanding = false;
            return;
        }

        // only move once per landing + only after first throw
        if (dice.isLanded && dice.firstThrow && !consumedThisLanding && !isMoving)
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
        int currentIndex = FindClosestWaypointIndex(player.position);

        for (int i = 0; i < steps; i++)
        {
            if (currentIndex >= waypoints.Count - 1) break;

            currentIndex++;
            Transform target = waypoints[currentIndex];

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

            yield return new WaitForSeconds(0.05f);
        }

        isMoving = false;

        // ✅ next player's turn
        currentTurn = (currentTurn + 1) % PlayerRegistry.Players.Count;

        // optional: force player to roll again (prevents auto re-trigger)
        dice.ResetDice();
    }

    private int ParseDiceNumber(string s)
    {
        if (string.IsNullOrEmpty(s)) return 0;
        if (int.TryParse(s, out int val) && val > 0)
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
