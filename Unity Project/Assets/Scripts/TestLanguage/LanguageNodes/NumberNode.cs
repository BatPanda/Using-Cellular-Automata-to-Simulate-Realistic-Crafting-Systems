namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public class NumberNode : INode
    {
        public token val {get; private set;}
        public NumberNode(token _new_val) { val = _new_val; }

        public INode substitute(INode _expr, string _var) => new NumberNode(val);
    }
}