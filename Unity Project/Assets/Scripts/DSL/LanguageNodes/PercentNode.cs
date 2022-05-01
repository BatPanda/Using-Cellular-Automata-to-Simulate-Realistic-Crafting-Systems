namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class PercentNode : INode
    {
        public NumNode chance {get; private set;}
        public INode right {get; private set;}

        public PercentNode(NumNode _chance, INode _right) {
            chance = _chance; 
            right = _right;
        }
    }
}