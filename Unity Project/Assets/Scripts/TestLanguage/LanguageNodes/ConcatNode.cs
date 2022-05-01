namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public class ConcatNode : INode
    {
        public INode left {get; private set;}
        public INode right {get; private set;}

        public ConcatNode( INode _l, INode _r ) { left = _l; right = _r; }

        public INode substitute(INode _expr, string _var) => new ConcatNode (left.substitute(_expr,_var),right.substitute(_expr,_var));

    }
}