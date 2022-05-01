using Context = System.Collections.Generic.Dictionary<string,DissertationTool.Assets.Scripts.DSL.types>;
using System.Linq;
using System;
using DissertationTool.Assets.Scripts.DSL.LanguageNodes;
namespace DissertationTool.Assets.Scripts.DSL
{

    public enum types {
        INGREDIENT,
        GROUP,
        COMMAND,
        RULE_COMMAND,
        DIRECTION,
        PATTERN,
        NUMBER
    }

    public static class typeChecker // Checks that things are type safe for the DSL.
    {
        public static bool isTypeable(this INode _root) => _root.typeCommand(new Context()).is_some;

        private static Maybe<types> GetMaybe(this Context _gamma, string _key) {
            if (_gamma.ContainsKey(_key)) {
                return new Maybe<types>(_gamma[_key]);
            } return new Maybe<types>();
        }

        private static bool isType(this Maybe<types> _mt, types _t) => _mt.is_some && _mt.value == _t;

        private static Maybe<types> typeExpression(this INode _root, Context _gamma) => _root switch {
            AroundNode _a => _a.left_group.ingredients.Count > 0 && _a.right_group.ingredients.Count > 0 ? new Maybe<types>(types.PATTERN) : new Maybe<types>(),
            SurroundNode _s => _s.left_group.ingredients.Count > 0 && _s.right_group.ingredients.Count > 0 ? new Maybe<types>(types.PATTERN) : new Maybe<types>(),
            WithNode _w => _w.left_group.ingredients.Count > 0 && _w.right_group.ingredients.Count > 0 ? new Maybe<types>(types.PATTERN) : new Maybe<types>(),
            WrappedGroupNode _wg => new Maybe<types>(types.PATTERN),
            NumNode _n => int.TryParse(_n.val.value, out int n) ? new Maybe<types>(types.NUMBER) : new Maybe<types>(),
            _ => throw new System.ArgumentException($"Invalid type '{_root.GetType()}' used in typeExpression!")
        };

        private static Maybe<types> typeCommand(this INode _root, Context _gamma) => _root switch {
            IngredientNode _i => new Maybe<types>(types.COMMAND),
            GroupNode _g => _g.ingredients.Count > 0 ? new Maybe<types>(types.COMMAND) : new Maybe<types>(),
            RecipeNode _r => _r.left_side.typeExpression(_gamma).isType(types.PATTERN) ? new Maybe<types>(types.RULE_COMMAND) : new Maybe<types>(),
            SwapNode _s => _s.left_side.typeExpression(_gamma).isType(types.PATTERN) ? new Maybe<types>(types.RULE_COMMAND) : new Maybe<types>(),
            SemicolonNode _sc => (_sc.left_side.typeCommand(_gamma).isType(types.COMMAND) || _sc.left_side.typeCommand(_gamma).isType(types.RULE_COMMAND)) && (_sc.right_side.typeCommand(_gamma).isType(types.COMMAND) || _sc.right_side.typeCommand(_gamma).isType(types.RULE_COMMAND)) ? new Maybe<types>(types.COMMAND) : new Maybe<types>(),
            EpsilonNode _e => new Maybe<types>(types.COMMAND), 
            PercentNode _p => _p.right.typeCommand(_gamma).isType(types.RULE_COMMAND) ? new Maybe<types>(types.COMMAND) : new Maybe<types>(),
            _ => throw new System.ArgumentException($"Invalid type '{_root.GetType()}' used in typeCommand!")
        };
    }
}