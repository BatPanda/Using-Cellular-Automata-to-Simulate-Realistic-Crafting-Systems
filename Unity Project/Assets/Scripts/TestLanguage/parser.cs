using DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes;
namespace DissertationTool.Assets.Scripts.TestLanguage
{
    public class parser //Where tokens become Inodes!
    {
        private lexer lex;
        private token current_token;

        
        public parser( lexer _new_lexer) { lex = _new_lexer; current_token = lex.next_token();}
        public parser( string _input) { lex = new lexer(_input); current_token = lex.next_token();}


        private void eat(tokenTypes _type) {
            if (_type == current_token.type) {
                current_token = lex.next_token();
            }
            else {
                throw new System.Exception($"Expected token type '{_type}' is not equal to the current tokens type of '{current_token.type}'");
            }
        } 

        //recursive desent parser
        public INode expr () {
            if (current_token.type == tokenTypes.LET) {
                return letExpr();
            } 
            else if (current_token.type == tokenTypes.PIPE || current_token.type == tokenTypes.NUMBER) {
                return numericTerm();
            } 
            else if (current_token.type == tokenTypes.SPEECHMARK) {
                return stringTerm();
            } 
            else {
                token next_token = lex.peek();
                if (next_token.type == tokenTypes.CONCAT) {
                    return stringTerm();
                } else if (next_token.type == tokenTypes.PLUS || next_token.type == tokenTypes.TIMES) {
                    return numericTerm();
                } else {
                    return ident();
                }
            }

        }

        private INode letExpr() {
            eat(tokenTypes.LET);
            token var_name = current_token;
            eat(tokenTypes.NAME);
            eat(tokenTypes.BE);
            INode left = expr();
            eat(tokenTypes.IN);
            INode right = expr(); 
            return new LetNode(var_name,left,right); 
        }

        private INode numericTerm() {
            INode ret = numericFactor();
            while (current_token.type == tokenTypes.PLUS) {
                eat(tokenTypes.PLUS);
                ret = new PlusNode(ret,numericFactor());
            }
            return ret;
        }

        private INode numericFactor() {
            INode ret = value();
            while (current_token.type == tokenTypes.TIMES) {
                eat(tokenTypes.TIMES);
                ret = new TimesNode(ret,value());
            }
            return ret;
        }

        private INode value() {
            if (current_token.type == tokenTypes.PIPE) {
                eat(tokenTypes.PIPE);
                INode ret = new LengthNode(stringTerm());
                eat(tokenTypes.PIPE);
                return ret;
            } 
            return current_token.type == tokenTypes.NUMBER ? numeric() : (current_token.type == tokenTypes.SPEECHMARK ? str() : ident());
        }

        private INode stringTerm() {
            INode ret = value();
            while (current_token.type == tokenTypes.CONCAT) {
                eat(tokenTypes.CONCAT);
                ret = new ConcatNode(ret, value());
            }
            return ret;
        }

        private INode numeric() {
            INode ret = new NumberNode(current_token);
            eat(tokenTypes.NUMBER);
            return ret;
        }

        private INode str() {
            eat(tokenTypes.SPEECHMARK);
            INode ret = new StringNode(current_token);
            eat(tokenTypes.STRING);
            eat(tokenTypes.SPEECHMARK);
            return ret;
        }

        private INode ident() {
            INode ret = new VaribleNode(current_token);
            eat(tokenTypes.NAME);
            return ret;
        }

    }
}