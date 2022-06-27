using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IKOption : MonoBehaviour
{
    private GameObject _character;
    private Dropdown _dropdown;
    List<string> options = new List<string>() {"None", "Two Bone", "CCD", "Jacobian Transpose", "Jacobian Pseudoinverse", "Jacobian DLS" };

    void Awake()
    {
        _character = GameObject.Find("Character");
        _dropdown = GetComponent<Dropdown>();
        _dropdown.AddOptions(options);
    }

    void Update()
    {
        _character.GetComponent<IKSolver>().ikSolver = options[_dropdown.value];
    }
}