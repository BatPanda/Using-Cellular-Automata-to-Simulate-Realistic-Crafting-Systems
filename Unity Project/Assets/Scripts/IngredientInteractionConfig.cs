using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SortingBiasOrderTypes {
    TOP_LEFT_TO_BOTTOM_RIGHT,
    BOTTOM_RIGHT_TO_TOP_RIGHT
} 

[CreateAssetMenu(fileName = "NewIngredientConfig", menuName = "Crafting Prototype/Ingredient Interaction Config", order = 1)]
public class IngredientInteractionConfig : ScriptableObject //This is scriptable object is the core instruction for the CA. It stores everthing needed to generate the rules.
{
    [Header("Metadata")]
    [Tooltip ("Ingredient config name used for debugging.")] [SerializeField] private string configName;

    [Header("Ingredient Information")]
    [Tooltip ("The list of ingredients.")] [SerializeField] private List<Ingredient> ingredients;
    [Tooltip ("The order cells with the same rule resolve in")] [SerializeField] private SortingBiasOrderTypes sortingBiasOrder = SortingBiasOrderTypes.BOTTOM_RIGHT_TO_TOP_RIGHT;
    [Tooltip ("The default cell state, all cells will start in this state unless changed.")] [SerializeField] private int startingStateIndex;

    [Header("Rule Resolve Code")]
    [Tooltip ("Code used to resolve the CA ruleset.")] [TextArea(1,50)] [SerializeField] private string ruleInput;

    public bool has_stored {get; private set;} = false;
    public Vector2Int stored_cell_grid_size {get; private set;}
    public Vector2Int stored_cell_padding {get; private set;}
    public float stored_cell_size {get; private set;}
    public string stored_cell_step_rate_field_text {get; private set;}
    public bool stored_cellular_step_rate_paused {get; private set;}
    public SerializableDictionary<Vector2Int,Ingredient> stored_cell_grid {get; private set;}

    public void setInternalState(Vector2Int _stored_cell_grid_size, Vector2Int _stored_cell_padding, float _stored_cell_size, string _stored_cell_step_rate_field_text, bool _cellular_step_rate_paused_stored, SerializableDictionary<Vector2Int,Ingredient> _stored_cell_grid) {
        has_stored = true;
        stored_cell_grid_size = _stored_cell_grid_size; 
        stored_cell_padding = _stored_cell_padding;
        stored_cell_size = _stored_cell_size;
        stored_cell_step_rate_field_text = _stored_cell_step_rate_field_text;
        stored_cellular_step_rate_paused = _cellular_step_rate_paused_stored;
        stored_cell_grid = _stored_cell_grid;
    }

    public string getConfigName() => configName;
    public List<Ingredient> getIngredients() => ingredients; 
    public SortingBiasOrderTypes getSortingBias() => sortingBiasOrder; 
    public Ingredient getDefaultIngredient() {
        if (ingredients.ElementAtOrDefault(startingStateIndex) == null) {
            throw new System.ArgumentException($"{configName} has no Ingredient registered with an ID of '{startingStateIndex}'");
        }
        return ingredients[startingStateIndex];
    }
    public string getRuleInput() => ruleInput;

    
}
