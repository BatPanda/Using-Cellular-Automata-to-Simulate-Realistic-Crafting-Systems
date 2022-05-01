using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Linq;
using DissertationTool.Assets.Scripts.Rules;
using DissertationTool.Assets.Scripts.DSL;
public static class CellGridHelper //A static helper class that deals helps handle generic functions. As such its used by every iteration.
{
    public static bool isCoordInGrid(this Vector2Int _max_size, Vector2Int _current_coord) => _current_coord.x >= 0 && _current_coord.y >= 0 && _current_coord.x < _max_size.x && _current_coord.y < _max_size.y;

    public static List<Vector2Int> makeMooreNeighbourhoodCoordsAroundCenter(this Vector2Int _center) => new List<Vector2Int>() { 
            new Vector2Int(_center.x-1,_center.y-1), new Vector2Int(_center.x,_center.y-1), new Vector2Int(_center.x+1,_center.y-1), 
            new Vector2Int(_center.x-1,_center.y),                                          new Vector2Int(_center.x+1,_center.y), 
            new Vector2Int(_center.x-1,_center.y+1), new Vector2Int(_center.x,_center.y+1), new Vector2Int(_center.x+1,_center.y+1)};

    public static List<Vector2Int> makeVonNeumannNeighbourhoodCoordsAroundCenter(this Vector2Int _center) => new List<Vector2Int>() { 
                                                     new Vector2Int(_center.x,_center.y-1),
            new Vector2Int(_center.x-1,_center.y),                                          new Vector2Int(_center.x+1,_center.y), 
                                                     new Vector2Int(_center.x,_center.y+1)};
    public static List<(GameObject,Vector2Int)> getSiteNeighbourhood(this Dictionary<Vector2Int, GameObject> _cell_grid, Vector2Int _max_size, Vector2Int _current_coord, bool _use_moore = true) {
        if (!_max_size.isCoordInGrid(_current_coord)) {throw new System.ArgumentException($"{_current_coord} is not a valid coordinate in the grid.");}
        if (_cell_grid.Count < 1) {throw new System.ArgumentException($"The cell grid is empty!"); }
        Vector2Int center = _current_coord;
        List<Vector2Int> neighbourhood_coords = _use_moore ? center.makeMooreNeighbourhoodCoordsAroundCenter() : center.makeVonNeumannNeighbourhoodCoordsAroundCenter();
        return neighbourhood_coords.Aggregate(new List<(GameObject,Vector2Int)>(),(_a,_b) => _max_size.isCoordInGrid(_b) ? _a.Concat(new[]{(_cell_grid[_b],_b)}).ToList() : _a );
    }

    public static List<Vector2Int> makeSquare(this Vector2Int _center, int _radius) {
        List<Vector2Int> ret_list = new List<Vector2Int>();
        for (int x = _center.x-Mathf.FloorToInt(_radius); x < _center.x+Mathf.FloorToInt(_radius); x++)
        {
            for (int y = _center.y-Mathf.FloorToInt(_radius); y < _center.y+Mathf.FloorToInt(_radius); y++)
            {
                ret_list.Add(new Vector2Int(x,y));
            }
        }
        return ret_list; 
    }

    public static List<Vector2Int> makeCircle(this Vector2Int _center, int _radius) => _center.makeSquare(_radius).Where(_p => Mathf.Sqrt((_p.x-_center.x)*(_p.x-_center.x)+(_p.y-_center.y)*(_p.y-_center.y))<_radius).ToList();

    public static List<(StreamlinedCell,Vector2Int)> getSiteNeighbourhood(this ConcurrentDictionary<Vector2Int, StreamlinedCell> _cell_grid, Vector2Int _max_size, Vector2Int _current_coord, bool _use_moore = true) {
        if (!_max_size.isCoordInGrid(_current_coord)) {throw new System.ArgumentException($"{_current_coord} is not a valid coordinate in the grid.");}
        if (_cell_grid.Count < 1) {throw new System.ArgumentException($"The cell grid is empty!"); }
        Vector2Int center = _current_coord;
        List<Vector2Int> neighbourhood_coords = _use_moore ? center.makeMooreNeighbourhoodCoordsAroundCenter() : center.makeVonNeumannNeighbourhoodCoordsAroundCenter();
        return neighbourhood_coords.Aggregate(new List<(StreamlinedCell,Vector2Int)>(),(_a,_b) => _max_size.isCoordInGrid(_b) ? _a.Concat(new[]{(_cell_grid[_b],_b)}).ToList() : _a );
    }

