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

        // Optional: only detect when touching floor
        if (!other.CompareTag("Floor"))
            return;

        // Dice must be basically stopped
        if (rb.linearVelocity.sqrMagnitude < 0.0005f &&
            rb.angularVelocity.sqrMagnitude < 0.0005f)
        {
            diceRollScript.isLanded = true;

            // ✅ THIS is the important fix
            // If your side objects are named "1", "2", "3", etc.
            diceRollScript.diceFaceNum = gameObject.name;

            // If named "Side1", "Side2", etc. use:
            // diceRollScript.diceFaceNum = gameObject.name.Replace("Side", "");
        }
    }
}
