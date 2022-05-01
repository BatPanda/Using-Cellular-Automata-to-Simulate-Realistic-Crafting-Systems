namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public class StringNode : INode
    {
        public token val {get; private set;}
        
        public StringNode(token _new_val) { val = _new_val; }
        public INode substitute(INode _expr, string _var) => new StringNode(val);

    }
}