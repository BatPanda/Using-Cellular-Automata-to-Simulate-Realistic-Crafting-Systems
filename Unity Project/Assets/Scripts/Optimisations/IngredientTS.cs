using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class IngredientTS { //Because of the need for thread safe data types we could not use the normal ingredient type because unity is not thread safe. So this was made!
    [SerializeField] private string uniqueName;
    [SerializeField] private List<Color> BaseColour = new List<Color>{Color.white};
    public List<Color> getColours() => BaseColour;
    [SerializeField] private SerializableDictionary<string,float> properties;
    
    public Dictionary<string,float> getProperties() {
        return properties.ToDictionary(p => p.Key,p => p.Value);
    }
    
    public string getIngredientName() => uniqueName;

    public IngredientTS(Ingredient _i) {
        uniqueName = _i.getIngredientName();
        BaseColour = new List<Color>();
        _i.getColours().ForEach(_e => {
            BaseColour.Add(new Color(_e.r,_e.g,_e.b,_e.a));
        });
        properties = new SerializableDictionary<string, float>();
        _i.getProperties().ToList().ForEach(_e => {
            properties[_e.Key] = _e.Value;
        });
    }
}
