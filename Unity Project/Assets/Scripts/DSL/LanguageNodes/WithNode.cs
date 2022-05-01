namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class WithNode : INode
    {
        public DirectionNode direction {get; private set;}
        public WrappedGroupNode left_group {get; private set;}
        public WrappedGroupNode right_group {get; private set;}

        public WithNode(DirectionNode _direction, WrappedGroupNode _left, WrappedGroupNode _right) {
            direction = _direction;
            left_group = _left;
            right_group = _right;
        }
    }
}