using System.Collections;
using System.Collections.Generic;
using DissertationTool.Assets.Scripts.TestLanguage;
using UnityEngine;

public class runTestLanguage : MonoBehaviour
{
    [SerializeField] private string input;
    void Start()
    {
        interpreter interp = new interpreter(input);
        Debug.Log( interp.isTypeable() ? "Language Output: " + (interp.evaluate()).ToString() : "Untypeable expression detected!");
    }
}

//A simple script used to run the test language, feel free to throw it on an object and have a play!