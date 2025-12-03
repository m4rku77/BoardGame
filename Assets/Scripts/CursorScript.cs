using UnityEngine;

public class CursorScript : MonoBehaviour
{
    public Texture2D[] cursors; 
    void Start()
    {
        DefaultCursor();
    }
    
    public void DefaultCursor()
    {
        Cursor.SetCursor(cursors[0], Vector2.zero, CursorMode.Auto);
    }

    // Update is called once per frame
    public void OnButtonCursor()
    {
        Cursor.SetCursor(cursors[1], Vector2.zero, CursorMode.Auto);
    }

    public void ButtonClickedCursor()
    {
        Cursor.SetCursor(cursors[2], Vector2.zero, CursorMode.Auto);
    }

    public void OnPropCursor()
    {
        Cursor.SetCursor(cursors[3], Vector2.zero, CursorMode.Auto);
    }

    public void AttentCursor()
    {
        Cursor.SetCursor(cursors[4], Vector2.zero, CursorMode.Auto);
    }

}
