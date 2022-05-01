using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CellData : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler //The old cell data script used in the old system. 
{


    public GameObject setupCell(int _new_x, int _new_y, float _new_size, Ingredient _start_ingredient, CellOverrideWidgetScript _cow, CellGridController _cgc, System.Random _rnd) {
        rand = _rnd;
        grid_x_pos = _new_x; grid_y_pos = _new_y; cell_size = _new_size;
        gameObject.GetComponent<Transform>().localScale = new Vector3(cell_size/100,cell_size/100,1);
        cow = _cow;
        cgc = _cgc;
        setNewState(_start_ingredient,true);
        if (debugCell) {cell_text.text = $"X={_new_x}:Y={_new_y}";}
        return gameObject;
    }

    public int grid_x_pos {get; private set;} = -1;
    public int grid_y_pos {get; private set;} = -1;
    private float cell_size = 0;
    private System.Random rand;

    private CellOverrideWidgetScript cow;
    private CellGridController cgc;


    [SerializeField] private bool debugCell = false;
    [SerializeField] private Image cell_image;

    [SerializeField] private TMP_Text cell_text;

    [SerializeField] private Ingredient ingredient; 

    public Ingredient getIngredient() => ingredient;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void setNewState(Ingredient _new_ingredient, bool _instant = false) {
        ingredient = _new_ingredient;
        Color new_colour = ingredient.getColours().getRandomColourFromList(rand);
        if (_instant) {cell_image.color = new_colour;} else
        {StartCoroutine(changeColour(cgc.getFadeTime(),new_colour));}
    }

    

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        if (cow != null) {
            cow.overrideThisCell(grid_x_pos,grid_y_pos);
        }
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (Input.GetMouseButton(0)) {
            if (cow != null) {
                cow.overrideThisCell(grid_x_pos,grid_y_pos);
            }
        }
    }

    private IEnumerator changeColour(float duration, Color new_col) {
        float e_time = 0f;
        Color current_colour = cell_image.color;
        while (e_time < duration) {
            Color new_c = Color.Lerp(current_colour,new_col,(e_time/duration));
            cell_image.color = new_c;
            e_time+=Time.deltaTime;
            yield return null;
        }
        cell_image.color = new_col;
    }
}
