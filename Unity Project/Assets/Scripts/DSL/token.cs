namespace DissertationTool.Assets.Scripts.DSL
{
    public enum TokenTypes {
        AND,
        INGREDIENT,
        GROUP,
        WHEN,
        BECOME,
        AROUND,
        SURROUND_BY,
        WITH,
        DIRECTION,
        IDENTIFIER,
        SWAP,
        SEMICOLON,
        EQUALS,
        EOF,
        PERCENT,
        NUMBER
    }

    public enum DirectionTypes {
        NORTH_WEST,
        NORTH,
        NORTH_EAST,
        EAST,
        SOUTH_EAST,
        SOUTH,
        SOUTH_WEST,
        WEST,
    }

    public class token //A holder for simple information, will be eaten by the parser and become a part of an Inode. Extra data here for internal states and to help with developers debugging!
    {
        public TokenTypes type {get; private set;} 

        public string value {get; private set;} = "";

        public int line_number {get; set;} = 0;
        public int character_number {get; set;} = 0;

        public token addLineAndCharacterNumber(int _line_num, int _char_num) { line_number = _line_num; character_number = _char_num; return this; }

        public token(TokenTypes _type, string _val) {
            type = _type;
            value = _val;
        }
    }
}