using UnityEngine;

public class DiceRollScript : MonoBehaviour
{
    private Rigidbody rBody;
    private Vector3 startPosition;

    [SerializeField] private float maxRandForcVal = 10f;
    [SerializeField] private float startRollingForce = 1200f;

    [Header("Stuck Fix")]
    [SerializeField] private bool autoRerollIfStuck = true;
    [SerializeField] private float stuckTimeout = 2.5f;
    [SerializeField] private float stopVelThreshold = 0.03f;
    [SerializeField] private float stopAngThreshold = 0.03f;

    private float settleTimer = 0f;

    public string diceFaceNum = "";
    public bool isLanded = false;
    public bool firstThrow = false;

    private void Awake()
    {
        startPosition = transform.position;

        // hard reset so it can NEVER start as landed with an old value
        isLanded = false;
        firstThrow = false;
        diceFaceNum = "";

        Initialize();
    }

    private void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.isKinematic = true;

        // random start rotation
        transform.rotation = Random.rotation;

        settleTimer = 0f;
    }

    private void RollDice()
    {
        isLanded = false;
        diceFaceNum = "";
        settleTimer = 0f;

        rBody.isKinematic = false;

        float forceX = Random.Range(0, maxRandForcVal);
        float forceY = Random.Range(0, maxRandForcVal);
        float forceZ = Random.Range(0, maxRandForcVal);

        rBody.AddForce(Vector3.up * Random.Range(800f, startRollingForce));
        rBody.AddTorque(forceX, forceY, forceZ, ForceMode.Impulse);
    }

    public void ResetDice()
    {
        transform.position = startPosition;
        transform.rotation = Random.rotation;

        firstThrow = false;
        isLanded = false;
        diceFaceNum = "";

        Initialize();
    }

    // ✅ Call this from your UI Image/Button OnClick
    public void ForceReroll()
    {
        // keep firstThrow = true so your turn system still counts this as a real roll
        firstThrow = true;

        isLanded = false;
        diceFaceNum = "";
        settleTimer = 0f;

        // hard reset then roll again
        transform.position = startPosition;
        transform.rotation = Random.rotation;

        rBody.isKinematic = true;
        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;

        rBody.isKinematic = false;
        RollDice();
    }

    private void Update()
    {
        if (rBody == null) return;

        // Auto reroll if stuck on an edge and never lands properly
        if (autoRerollIfStuck && !rBody.isKinematic && !isLanded && firstThrow)
        {
            bool almostStopped =
                rBody.linearVelocity.sqrMagnitude < (stopVelThreshold * stopVelThreshold) &&
                rBody.angularVelocity.sqrMagnitude < (stopAngThreshold * stopAngThreshold);

            if (almostStopped) settleTimer += Time.deltaTime;
            else settleTimer = 0f;

            if (settleTimer >= stuckTimeout)
            {
                ForceReroll();
                return;
            }
        }

        // Click dice to roll (first roll or reroll after landing)
        if ((Input.GetMouseButtonDown(0) && isLanded) ||
            (Input.GetMouseButtonDown(0) && !firstThrow))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    firstThrow = true;
                    RollDice();
                }
            }
        }
    }
}
