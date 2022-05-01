namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class SwapNode : INode
    {
        public INode left_side {get; private set;}
        public DirectionNode direction {get; private set;}
        public SwapNode(INode _left_side, DirectionNode _direction) {
            left_side = _left_side;
            direction = _direction;
        }
    }
}