using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class CellOverrideWidgetScript : MonoBehaviour //A script for the old prototype that use to handle spawning cells automatically.
{
    [SerializeField] private CellGridController cg_controller;
    [SerializeField] private TMP_InputField type_name;
    [SerializeField] private TMP_InputField x_pos;
    [SerializeField] private TMP_InputField y_pos;

    [SerializeField] private TMP_InputField auto_place_rate;
    [SerializeField] private Toggle should_auto;

    void Start() {
        StartCoroutine(doAutoPlace());
    }

    public IEnumerator doAutoPlace() {
        while (true) {
            if (should_auto.isOn) {
                overrideCell();
            }
            yield return new WaitForSeconds((float)Convert.ToDouble(auto_place_rate.text));
        }
    }

    public void overrideCell() {
        if (type_name.text == "" || x_pos.text == "" || y_pos.text == "" ) {return;}
        cg_controller.setCell(new Vector2Int(Convert.ToInt32(x_pos.text),Convert.ToInt32(y_pos.text)),type_name.text);
    }

    public void overrideThisCell(int _x, int _y) {
        if (type_name.text == "") {return;}
        cg_controller.setCell(new Vector2Int(_x,_y),type_name.text);
    }
}
