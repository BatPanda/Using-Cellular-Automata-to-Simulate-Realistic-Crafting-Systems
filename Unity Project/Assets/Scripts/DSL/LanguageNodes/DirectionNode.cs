using System;
namespace DissertationTool.Assets.Scripts.DSL.LanguageNodes
{
    public class DirectionNode : INode
    {
        public DirectionTypes direction {get; private set;}
        public DirectionNode (token _direction) {
            direction = (DirectionTypes)Convert.ToInt32(_direction.value);
        }
    }
}