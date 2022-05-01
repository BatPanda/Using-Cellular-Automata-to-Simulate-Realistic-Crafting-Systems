using System.Collections.Generic;
using System.Linq;
namespace DissertationTool.Assets.Scripts.DSL
{
    public class lexer //Where the string input is turned into tokens! 
    {
        private string input;
   
        private int position;

        private int line_number;
        private int character_number;

        public lexer(string _input) {
            position = 0;
            line_number = 0;
            character_number = 0;
            input = _input;
        }

        private char current_char {get => position >= input.Length ? '\0' : input[position];}

        private Dictionary<string,token> keywords = new Dictionary<string, token>() {
            { "&&", new token(TokenTypes.AND, "&&") },
            { "Ingredient", new token(TokenTypes.INGREDIENT, "Ingredient") },
            { "Group", new token(TokenTypes.GROUP, "Group") },
            { "When", new token(TokenTypes.WHEN, "When") },
            { "become", new token(TokenTypes.BECOME, "become") },
            { "around", new token(TokenTypes.AROUND, "around") },
            { "surrounded by", new token(TokenTypes.SURROUND_BY, "surrounded by") },
            { "with", new token(TokenTypes.WITH, "with") },
            { "northwest", new token(TokenTypes.DIRECTION, "0") },
            { "north", new token(TokenTypes.DIRECTION, "1") },
            { "northeast", new token(TokenTypes.DIRECTION, "2") },
            { "east", new token(TokenTypes.DIRECTION, "3") },
            { "southeast", new token(TokenTypes.DIRECTION, "4") },
            { "south", new token(TokenTypes.DIRECTION, "5") },
            { "southwest", new token(TokenTypes.DIRECTION, "6") },
            { "west", new token(TokenTypes.DIRECTION, "7") },
            { "swap", new token(TokenTypes.SWAP, "swap") },
            { ";", new token(TokenTypes.SEMICOLON, ";") },
            { "=", new token(TokenTypes.EQUALS, "=") },
            { "\0", new token(TokenTypes.EOF, "\0") },
            { "Percent", new token(TokenTypes.PERCENT, "Percent")}
        };

        public token peek() {
            int old_pos = position;
            token ret = next_token();
            position = old_pos;
            return ret;
        }

        public token next_token() {
            while (char.IsWhiteSpace(current_char)) {
                if (current_char == '\n') {
                    character_number = 0;
                    line_number++;
                } else {
                    character_number++;
                }
                position++;
            }
            if (position == input.Length) {return new token(TokenTypes.EOF, "\0").addLineAndCharacterNumber(line_number,character_number);}

            Maybe<token> m_keyword = extractKeywordToken();
            
            if (m_keyword.is_some) { return m_keyword.value; }

            if (char.IsDigit(current_char)) {
                return extractNumberToken();
            }

            return extractNameToken();
        }

        private Maybe<token> extractKeywordToken() {
            Maybe<token> valid_keyword = keywords.Keys.Where(_l => _l.Length + position <= input.Length)
                         .Where(_k => input.Substring(position,_k.Length) == _k).OrderByDescending(_k => _k.Length).Select(_t => new Maybe<token>(keywords[_t])).DefaultIfEmpty(new Maybe<token>()).First();
            if (valid_keyword.is_some) { 
                valid_keyword.value.line_number = line_number;
                valid_keyword.value.character_number = character_number;
                position += keywords.Where(_kv => _kv.Value.Equals(valid_keyword.value)).ToList()[0].Key.Length;
                character_number += keywords.Where(_kv => _kv.Value.Equals(valid_keyword.value)).ToList()[0].Key.Length;
            }
            return valid_keyword;
        }

        private token extractNameToken() {
            string name = "";
            int char_num = character_number;
            while (char.IsLetterOrDigit(current_char) || current_char == '_') {
                name += current_char;
                position ++;
                character_number++;
            }
            return new token(TokenTypes.IDENTIFIER, name).addLineAndCharacterNumber(line_number,char_num);
        }

        private token extractNumberToken() {
            string num_string = "";
            while (char.IsDigit(current_char))
            {
                 num_string += current_char;
                 position++;
            }
            return new token(TokenTypes.NUMBER, num_string);
        }
    }
}