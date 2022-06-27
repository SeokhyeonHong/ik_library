using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    [Range(1, 100)]
    public int fontSize;
    [Range(0, 1)]
    public float red, green, blue;
    private float _deltaTime = 0.0f;
    void Start()
    {
        Application.targetFrameRate = 200;
        fontSize = 50;
    }

    // Update is called once per frame
    void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / fontSize;
        style.normal.textColor = new Color(red, green, blue, 1.0f);
        float msec = _deltaTime * 1000.0f;
        float fps = 1.0f / _deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.}fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
