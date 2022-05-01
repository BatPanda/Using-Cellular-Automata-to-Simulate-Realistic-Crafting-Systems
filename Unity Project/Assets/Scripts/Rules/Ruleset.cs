using System;
using System.Linq;
using System.Collections.Generic;
using DissertationTool.Assets.Scripts.DSL;
using DissertationTool.Assets.Scripts.DSL.LanguageNodes;
namespace DissertationTool.Assets.Scripts.Rules
{
    public class Ruleset //A ruleset is a class used to contain all rules this is what the DSL will strive to generate!
    {
        public Ruleset(Dictionary<string,int> _used_ingredients, Dictionary<string,int> _ingredients_groups, List<Rule> _rules, int _seed) {
            used_ingredients = _used_ingredients;
            ingredients_groups = _ingredients_groups;
            rules = _rules;
            rand = new System.Random(_seed);  
        }

        public Ruleset(Ruleset _ruleset) {
            used_ingredients = new Dictionary<string, int>();
            _ruleset.used_ingredients.Keys.ToList().ForEach(_e => {
                used_ingredients[_e] = _ruleset.used_ingredients[_e];
            });
            ingredients_groups = new Dictionary<string, int>();
            _ruleset.ingredients_groups.Keys.ToList().ForEach(_e => {
                ingredients_groups[_e] = _ruleset.ingredients_groups[_e];
            });
            rules = new List<Rule>();
            _ruleset.rules.ForEach(_r => rules.Add(new Rule(_r)));
            rand = _ruleset.rand; //note: this may not be safe.
        }

        public Ruleset(Dictionary<string,int> _used_ingredients, Dictionary<string,int> _ingredients_groups, List<Rule> _rules) : this (_used_ingredients,_ingredients_groups,_rules,Guid.NewGuid().GetHashCode()) {}

        private System.Random rand;
        public Dictionary<string,int> used_ingredients {get; private set;} =  new Dictionary<string, int>();
        public Dictionary<string,int> ingredients_groups {get; private set;} = new Dictionary<string, int>();
        public List<Rule> rules {get; private set;} = new List<Rule>();

        public int getNextPercentileNumber() {
            return rand.Next(0,101);
        }

        public Ruleset addIngredient(token _t) {
            used_ingredients.Add(_t.value,used_ingredients.Count);
            return this;
        }

        private int getGroupID(List<IngredientNode> _i) => _i.Aggregate(0,(_acc,_b) => _acc + (int)System.Math.Pow(2,used_ingredients[_b.name.value]));

        public Ruleset addGroup(token _t, List<IngredientNode> _i) {
            ingredients_groups.Add(_t.value,getGroupID(_i));
            return this;
        }

    }
}