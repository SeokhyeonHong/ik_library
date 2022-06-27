using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{
    public Vector3 move;
    private Color _defaultColor;
    private bool _dragable = false;
    public bool dragable
    {
        get { return _dragable; }
        set { _dragable = dragable; }
    }

    private Vector3 _prevSpace;
    private Vector3 _screenSpace;
    void Awake()
    {
        _defaultColor = GetComponent<Renderer>().material.GetColor("_Color");
        move.Normalize();
    }

    void OnMouseDown()
    {
        _screenSpace = Camera.main.WorldToScreenPoint(transform.parent.position);
        var prevScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenSpace.z);
		_prevSpace = Camera.main.ScreenToWorldPoint(prevScreenSpace);

        GetComponent<Renderer>().material.SetColor("_Color", Color.white);
    }

    void OnMouseDrag()
    {
        var currScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _screenSpace.z);
        var currPosition = Camera.main.ScreenToWorldPoint(currScreenSpace) ;

        Vector3 dirVector = currPosition - _prevSpace;
        dirVector = Vector3.Scale(dirVector, move);
        transform.parent.position += dirVector;
        _prevSpace = currPosition;
    }

    void OnMouseUp()
    {
        GetComponent<Renderer>().material.SetColor("_Color", _defaultColor);
    }
}