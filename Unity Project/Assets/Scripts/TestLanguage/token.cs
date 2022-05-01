namespace DissertationTool.Assets.Scripts.TestLanguage
{
    public enum tokenTypes {
        EOF,
        NUMBER,
        STRING,
        PLUS,
        TIMES,
        CONCAT,
        PIPE,
        LET,
        BE,
        IN,
        NAME,
        SPEECHMARK
    }

    public class token //Used to help construct nodes later! Very important!
    {
        public tokenTypes type {get; private set;} 

        public string value {get; private set;} = "";

        public token(tokenTypes _type, string _val) {
            type = _type;
            value = _val;
        }
    }
}