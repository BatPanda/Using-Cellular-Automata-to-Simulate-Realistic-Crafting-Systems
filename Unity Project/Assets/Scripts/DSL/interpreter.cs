using DissertationTool.Assets.Scripts.DSL.LanguageNodes;
using DissertationTool.Assets.Scripts.Rules;
using System;
using System.Linq;
using System.Collections.Generic;
namespace DissertationTool.Assets.Scripts.DSL
{
    public class interpreter //Where Inodes are executed. The idea is that this will generate a ruleset class by building up rules. Check out the statics and dynamics in the report.
    {
        private enum PatternTypes {
            AROUND,
            SURROUND,
            WITH,
            GROUP
        }
        private INode root;

        public interpreter(INode _tree) { root = _tree; }
        public interpreter(parser _parser) {root = _parser.file();}
        public interpreter(lexer _lexer) {root = new parser(_lexer).file();}
        public interpreter(string _input) {root = new parser(_input).file();} 

        public bool isTypeable() => typeChecker.isTypeable(root);

        public Ruleset evaluate() => isTypeable() ? evaluateCommand(root, new Ruleset(new Dictionary<string, int>(), new Dictionary<string, int>(), new List<Rule>())) : throw new Exception("CA Rule input cannot be typed. (This is a syntax error in the CA config, probably)");
        
        private List<(A,B)> cartesianProduct<A,B> (List<A> _left, List<B> _right) => _left.SelectMany(_x => _right.Select(_y => (_x,_y))).ToList();
        private Ruleset handleRecipe(INode _pattern, token _result, Ruleset _nu, int _chance = 100) {
            switch(evaluateExpression(_pattern,_nu)) {
                case var tuple when tuple.Item3.is_some && tuple.Item4.Equals(PatternTypes.WITH): {
                    List<(string,string)> rule_parts = cartesianProduct(tuple.Item1,tuple.Item2);
                    rule_parts.ForEach(_rp => _nu.rules.Add(new Rule(_rp.Item1,RuleDetectionType.WITH,tuple.Item3,_rp.Item2,_result.value,_chance)));
                    break;
                }
                case var tuple when tuple.Item4.Equals(PatternTypes.AROUND): {
                    List<(string,string)> rule_parts = cartesianProduct(tuple.Item1,tuple.Item2);
                    rule_parts.ForEach(_rp => _nu.rules.Add(new Rule(_rp.Item2,RuleDetectionType.AROUND,tuple.Item3,_rp.Item1,_result.value,_chance)));
                    break;
                }
                case var tuple when tuple.Item4.Equals(PatternTypes.SURROUND): {
                    List<(string,string)> rule_parts = cartesianProduct(tuple.Item1,tuple.Item2);
                    rule_parts.ForEach(_rp => _nu.rules.Add(new Rule(_rp.Item1,RuleDetectionType.SURROUND,tuple.Item3,_rp.Item2,_result.value,_chance)));
                    break;
                }
                case var tuple when tuple.Item4.Equals(PatternTypes.GROUP): {
                    tuple.Item1.ForEach(_t => _nu.rules.Add(new Rule(_t,RuleDetectionType.GROUP,_result.value,_chance)));
                    break;
                }
                default: {
                    throw new System.ArgumentException("switch fell out of scope in handleRecipe");
                }
            }
            return _nu;
        } 

        private Ruleset handleSwap(INode _pattern, DirectionTypes _dir, Ruleset _nu, int _chance = 100) {
            switch(evaluateExpression(_pattern,_nu)) {
                case var tuple when tuple.Item3.is_some && tuple.Item4.Equals(PatternTypes.WITH): {
                    List<(string,string)> rule_parts = cartesianProduct(tuple.Item1,tuple.Item2);
                    rule_parts.ForEach(_rp => _nu.rules.Add(new Rule(_rp.Item1,RuleDetectionType.WITH,tuple.Item3,_rp.Item2,_dir,_chance)));
                    break;
                }
                case var tuple when tuple.Item4.Equals(PatternTypes.AROUND): {
                    List<(string,string)> rule_parts = cartesianProduct(tuple.Item1,tuple.Item2);
                    rule_parts.ForEach(_rp => _nu.rules.Add(new Rule(_rp.Item2,RuleDetectionType.WITH,tuple.Item3,_rp.Item1,_dir,_chance)));
                    break;
                }
                case var tuple when tuple.Item4.Equals(PatternTypes.SURROUND): {
                    List<(string,string)> rule_parts = cartesianProduct(tuple.Item1,tuple.Item2);
                    rule_parts.ForEach(_rp => _nu.rules.Add(new Rule(_rp.Item1,RuleDetectionType.WITH,tuple.Item3,_rp.Item2,_dir,_chance)));
                    break;
                }
                case var tuple when tuple.Item4.Equals(PatternTypes.GROUP): {
                    tuple.Item1.ForEach(_t => _nu.rules.Add(new Rule(_t,RuleDetectionType.GROUP,_dir,_chance)));
                    break;
                }
                default: {
                    throw new System.ArgumentException("switch fell out of scope in handleSwap");
                }
            }
            return _nu;
        }

        public Ruleset evaluateCommand(INode _root, Ruleset _nu) => _root switch {
            EpsilonNode _e => _nu,
            IngredientNode _i => _nu.addIngredient(_i.name),
            SemicolonNode _sc => evaluateCommand(_sc.right_side, evaluateCommand(_sc.left_side,_nu)),
            GroupNode _g => _nu.addGroup(_g.name,_g.ingredients),
            RecipeNode _r => handleRecipe(_r.left_side,_r.result,_nu),
            SwapNode _s => handleSwap(_s.left_side,_s.direction.direction,_nu),
            PercentNode _p => _p.right switch {
                RecipeNode _r => handleRecipe(_r.left_side,_r.result,_nu,evaltuateNumber(_p.chance,_nu)),
                SwapNode _s => handleSwap(_s.left_side,_s.direction.direction,_nu,evaltuateNumber(_p.chance,_nu)),
                _ => throw new System.ArgumentException($"No registered case for INode {_root} in PercentNode pattern matching.")
            },
            _ => throw new System.ArgumentException($"No registered case for INode {_root} in evaluateCommand.")
        };
        
        private (List<string>,List<string>,Maybe<DirectionTypes>,PatternTypes) evaluateExpression(INode _root, Ruleset _nu) => _root switch {
            AroundNode _a => (_a.left_group.getIngredientNames(_nu),_a.right_group.getIngredientNames(_nu),new Maybe<DirectionTypes>(),PatternTypes.AROUND),
            SurroundNode _s => (_s.left_group.getIngredientNames(_nu),_s.right_group.getIngredientNames(_nu),new Maybe<DirectionTypes>(),PatternTypes.SURROUND),
            WithNode _w => (_w.left_group.getIngredientNames(_nu),_w.right_group.getIngredientNames(_nu),new Maybe<DirectionTypes>(_w.direction.direction),PatternTypes.WITH),
            WrappedGroupNode _wg => (_wg.getIngredientNames(_nu),new List<string>(),new Maybe<DirectionTypes>(),PatternTypes.GROUP), 
            _ => throw new System.ArgumentException($"No registered case for INode {_root} in evaluateExpression.")
        };

        private int evaltuateNumber(INode _root, Ruleset _nu) => _root switch {
            NumNode _n => Convert.ToInt32(_n.val.value),
            _ => throw new System.ArgumentException($"No registered case for INode {_root} in evaltuateNumber.")
        };
    }
}