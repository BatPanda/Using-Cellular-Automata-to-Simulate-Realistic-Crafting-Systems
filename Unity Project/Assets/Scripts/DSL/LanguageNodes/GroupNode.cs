using System.Collections.Generic;
namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class GroupNode : INode
    {
        public token name {get; private set;}
        public List<IngredientNode> ingredients {get; private set;}
        public GroupNode (token _name, List<IngredientNode> _ingredients) {
            name = _name;
            ingredients = _ingredients;
        }
    }
}