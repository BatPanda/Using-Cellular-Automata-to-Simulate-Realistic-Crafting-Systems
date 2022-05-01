using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BrushSizeButton : MonoBehaviour // A script used to control the burhs size of the CA clicker drawer.
{
    [SerializeField] private Image sizeSquare;
    [SerializeField] private TMP_Text sizeIndicator;
    [SerializeField] private CAClickDrawer clickDrawer;
    public int max_size = 3;
    int size = 1;

    // Start is called before the first frame update
    void Start()
    {
        updateDrawer();
    }

    public void onSelected() {
        size += Input.GetKey(KeyCode.LeftShift) ? -1 : 1;
        if (size > max_size) {size = 1;}
        else if (size == 0) {size = max_size;}
        updateDrawer();
    }

    private void updateDrawer() {
        clickDrawer.setBrushSize(size);
        float scale = (float)size/(float)max_size;
        sizeSquare.transform.localScale = new Vector3(scale,scale,1);
        sizeIndicator.text = size.ToString();
    }
}
