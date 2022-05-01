using DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes;
using System;
using System.Linq;
namespace DissertationTool.Assets.Scripts.TestLanguage
{
    public class interpreter //Where Inodes are executed!
    {
        private INode root;

        public interpreter(INode _tree) { root = _tree; }
        public interpreter(parser _parser) {root = _parser.expr();}
        public interpreter(lexer _lexer) {root = new parser(_lexer).expr();}
        public interpreter(string _input) {root = new parser(_input).expr();} 

        public bool isTypeable() => typeChecker.isTypeable(root);

        public Either<string,int> evaluate() => evaluate(root);
        private Either<string,int> evaluate(INode _root) => _root switch { //structural dynamics time
            StringNode _str => new Either<string,int>(_str.val.value),
            NumberNode _num => new Either<string,int>(Convert.ToInt32(_num.val.value)),
            PlusNode _plus => new Either<string,int>(evaluate(_plus.left).Right + evaluate(_plus.right).Right),
            ConcatNode _cat => new Either<string,int>(evaluate(_cat.left).Left + evaluate(_cat.right).Left),
            LengthNode _len => new Either<string,int>(evaluate(_len.val).Left.Length),
            TimesNode _tim => new Either<string,int>(evaluate(_tim.left).Right * evaluate(_tim.right).Right),
            LetNode _let => evaluate(_let.right.substitute(eitherResultToINode(evaluate(_let.left)),_let.var_name.value)),
            _ => throw new System.ArgumentException("?")
        };

        private INode eitherResultToINode (Either<string,int> _e) => _e.is_left ? (INode)(new StringNode(new token(tokenTypes.STRING, _e.Left))) : (INode)(new NumberNode(new token(tokenTypes.NUMBER, _e.Right.ToString())));
    }
}