    //Any function that ends in TS like this is suppose to be thread safe.
    public static List<(StreamlinedCell,Vector2Int)> getSiteNeighbourhoodTS(this ConcurrentDictionary<Vector2Int, StreamlinedCell> _cell_grid, Vector2Int _max_size, Vector2Int _current_coord, int _top_most, bool _use_moore = true) {
        if (!_max_size.isCoordInGrid(_current_coord)) {throw new System.ArgumentException($"{_current_coord} is not a valid coordinate in the grid.");}
        if (_cell_grid.Count < 1) {throw new System.ArgumentException($"The cell grid is empty!"); }
        Vector2Int center = _current_coord;
        List<Vector2Int> neighbourhood_coords = _use_moore ? center.makeMooreNeighbourhoodCoordsAroundCenter() : center.makeVonNeumannNeighbourhoodCoordsAroundCenter();
        return neighbourhood_coords.Aggregate(new List<(StreamlinedCell,Vector2Int)>(),(_a,_b) => _max_size.isCoordInGrid(_b) ? _a.Concat(new[]{(_cell_grid[_b+new Vector2Int(0,_top_most)],_b+new Vector2Int(0,_top_most))}).ToList() : _a );
    }

    public static Maybe<(GameObject,Vector2Int)> getDirectionNeighbour(this Dictionary<Vector2Int, GameObject> _cell_grid, Vector2Int _max_size, Vector2Int _current_coord, DirectionTypes _direction) {
        if (!_max_size.isCoordInGrid(_current_coord)) {throw new System.ArgumentException($"{_current_coord} is not a valid coordinate in the grid.");}
        if (_cell_grid.Count < 1) {throw new System.ArgumentException($"The cell grid is empty!"); }
        Vector2Int center = _current_coord;
        Vector2Int new_coord = _direction switch {
            DirectionTypes.NORTH_WEST => new Vector2Int(center.x-1,center.y-1),
            DirectionTypes.NORTH => new Vector2Int(center.x,center.y-1),
            DirectionTypes.NORTH_EAST => new Vector2Int(center.x+1,center.y-1),
            DirectionTypes.EAST => new Vector2Int(center.x+1,center.y),
            DirectionTypes.SOUTH_EAST => new Vector2Int(center.x+1,center.y+1),
            DirectionTypes.SOUTH => new Vector2Int(center.x,center.y+1),
            DirectionTypes.SOUTH_WEST => new Vector2Int(center.x-1,center.y+1),
            DirectionTypes.WEST => new Vector2Int(center.x-1,center.y),
            _ => throw new System.ArgumentException()
        };
        return _max_size.isCoordInGrid(new_coord) ? new Maybe<(GameObject,Vector2Int)>((_cell_grid[new_coord],new_coord)) : new Maybe<(GameObject,Vector2Int)>();
    }

    public static Maybe<(StreamlinedCell,Vector2Int)> getDirectionNeighbour(this ConcurrentDictionary<Vector2Int, StreamlinedCell> _cell_grid, Vector2Int _max_size, Vector2Int _current_coord, DirectionTypes _direction) {
        if (!_max_size.isCoordInGrid(_current_coord)) {throw new System.ArgumentException($"{_current_coord} is not a valid coordinate in the grid.");}
        if (_cell_grid.Count < 1) {throw new System.ArgumentException($"The cell grid is empty!"); }
        Vector2Int center = _current_coord;
        Vector2Int new_coord = _direction switch {
            DirectionTypes.NORTH_WEST => new Vector2Int(center.x-1,center.y-1),
            DirectionTypes.NORTH => new Vector2Int(center.x,center.y-1),
            DirectionTypes.NORTH_EAST => new Vector2Int(center.x+1,center.y-1),
            DirectionTypes.EAST => new Vector2Int(center.x+1,center.y),
            DirectionTypes.SOUTH_EAST => new Vector2Int(center.x+1,center.y+1),
            DirectionTypes.SOUTH => new Vector2Int(center.x,center.y+1),
            DirectionTypes.SOUTH_WEST => new Vector2Int(center.x-1,center.y+1),
            DirectionTypes.WEST => new Vector2Int(center.x-1,center.y),
            _ => throw new System.ArgumentException()
        };
        return _max_size.isCoordInGrid(new_coord) ? new Maybe<(StreamlinedCell,Vector2Int)>((_cell_grid[new_coord],new_coord)) : new Maybe<(StreamlinedCell,Vector2Int)>();
    }

