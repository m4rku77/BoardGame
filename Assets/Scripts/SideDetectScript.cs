using UnityEngine;

public class SideDetectScript : MonoBehaviour
{
    private DiceRollScript diceRollScript;
    private Rigidbody rb;

    private void Awake()
    {
        diceRollScript = GetComponentInParent<DiceRollScript>();
        rb = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (diceRollScript == null || rb == null)
            return;

        // ✅ Only detect after the dice has been thrown at least once
        if (!diceRollScript.firstThrow)
            return;

        // ✅ Prevent re-detecting once already landed
        if (diceRollScript.isLanded)
            return;

        // Optional: only detect when touching floor
        if (!other.CompareTag("Floor"))
            return;

        // Dice must be basically stopped
        if (rb.linearVelocity.sqrMagnitude < 0.0005f &&
            rb.angularVelocity.sqrMagnitude < 0.0005f)
        {
            diceRollScript.isLanded = true;

            // ✅ If your side objects are named Side1..Side6
            diceRollScript.diceFaceNum = gameObject.name.Replace("Side", "");

            // If your side objects are named just "1".."6", use instead:
            // diceRollScript.diceFaceNum = gameObject.name;
        }
    }
}
