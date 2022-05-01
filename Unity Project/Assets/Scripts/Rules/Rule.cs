using DissertationTool.Assets.Scripts.DSL;
namespace DissertationTool.Assets.Scripts.Rules
{
    public enum RuleDetectionType {
        AROUND,
        SURROUND,
        WITH,
        GROUP
    }

    public enum RuleResolutionType {
        UPDATE_SELF,
        SWAP
    }

    public class Rule //The rule class, used to store the metadata about a single rule. Is created by the DSL and added to a ruleset.
    {
        public string active_site_type {get; private set;}
        public Maybe<string> target_site_type {get; private set;}
        public RuleDetectionType detection_type {get; private set;}
        public Maybe<DirectionTypes> detection_with_direction {get; private set;}
        public RuleResolutionType resolution_type {get; private set;}
        public Maybe<DirectionTypes> swap_direction {get; private set;}
        public Maybe<string> new_type {get; private set;}
        public int chance {get; private set;}

        public Rule(Rule _rule) {
            active_site_type = _rule.active_site_type;
            detection_type = _rule.detection_type;
            if (_rule.detection_with_direction.is_some) {
                detection_with_direction = new Maybe<DirectionTypes>(_rule.detection_with_direction.value);
            } else {
                detection_with_direction = new Maybe<DirectionTypes>();
            }
            if (_rule.target_site_type.is_some) {
                target_site_type = new Maybe<string>(_rule.target_site_type.value);
            } else {
                target_site_type = new Maybe<string>();
            }
            resolution_type = _rule.resolution_type;
            if (_rule.new_type.is_some) {
                _rule.new_type = new Maybe<string>(_rule.new_type.value);
            } else {
                _rule.new_type = new Maybe<string>();
            }
            if (_rule.swap_direction.is_some) {
                _rule.swap_direction = new Maybe<DirectionTypes>(_rule.swap_direction.value);
            } else {
                _rule.swap_direction = new Maybe<DirectionTypes>();
            }
            chance = _rule.chance;
        }

        public Rule(string _active_type, RuleDetectionType _detection_type, Maybe<DirectionTypes> _maybe_with_direction, string _target_type, string _result_type, int _chance) {
            active_site_type = _active_type;
            detection_type = _detection_type;
            detection_with_direction = _maybe_with_direction;
            target_site_type = new Maybe<string>(_target_type);
            resolution_type = RuleResolutionType.UPDATE_SELF;
            new_type = new Maybe<string>(_result_type);
            swap_direction = new Maybe<DirectionTypes>();
            chance = _chance;
        } 
        public Rule(string _active_type, RuleDetectionType _detection_type, Maybe<DirectionTypes> _maybe_with_direction, string _target_type, DirectionTypes _swap_direction, int _chance) {
            active_site_type = _active_type;
            detection_type = _detection_type;
            detection_with_direction = _maybe_with_direction;
            target_site_type = new Maybe<string>(_target_type);
            resolution_type = RuleResolutionType.SWAP;
            swap_direction = new Maybe<DirectionTypes>(_swap_direction);
            new_type = new Maybe<string>();
            chance = _chance;
        }

        public Rule(string _active_type, RuleDetectionType _detection_type, string _result_type, int _chance) {
            active_site_type = _active_type;
            detection_type = _detection_type;
            detection_with_direction = new Maybe<DirectionTypes>();
            target_site_type = new Maybe<string>();
            resolution_type = RuleResolutionType.UPDATE_SELF;
            swap_direction = new Maybe<DirectionTypes>();
            new_type = new Maybe<string>(_result_type);
            chance = _chance;
        }

        public Rule(string _active_type, RuleDetectionType _detection_type, DirectionTypes _swap_direction, int _chance) {
            active_site_type = _active_type;
            detection_type = _detection_type;
            detection_with_direction = new Maybe<DirectionTypes>();
            target_site_type = new Maybe<string>();
            resolution_type = RuleResolutionType.SWAP;
            swap_direction = new Maybe<DirectionTypes>(_swap_direction);
            new_type = new Maybe<string>();
            chance = _chance;
        }
    }
}