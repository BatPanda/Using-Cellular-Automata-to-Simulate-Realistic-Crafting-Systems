using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class IngredientSelect : MonoBehaviour //A script for the button that lets you select new ingredients in the showcase
{
    [SerializeField] private TMP_Text button_name;
    private CAClickDrawer click_drawer_ref;
    private string ingredient_name;
    [SerializeField] private Image image;
    private IngredientPanelPopulator populator;
    private bool is_active = false;

    public void setup(CAClickDrawer _cd, string _name, string _display, IngredientPanelPopulator _populator) {
        click_drawer_ref = _cd;
        ingredient_name = _name;
        button_name.text = _display;
        populator = _populator;
    }

    public void deselct() {
        is_active = false;
        updateImage();
    }

    private void updateImage(){ 
        image.color = is_active ? new Color(0.6226415f,0.5078136f,0.4620861f) : Color.white;
    }

    public void selected() {
        click_drawer_ref.setBrushIngredient(ingredient_name);
        populator.deselectAll();
        is_active = true;
        updateImage();
    }
}
