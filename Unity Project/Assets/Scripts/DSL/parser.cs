using System.Collections.Generic;
using DissertationTool.Assets.Scripts.DSL.LanguageNodes;
using System.Linq;
namespace DissertationTool.Assets.Scripts.DSL
{
    public class parser //Where tokens become I nodes. If you want to understand this, I would recommend first checking out the grammers from the report. Also try checking out the test language first as its much simpler :)
    {
        private lexer lex;
        private token current_token;

        
        public parser( lexer _new_lexer) { lex = _new_lexer; current_token = lex.next_token();}
        public parser( string _input) { lex = new lexer(_input); current_token = lex.next_token();}


        private void eat(TokenTypes _type) {
            if (_type == current_token.type) {
                current_token = lex.next_token();
            }
            else {
                throw new System.Exception($"Expected token type '{_type}' is not equal to the current tokens type of '{current_token.type}' at line {current_token.line_number}:{current_token.character_number}");
            }
        }

        public INode file() {
            List<INode> ret = new List<INode>();
            if (current_token.type == TokenTypes.EOF) return new EpsilonNode();
            ret.Add(line());
            while (current_token.type == TokenTypes.SEMICOLON) {
                eat(TokenTypes.SEMICOLON);
                ret.Add(line());
            }

            return ret.Aggregate((INode)new EpsilonNode(),(_a,_b) => (INode)new SemicolonNode(_a,_b));
        }

        private INode line() {
            if (current_token.type == TokenTypes.INGREDIENT) {
                eat(TokenTypes.INGREDIENT);
                INode ingr = new IngredientNode(current_token);
                eat (TokenTypes.IDENTIFIER);
                return ingr;
            } else if (current_token.type == TokenTypes.GROUP) {
                eat(TokenTypes.GROUP);
                token ident = current_token;
                eat(TokenTypes.IDENTIFIER);
                eat(TokenTypes.EQUALS);
                List<IngredientNode> grp = group().ingredients;
                return new GroupNode(ident,grp);

            } else if (current_token.type == TokenTypes.EOF) {
                return new EpsilonNode();
            } else if (current_token.type == TokenTypes.PERCENT) {
                eat(TokenTypes.PERCENT);
                return new PercentNode(number(),line());
            } else {
                eat(TokenTypes.WHEN);
                INode left = augmentedIngredient();
                if (current_token.type == TokenTypes.BECOME) {
                    eat(TokenTypes.BECOME);
                    token ident = current_token;
                    eat(TokenTypes.IDENTIFIER);
                    return new RecipeNode(left,ident);
                } else {
                    eat(TokenTypes.SWAP);
                    token dir = current_token;
                    eat(TokenTypes.DIRECTION);
                    return new SwapNode(left,new DirectionNode(dir));
                }
            }
        }

        private NumNode number() {
            token num = current_token;
            eat(TokenTypes.NUMBER);
            return new NumNode(num);
        }

        private INode augmentedIngredient() {
            WrappedGroupNode left = group();
            if (current_token.type == TokenTypes.AROUND) {
                eat(TokenTypes.AROUND);
                return new AroundNode(left,group());
            } else if (current_token.type == TokenTypes.SURROUND_BY) {
                eat(TokenTypes.SURROUND_BY);
                return new SurroundNode(left,group());
            } else if (current_token.type == TokenTypes.WITH) {
                eat(TokenTypes.WITH);
                token dir = current_token;
                eat(TokenTypes.DIRECTION);
                return new WithNode(new DirectionNode(dir),left,group());
            } else {
                return left;
            }
        }

        private WrappedGroupNode group() {
            List<IngredientNode> ingrlist = new List<IngredientNode>();
            ingrlist.Add(new IngredientNode(current_token));
            eat(TokenTypes.IDENTIFIER);
            while (current_token.type == TokenTypes.AND) {
                eat(TokenTypes.AND);
                ingrlist.Add(new IngredientNode(current_token));
                eat(TokenTypes.IDENTIFIER);
            }
            return new WrappedGroupNode(ingrlist);
        }
    }
}