using System.Collections.Generic;
using UnityEngine;

public static class PlayerRegistry
{
    private static readonly List<Transform> players = new List<Transform>();

    public static IReadOnlyList<Transform> Players => players;

    public static void Clear() => players.Clear();

    public static void Register(Transform t)
    {
        if (t != null && !players.Contains(t))
            players.Add(t);
    }
}
