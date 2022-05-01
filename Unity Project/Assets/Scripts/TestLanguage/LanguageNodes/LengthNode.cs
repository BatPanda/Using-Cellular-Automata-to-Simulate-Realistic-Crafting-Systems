namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public class LengthNode : INode
    {
        public INode val {get; private set;}

        public LengthNode( INode _str ) { val = _str;}

        public INode substitute(INode _expr, string _var) => new LengthNode(val.substitute(_expr,_var));

    }
}