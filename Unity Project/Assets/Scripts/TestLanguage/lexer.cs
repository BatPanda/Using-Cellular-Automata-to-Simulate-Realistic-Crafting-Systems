using System.Collections.Generic;
using System.Linq;
namespace DissertationTool.Assets.Scripts.TestLanguage
{
    public class lexer //Where the input string becomes tokens!
    {
        public lexer(string _input) {
            position = 0;
            input = _input;
        }

        private string input;
        
        private int position;

        private Queue<token> backlog = new Queue<token>();

        private char current_char {get => position >= input.Length ? '\0' : input[position];}

        private Dictionary<string,token> keywords = new Dictionary<string, token>() {
            { "let", new token(tokenTypes.LET, "let") },
            { "in", new token(tokenTypes.IN, "in") },
            { "be", new token(tokenTypes.BE, "be") },
            { "^", new token(tokenTypes.CONCAT, "^") },
            { "|", new token(tokenTypes.PIPE, "|") },
            { "\0", new token(tokenTypes.EOF, "\0") },
            { "+", new token(tokenTypes.PLUS, "+") },
            { "*", new token(tokenTypes.TIMES, "*") }
        };

        public token peek() {
            int old_pos = position;
            token ret = next_token();
            position = old_pos;
            return ret;
        }

        public token next_token() {
            if (backlog.Count > 0) {
                return backlog.Dequeue();
            }

            while (char.IsWhiteSpace(current_char)) {
                position++;
            }

            Maybe<token> m_keyword = extractKeywordToken();
            if (m_keyword.is_some) { return m_keyword.value; }

            if (current_char == '"') {
                return extractStringToken();
            }

            if (char.IsDigit(current_char)) {
                return extractNumberToken();
            }

            return extractNameToken();

        }

        private Maybe<token> extractKeywordToken() {
            Maybe<token> valid_keyword = keywords.Keys.Where(_l => _l.Length + position <= input.Length)
                         .Where(_k => input.Substring(position,_k.Length) == _k).OrderByDescending(_k => _k.Length).Select(_t => new Maybe<token>(keywords[_t])).DefaultIfEmpty(new Maybe<token>()).First();
            if (valid_keyword.is_some) { position += valid_keyword.value.value.Length;}
            return valid_keyword;
        }

        private token extractStringToken() {
            string ret = input.Substring(position+1,input.IndexOf('"',position+1) - position-1);
            backlog.Enqueue(new token(tokenTypes.STRING, ret));
            backlog.Enqueue(new token(tokenTypes.SPEECHMARK, "\""));
            position += ret.Length+2;
            return new token(tokenTypes.SPEECHMARK, "\"");
        }

        private token extractNumberToken() {
            string num_string = "";
            while (char.IsDigit(current_char))
            {
                 num_string += current_char;
                 position++;
            }
            return new token(tokenTypes.NUMBER, num_string);
        }

        private token extractNameToken() {
            string name = "";
            while (char.IsLetter(current_char)) {
                name += current_char;
                position ++;
            }
            return new token(tokenTypes.NAME, name);
        }
    }
}