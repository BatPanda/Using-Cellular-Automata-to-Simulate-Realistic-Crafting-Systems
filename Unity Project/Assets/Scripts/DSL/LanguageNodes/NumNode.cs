namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class NumNode : INode
    {
        public token val {get; private set;}
        public NumNode(token _new_val) { val = _new_val; }
    }
}