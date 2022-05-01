using System.Collections.Generic;
using System.Linq;
using System;
using DissertationTool.Assets.Scripts.Rules;
namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class WrappedGroupNode : INode
    {
        public List<string> getIngredientNames(Ruleset _nu) {
            List<string> ret = new List<string>();
            List<string> expanded_groups = new List<string>();
            ingredients.ForEach(_i => { //todo
                string token_val = _i.name.value;
                if (_nu.ingredients_groups.Keys.Contains(token_val)) {

                    int group_key = _nu.ingredients_groups[token_val];
                    string binary = Convert.ToString(group_key,2);
                    binary = binary.reverseString();
                    for (int i = 0; i < binary.Length; i++)
                    {
                        if (binary[i] == '1') {
                            ret.Add(_nu.used_ingredients.Keys.ToList()[i]);
                        }
                    }
                } else if (_nu.used_ingredients.Keys.Contains(token_val)) {
                    ret.Add(token_val);                    
                } else {
                    throw new System.ArgumentException($"Ingredient with name '{token_val}' not a known ingredient!");
                }
            });
            return ret;
        }
        public List<IngredientNode> ingredients {get; private set;}
        public WrappedGroupNode (List<IngredientNode> _ingredients) {
            ingredients = _ingredients;
        }
    }
}