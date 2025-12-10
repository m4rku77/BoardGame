using UnityEngine;
using TMPro;
public class NameScript : MonoBehaviour
{
    TextMeshPro tMP;

    void Awake()
    {
        // This finds TextMeshPro/TMP_Text components in any child, even if inactive
        tMP = GetComponentInChildren<TextMeshPro>(true);

        if (tMP == null)
        {
            Debug.LogError("NameScript: No TextMeshPro component found in children of " + gameObject.name);
        }
    }


    public void SetName (string name)
    {
        tMP.text = name;
        tMP.color = new Color32((byte)Random.Range(0,256), (byte)Random.Range(0,255), (byte)Random.Range(0,255), 255);
    }
}
