using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Cursor : MonoBehaviour
{
    public Texture2D CursorArrow;
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.SetCursor(CursorArrow, Vector2.zero, CursorMode.ForceSoftware);
    }

}
