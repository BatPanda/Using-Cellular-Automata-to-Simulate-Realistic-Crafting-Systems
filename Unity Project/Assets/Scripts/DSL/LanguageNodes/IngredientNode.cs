namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class IngredientNode : INode
    {
        public token name {get; private set;}
        public IngredientNode(token _name) {
            name = _name;
        }
    }
}