    public static Maybe<(StreamlinedCell,Vector2Int)> getDirectionNeighbourTS(this ConcurrentDictionary<Vector2Int, StreamlinedCell> _cell_grid, int _width, int _height, Vector2Int _current_coord, DirectionTypes _direction) {
        //Debug.Log(_max_size);
        //if (_cell_grid.Count < 1) {throw new System.ArgumentException($"The cell grid is empty!"); }


        Vector2Int max_size = new Vector2Int(_width,_height);
        if (!max_size.isCoordInGrid(_current_coord)) { Debug.Log($"{_current_coord} is not a valid coordinate in the grid."); throw new System.ArgumentException($"{_current_coord} is not a valid coordinate in the grid.");}

        Vector2Int center = _current_coord;
        Vector2Int new_coord = _direction switch {
            DirectionTypes.NORTH_WEST => new Vector2Int(center.x-1,center.y-1),
            DirectionTypes.NORTH => new Vector2Int(center.x,center.y-1),
            DirectionTypes.NORTH_EAST => new Vector2Int(center.x+1,center.y-1),
            DirectionTypes.EAST => new Vector2Int(center.x+1,center.y),
            DirectionTypes.SOUTH_EAST => new Vector2Int(center.x+1,center.y+1),
            DirectionTypes.SOUTH => new Vector2Int(center.x,center.y+1),
            DirectionTypes.SOUTH_WEST => new Vector2Int(center.x-1,center.y+1),
            DirectionTypes.WEST => new Vector2Int(center.x-1,center.y),
            _ => throw new System.ArgumentException()
        };
        if (max_size.isCoordInGrid(new_coord)) {
            _cell_grid.TryGetValue(new_coord, out StreamlinedCell cell);
            return new Maybe<(StreamlinedCell, Vector2Int)>((cell,new_coord));
            //return new Maybe<(StreamlinedCell, Vector2Int)>();
        } else {
            return new Maybe<(StreamlinedCell, Vector2Int)>();
        }
    }

    public static Ruleset getRulesFromConfig(this IngredientInteractionConfig _iic) {
        interpreter inter = new interpreter(_iic.getRuleInput());
        return inter.evaluate(); 
    }

    public static string reverseString(this string _string) {
        char[] array = new char[_string.Length];
        int forward = 0;
        for (int i = _string.Length - 1; i >= 0; i--)
        {
            array[forward++] = _string[i];
        }
        return new string(array);
    }

    public static CellData tryGetCellData(this GameObject _go) {
        if (_go.GetComponent<CellData>() == null) {
            throw new System.ArgumentException($"Gameobject {_go} has not got a CellData component!");
        }
        return _go.GetComponent<CellData>();
    }

    private static List<Ingredient> getIngredientFromNameAggrigateFunc( List<Ingredient> _list, (string,Ingredient) _arg) { List<Ingredient> list = _list; list.Add(_arg.Item2); return list;  } 

    public static Ingredient getIngredientFromName(this IngredientInteractionConfig _config, string _name) {
        List<(string, Ingredient)> name_ingredient_pair = new List<(string, Ingredient)>();
        _config.getIngredients().ForEach(_i => name_ingredient_pair.Add((_i.getIngredientName(),_i)));
        List<Ingredient> filtered_list = name_ingredient_pair.Where(_i => _i.Item1 == _name).ToList().Aggregate(new List<Ingredient>(),(_a,_b) => getIngredientFromNameAggrigateFunc(_a,_b));
        if (filtered_list.Count == 0) {
            throw new System.ArgumentException($"No ingredient registered in {_config.getConfigName()} config with a name of '{_name}'");
        }
        if (filtered_list.Count > 1) {
            throw new System.ArgumentException($"Multiple ingredients registered in {_config.getConfigName()} config with a name of '{_name}'. Names must be unique.");
        }
        return filtered_list[0];
    }

    public static List<(GameObject,Vector2Int)> filterCellsToOnlyRuleCenter (this Dictionary<Vector2Int, GameObject> _cell_grid, Rule _rule, SortingBiasOrderTypes _order_bias) {
        List<(GameObject,Vector2Int)> return_list = new List<(GameObject, Vector2Int)>();
        List<Vector2Int> ordered_keys = new List<Vector2Int>();
        switch(_order_bias) {
            case SortingBiasOrderTypes.TOP_LEFT_TO_BOTTOM_RIGHT: {
                ordered_keys = _cell_grid.Keys.ToList();
                break;
            }
            case SortingBiasOrderTypes.BOTTOM_RIGHT_TO_TOP_RIGHT: {
                ordered_keys = _cell_grid.Keys.ToList();
                ordered_keys.Reverse();
                break;
            }
            default: {
                throw new System.ArgumentException($"SortingBiasOrderTypes '{_order_bias}' not handled in filterCellsToOnlyRuleCenter!");
            }
        }
        ordered_keys.ForEach(_k => {if (_cell_grid[_k].tryGetCellData().getIngredient().getIngredientName() == _rule.active_site_type) {return_list.Add((_cell_grid[_k],_k));}});
        return return_list;
    } 


