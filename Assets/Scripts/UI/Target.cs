using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private bool _selected = false;
    public bool selected
    {
        get { return _selected; }
    }
    private Drag _drag;
    
    void Start()
    {
        
    }

    void Update()
    {
        SelectTarget();
        ActivateChildren();
    }

    void SelectTarget()
    {
        if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);
            if(hits.Length > 0)
            {
                GameObject hitObj = hits[0].transform.gameObject;
                if(hitObj == this.gameObject || hitObj.transform.IsChildOf(this.transform))
                {
                    _selected = true;
                }
                else
                {
                    _selected = false;
                }
            }
            else
            {
                _selected = false;
            }
		}
    }

    void ActivateChildren()
    {
        for(int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(_selected);
        }
    }
}