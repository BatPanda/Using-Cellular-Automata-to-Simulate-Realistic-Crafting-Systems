using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;


[System.Serializable]
public class DictSFUnityEvent : UnityEvent<Dictionary<string,float>> //A cheaty way to make a dictionary seralizable for the editor.
{
    public Dictionary<string,float> dict_param;
}