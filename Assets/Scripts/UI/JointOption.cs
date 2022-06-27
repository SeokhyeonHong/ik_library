using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JointOption : MonoBehaviour
{
    protected List<string> _jointNames;
    protected List<Transform> _joints;
    protected Dropdown _dropdown;
    protected GameObject _character;
    protected void Awake()
    {
        _jointNames = new List<string>();
        _joints = new List<Transform>();

        _character = GameObject.Find("Character");
        AddJointName(_character.transform);

        _dropdown = GetComponent<Dropdown>();
        _dropdown.AddOptions(_jointNames);
    }

    void AddJointName(Transform t, int depth = 0)
    {
        for(int i = 0; i < t.childCount; ++i)
        {
            Transform child = t.GetChild(i);
            string childName = child.name;
            if(childName.Contains("mixamorig:"))
            {
                childName = childName.Substring(childName.IndexOf(":") + 1);
                string blank = "";
                for(int d = 0; d < depth; ++d)
                {
                    blank += "   ";
                }
                _jointNames.Add(blank + childName);
                _joints.Add(child);
            }
            AddJointName(child, depth + 1);
        }
    }
}