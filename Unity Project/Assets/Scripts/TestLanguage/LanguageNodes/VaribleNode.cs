namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public class VaribleNode : INode
    {
        public token val {get; private set;}

        public VaribleNode(token _new_val) { val = _new_val; }

        public INode substitute(INode _expr, string _var) => val.value.Equals(_var) ? _expr : new VaribleNode(val);

    }
}