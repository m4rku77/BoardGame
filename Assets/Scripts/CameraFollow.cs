using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Default (Idle) Camera State")]
    public Vector3 defaultPosition = new Vector3(1f, 6f, 3f);
    public Vector3 defaultRotation = new Vector3(60f, 0f, 0f);

    [Header("Focus (While Moving)")]
    public Transform target;
    public Vector3 focusOffset = new Vector3(0f, 4.5f, 2f); // closer
    public float followSpeed = 6f;

    private bool focusing = false;

    private void Start()
    {
        // ensure camera starts EXACTLY like your screenshot
        transform.position = defaultPosition;
        transform.rotation = Quaternion.Euler(defaultRotation);
    }

    private void LateUpdate()
    {
        if (!focusing || target == null)
        {
            // return to original position & rotation
            transform.position = Vector3.Lerp(
                transform.position,
                defaultPosition,
                followSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(defaultRotation),
                followSpeed * Time.deltaTime
            );
            return;
        }

        // focus mode (follow active player)
        Vector3 desiredPos = target.position + focusOffset;
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            followSpeed * Time.deltaTime
        );
       
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.Euler(defaultRotation),
            followSpeed * Time.deltaTime
        );


    }

    // called when player starts moving
    public void FocusOn(Transform t)
    {
        target = t;
        focusing = true;
    }

    // called when movement ends
    public void ResetFocus()
    {
        focusing = false;
        target = null;
    }
}
