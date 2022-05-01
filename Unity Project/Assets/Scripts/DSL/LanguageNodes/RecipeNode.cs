namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class RecipeNode : INode
    {
        public INode left_side {get; private set;}
        public token result {get; private set;}

        public RecipeNode(INode _left_side, token _result) {
            left_side = _left_side;
            result = _result;
        }
    }
}