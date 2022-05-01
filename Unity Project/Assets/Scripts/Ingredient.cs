using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Crafting Prototype/Ingredient", order = 2)]
public class Ingredient : ScriptableObject // an ingredient! this is a scriptable object used to store information that will be placed into the ingredient interaction config.
{
    [SerializeField] private string uniqueName;
    [SerializeField] private string displayName;

    [SerializeField] private bool selectable = true;
    [SerializeField] private List<Color> BaseColour = new List<Color>{Color.white};

    public List<Color> getColours() => BaseColour;

    public bool getSelectable() => selectable;

    [SerializeField] private SerializableDictionary<string,float> properties;
    
    public Dictionary<string,float> getProperties() {
        return properties.ToDictionary(p => p.Key,p => p.Value);
    }
    
    public string getIngredientName() => uniqueName;
    public string getIngredientDisplayName() => displayName;
}
