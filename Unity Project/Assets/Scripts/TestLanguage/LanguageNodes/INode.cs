namespace DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes
{
    public interface INode
    {
         INode substitute(INode _expr, string _var);
    }
}