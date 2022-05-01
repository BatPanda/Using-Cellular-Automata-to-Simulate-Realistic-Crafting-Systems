using Context = System.Collections.Generic.Dictionary<string,DissertationTool.Assets.Scripts.TestLanguage.types>;
using System.Linq;
using DissertationTool.Assets.Scripts.TestLanguage.LanguageNodes;
namespace DissertationTool.Assets.Scripts.TestLanguage
{
    public enum types {
        NUM,
        STR
    }
    public static class typeChecker //Checks if everything is typeable!
    {
        public static bool isTypeable(this INode _root) => _root.typeExpression(new Context()).is_some;

        private static Maybe<types> GetMaybe(this Context _gamma, string _key) {
            if (_gamma.ContainsKey(_key)) {
                return new Maybe<types>(_gamma[_key]);
            } return new Maybe<types>();
        }

        private static bool isType(this Maybe<types> _mt, types _t) => _mt.is_some && _mt.value == _t;

        private static Maybe<types> typeLet(this LetNode _root, Context _gamma) {
            Maybe<types> left_type = typeExpression(_root.left,_gamma);
            if (!left_type.is_some) {return new Maybe<types>();}
            _gamma[_root.var_name.value] = left_type.value;
            Maybe<types> right_type = typeExpression(_root.right,_gamma);
            _gamma.Remove(_root.var_name.value);
            return right_type;
        }

        private static Maybe<types> typeExpression(this INode _root, Context _gamma) => _root switch {
            VaribleNode var => _gamma.GetMaybe(var.val.value),
            StringNode str => new Maybe<types>(types.STR),
            NumberNode num => new Maybe<types>(types.NUM),
            PlusNode plus => plus.left.typeExpression(_gamma).isType(types.NUM) && plus.right.typeExpression(_gamma).isType(types.NUM) ? new Maybe<types> (types.NUM) : new Maybe<types>(),
            TimesNode times => times.left.typeExpression(_gamma).isType(types.NUM) && times.right.typeExpression(_gamma).isType(types.NUM) ? new Maybe<types> (types.NUM) : new Maybe<types>(),
            ConcatNode concat => concat.left.typeExpression(_gamma).isType(types.STR) && concat.right.typeExpression(_gamma).isType(types.STR) ? new Maybe<types> (types.STR) : new Maybe<types>(),
            LengthNode length => length.val.typeExpression(_gamma).isType(types.STR) ? new Maybe<types> (types.NUM) : new Maybe<types>(),
            LetNode let => let.typeLet(_gamma),

            _ => throw new System.ArgumentException($"Invalid type '{_root.GetType()}' used in typeExpression!")
        };
    }
}