    public static List<(StreamlinedCell,Vector2Int)> filterCellToOnlyRuleCenter (this ConcurrentDictionary<Vector2Int, StreamlinedCell> _cell_grid, Rule _rule, SortingBiasOrderTypes _order_bias) {
        List<(StreamlinedCell,Vector2Int)> return_list = new List<(StreamlinedCell, Vector2Int)>();
        List<Vector2Int> ordered_keys = new List<Vector2Int>();
        switch(_order_bias) {
            case SortingBiasOrderTypes.TOP_LEFT_TO_BOTTOM_RIGHT: {
                ordered_keys = _cell_grid.Keys.ToList();
                break;
            }
            case SortingBiasOrderTypes.BOTTOM_RIGHT_TO_TOP_RIGHT: {
                ordered_keys = _cell_grid.Keys.ToList();
                ordered_keys.Reverse();
                break;
            }
            default: {
                throw new System.ArgumentException($"SortingBiasOrderTypes '{_order_bias}' not handled in filterCellsToOnlyRuleCenter!");
            }
        }
        ordered_keys.ForEach(_k => {if (_cell_grid[_k].ingredient.getIngredientName() == _rule.active_site_type) {return_list.Add((_cell_grid[_k],_k));}});
        return return_list;
    }



    public static Color getRandomColourFromList(this List<Color> _colours, System.Random _rand) => _colours[_rand.Next(0,_colours.Count)];

    public static List<(GameObject,Vector2Int)> filterCellsToRemoveExpended (this List<(GameObject,Vector2Int)> _list, List<GameObject> _expended) => _list.Where(_e => !_expended.Contains(_e.Item1)).ToList();
    public static List<(StreamlinedCell,Vector2Int)> filterCellsToRemoveExpended (this List<(StreamlinedCell,Vector2Int)> _list, List<Vector2Int> _expended) => _list.Where(_e => !_expended.Contains(_e.Item2)).ToList();

    public static List<(StreamlinedCell,Vector2Int)> filterCellsToRemoveExpendedTS (this List<(StreamlinedCell,Vector2Int)> _list, ConcurrentDictionary<Vector2Int, StreamlinedCell> _expended) {
        List<(StreamlinedCell, Vector2Int)> ret = new List<(StreamlinedCell, Vector2Int)>();
        foreach (var element in _list) {
            if (!_expended.TryGetValue(element.Item2, out StreamlinedCell cell)) {
                ret.Add(element);
            }
        }
        return ret;
        //return _list.Where(_e => !_expended.ContainsKey(_e.Item2)).ToList();
    } 
        

    //thread safe

    // public static List<(StreamlinedCell,Vector2Int)> filterCellToOnlyRuleCenterTS (this ConcurrentDictionary<Vector2Int, StreamlinedCell> _slice, Rule _rule, SortingBiasOrderTypes _order_bias) {
    //     List<(StreamlinedCell,Vector2Int)> return_list = new List<(StreamlinedCell, Vector2Int)>();
    //     List<Vector2Int> ordered_keys = new List<Vector2Int>();
        
    //     switch(_order_bias) {
    //         case SortingBiasOrderTypes.TOP_LEFT_TO_BOTTOM_RIGHT: {
    //             ordered_keys = _slice.Keys.ToList();
    //             break;
    //         }
    //         case SortingBiasOrderTypes.BOTTOM_RIGHT_TO_TOP_RIGHT: {
    //             ordered_keys = _slice.Keys.ToList();
    //             ordered_keys.Reverse();
    //             break;
    //         }
    //         default: {
    //             throw new System.ArgumentException($"SortingBiasOrderTypes '{_order_bias}' not handled in filterCellsToOnlyRuleCenter!");
    //         }
    //     }
    //     ordered_keys.ForEach(_k => {
    //         _slice.TryGetValue(_k, out StreamlinedCell cell);

    //         //Debug.Log(cell.ingredient.getIngredientName());
    //         if (cell.thread_ingredient.is_some) {
    //             if (cell.thread_ingredient.value.getIngredientName() == _rule.active_site_type) {
    //             return_list.Add((cell,_k));
    //             }
    //         }
    //     });
    //     return return_list;
    // }

}