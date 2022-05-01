using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientPanelPopulator : MonoBehaviour //A script that generates the ingredient select buttons later.
{

    [SerializeField] private GameObject ingredient_panel;
    [SerializeField] private NewCellGridController cell_grid_controller;
    [SerializeField] private CAClickDrawer click_drawer;
    [SerializeField] private Transform content_area;

    private List<GameObject> panels;


    // Start is called before the first frame update
    void Start()
    {
        panels = new List<GameObject>();
        cell_grid_controller.getInteractionConfig().getIngredients().ForEach(_e => {
            if (_e.getSelectable()) {
                GameObject panel = Instantiate(ingredient_panel);
                panel.transform.SetParent(content_area,false);
                panel.GetComponent<IngredientSelect>().setup(click_drawer,_e.getIngredientName(),_e.getIngredientDisplayName(),this);
                panels.Add(panel);
            }
        });
    }

    public void deselectAll() {
        panels.ForEach(_e => _e.GetComponent<IngredientSelect>().deselct());
    }
}
