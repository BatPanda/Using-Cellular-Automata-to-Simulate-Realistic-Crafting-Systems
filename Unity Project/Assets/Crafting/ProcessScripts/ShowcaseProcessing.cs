using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowcaseProcessing : MonoBehaviour
{
    [SerializeField] private TMP_Text processed_title;
    [SerializeField] private TMP_Text processed_description;
    [SerializeField] private CanvasGroup group;

    [SerializeField] private NewCellGridController cellGrid;

    public void Process(Dictionary<string,float> _data) {
        //string title = "test";
        string description = "";
        string prefix = "";   
        string suffix = "";        

        float poison_flag = _data.ContainsKey("poison") ? _data["poison"] : 0;
        float spice_flag = _data.ContainsKey("spice") ? _data["spice"] : 0;
        float wet_flag = _data.ContainsKey("wet") ? _data["wet"] : 0;
        float empty_flag = _data.ContainsKey("empty") ? _data["empty"] : 0;
        float glow_flag = _data.ContainsKey("glow") ? _data["glow"] : 0;
        float healing_flag = _data.ContainsKey("healing") ? _data["healing"] : 0;
        float blinding_flag = _data.ContainsKey("blinding") ? _data["blinding"] : 0;
        float nausa_flag = _data.ContainsKey("nausa") ? _data["nausa"] : 0;
        float potion_flag = _data.ContainsKey("potion") ? _data["potion"] : 0;


        Vector2Int size = cellGrid.getCellLengthAndHeight();
        int max_cells = size.x*size.y;

        if (spice_flag == max_cells) {
            prefix = "Block of ";
            suffix = "Salt";
        } else if (spice_flag == max_cells*-1) {
            prefix = "Block of ";
            suffix = "Honey";
        } else if (max_cells == empty_flag) {
            prefix = "Empty ";
            suffix = "Void";
        }
        else if (potion_flag != 0 && potion_flag > (max_cells-empty_flag)*0.4) {
            prefix += potion_flag == max_cells-empty_flag ? "" : "Chunky ";

            if (healing_flag > 0 || poison_flag > 0) {
                prefix += healing_flag > poison_flag ? "Regeneration " : "Poison ";
            }
            
            if (glow_flag > 0 || blinding_flag > 0) {
                prefix += glow_flag > blinding_flag ? "Night vision " : "Blinding ";
            }

            if (nausa_flag > 0) {
                prefix = "Nauseating " + prefix;
            }

            suffix = "Potion";
            suffix += spice_flag > 0 ? (spice_flag > max_cells*0.3 ? " from Hell" : " of Spice") : (spice_flag < 0 ? (spice_flag < (max_cells*0.3)*-1 ? " of Fructose" : " of Sweetness") : "");

        } else {
            prefix += wet_flag > 0 ? "Wet " : "";

            if (healing_flag > 0 || poison_flag > 0) {
                prefix += healing_flag > poison_flag ? "Healing " : "Harming ";
            }

            if (glow_flag > 0 || blinding_flag > 0) {
                prefix += glow_flag > blinding_flag ? "Glow " : "Dark ";
            }

            if (nausa_flag > 0) {
                prefix = "Sickening " + prefix;
            }

            suffix = wet_flag >= max_cells-empty_flag ? "Liquid" : "Powder";
            suffix += spice_flag > 0 ? (spice_flag > max_cells*0.3 ? " of Numbing" : " of Heat") : (spice_flag < 0 ? (spice_flag < (max_cells*0.3)*-1 ? " of Fragrance" : " of Sweetness") : "");
        }


        description += "\nProperties:\n";
        description += $"Used Space: {((max_cells-empty_flag)/max_cells)*100}%\n";
        description += $"Liquid Efficiency: {(wet_flag/(max_cells-empty_flag))*100}%\n";
        description += $"Pure Potion: {(potion_flag/(max_cells-empty_flag))*100}%\n";
        description += $"Estimated Taste: {(spice_flag > 0 ? (spice_flag > max_cells*0.3 ? "Hellish Spice" : "Spicy") : (spice_flag < 0 ? (spice_flag < (max_cells*0.3)*-1 ? "Overhealming Sweetness" : "Sweet") : (max_cells == empty_flag ? "None" :"Mundane")))}\n";
        description += $"Healing Grade: {healing_flag}\n";
        description += $"Poison Grade: {poison_flag}\n";
        description += $"Glow Grade: {glow_flag}\n";
        description += $"Blinding Grade: {blinding_flag}\n";
        description += $"Nausea Grade: {nausa_flag}\n";
        
        if (prefix == "Wet " && suffix == "Liquid") { prefix = "Drinking"; suffix = " Water"; }
        if (prefix == "Wet " && suffix == "Liquid of Sweetness") { prefix = "Honey"; suffix = " Water"; }

        processed_title.text = $"{prefix}{suffix}";
        processed_description.text = description;
        StartCoroutine(showText());
    }

    private IEnumerator showText () {
        float duration = 1f;
        float e_time = 0f;
        while (e_time < duration) {
            float fade = Mathf.Lerp(0,1,(e_time/duration));
            group.alpha = fade;
            e_time+=Time.deltaTime;
            yield return null;
        }
        group.alpha = 1;
    }
}
