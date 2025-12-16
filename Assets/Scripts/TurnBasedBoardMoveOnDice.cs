using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnBasedBoardMoveOnDice : MonoBehaviour
{

    [SerializeField] private bool faceCameraAlways = true;
    [SerializeField] private Camera faceCamera; // drag Main Camera here (or leave null)
    [SerializeField] private float faceCameraRotateSpeed = 720f;


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

    // Board index: 0 = P2, 1 = P3, ...
    // If player is not on board yet: enteredBoard[player] = false
    private readonly Dictionary<Transform, int> playerIndex = new Dictionary<Transform, int>();
    private readonly Dictionary<Transform, bool> enteredBoard = new Dictionary<Transform, bool>();

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
        while (PlayerRegistry.Players.Count == 0)
            yield return null;

        foreach (var p in PlayerRegistry.Players)
        {
            if (p == null) continue;

            // ✅ Start OFF the board at spawnpoint (no snapping)
            enteredBoard[p] = false;
            playerIndex[p] = 0; // their first board tile will be P2 (index 0)
        }
    }

    private void Update()
    {
        if (dice == null || waypoints.Count == 0) return;
        if (PlayerRegistry.Players.Count == 0) return;
        if (isMoving) return;

        // don’t move until a real roll happened
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

        // ✅ If player is still at spawn, first step is Spawn -> P2
        if (!enteredBoard[player] && steps > 0)
        {
            yield return MoveToTarget(player, waypoints[0]); // P2
            enteredBoard[player] = true;
            playerIndex[player] = 0; // now standing on P2
            steps -= 1;              // ✅ consume 1 move
            yield return new WaitForSeconds(stepPause);
        }

        // Now move remaining steps on the board
        for (int i = 0; i < steps; i++)
        {
            int idx = playerIndex[player];
            if (idx >= waypoints.Count - 1) break;

            idx++;
            playerIndex[player] = idx;

            yield return MoveToTarget(player, waypoints[idx]);
            yield return new WaitForSeconds(stepPause);
        }

        isMoving = false;

        // ✅ next player's turn
        currentTurn = (currentTurn + 1) % PlayerRegistry.Players.Count;

        // ✅ prepare for next roll
        dice.ResetDice();
        consumedThisLanding = false;
    }

    private IEnumerator MoveToTarget(Transform player, Transform target)
    {
        while (Vector3.Distance(player.position, target.position) > arriveDistance)
        {
            player.position = Vector3.MoveTowards(player.position, target.position, moveSpeed * Time.deltaTime);

            // Replace the whole dir/LookRotation rotation block with this:
            if (faceCameraAlways)
            {
                if (faceCamera == null) faceCamera = Camera.main;

                if (faceCamera != null)
                {
                    Vector3 toCam = faceCamera.transform.position - player.position;
                    toCam.y = 0f;

                    if (toCam.sqrMagnitude > 0.001f)
                    {
                        Quaternion look = Quaternion.LookRotation(toCam.normalized, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);

                        player.rotation = Quaternion.RotateTowards(player.rotation, look, faceCameraRotateSpeed * Time.deltaTime);
                    }
                }
            }

            yield return null;
        }
    }

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
