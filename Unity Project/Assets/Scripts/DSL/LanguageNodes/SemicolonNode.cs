namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class SemicolonNode : INode
    {
        public INode left_side {get; private set;}
        public INode right_side {get; private set;}

        public SemicolonNode(INode _l, INode _r) {
            left_side = _l;
            right_side = _r;
        }
    }
}