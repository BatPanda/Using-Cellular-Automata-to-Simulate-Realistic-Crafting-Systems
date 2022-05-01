namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class AroundNode : INode
    {
        public WrappedGroupNode left_group {get; private set;}
        public WrappedGroupNode right_group {get; private set;}

        public AroundNode(WrappedGroupNode _left, WrappedGroupNode _right) {
            left_group = _left;
            right_group = _right;
        }
    }
}