namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class LineNode : INode
    {
        public INode content {get; private set;}
        public LineNode(INode _content) {
            content = _content;
        }
    }
}