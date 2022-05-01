namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public class LetNode : INode
    {
        public INode left {get; private set;}
        public INode right {get; private set;}
        public token var_name {get; private set;}

        public LetNode(token _var_name, INode _left, INode _right) { var_name = _var_name; left = _left; right = _right;}

        public INode substitute(INode _expr, string _var) => _var.Equals(var_name.value) ? new LetNode(var_name,left,right) : new LetNode(var_name,left.substitute(_expr,_var),right.substitute(_expr,_var));
    }
}