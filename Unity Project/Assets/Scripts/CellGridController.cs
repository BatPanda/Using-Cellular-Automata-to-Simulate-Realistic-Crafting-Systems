using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DissertationTool.Assets.Scripts.Rules;
using DissertationTool.Assets.Scripts.DSL;
using TMPro;

using System.Diagnostics;

public class CellGridController : MonoBehaviour    //The old cell grid controller! Still in use in the demo scene!
{
    [Header("Grid Settings:")] 
    [SerializeField] private GameObject cellInstance;
    [SerializeField] private Vector2Int cellGridSize = new Vector2Int(3,3);
    [SerializeField] private Vector2Int cellPadding = new Vector2Int(0,0);
    [SerializeField] private float cellSize = 100;

    [Header("CA Settings")]
    [SerializeField] private TMP_InputField CellularStepRateField;
    [SerializeField] private Toggle CellularStepRatePaused;

    [Header("Ingredient Settings:")]
    [SerializeField] private IngredientInteractionConfig ingredientInteractionConfig;

    [Header("Aggregate Settings:")]
    [SerializeField] private DictSFUnityEvent process;

    [Header("Optional Settings:")]
    [SerializeField] private CellOverrideWidgetScript cellOverrideWidgetScript;

    private Dictionary<Vector2Int, GameObject> cell_grid;
    private Ruleset ca_rules;

    private System.Random rand;

    private int ca_steps = 0;

    Stopwatch stop_watch;


    // Start is called before the first frame update
    void Awake()
    {
        rand = new System.Random(Guid.NewGuid().GetHashCode());
        cell_grid = new Dictionary<Vector2Int, GameObject>();
        ca_rules = ingredientInteractionConfig.getRulesFromConfig();
        UnityEngine.Debug.Log("Language Evaluated");
        // ca_rules = new Ruleset(new Dictionary<string, int>(){{"air",0},{"sand",1}}, new Dictionary<string, int>() {}, new List<Rule>() {
        //     {
        //         new Rule("sand",RuleDetectionType.WITH,new Maybe<DirectionTypes>(DirectionTypes.SOUTH),"air",DirectionTypes.SOUTH)
        //     }, 
        //     {
        //         new Rule("sand",RuleDetectionType.WITH,new Maybe<DirectionTypes>(DirectionTypes.SOUTH_EAST),"air",DirectionTypes.SOUTH_EAST)
        //     },
        //     {
        //         new Rule("sand",RuleDetectionType.WITH,new Maybe<DirectionTypes>(DirectionTypes.SOUTH_WEST),"air",DirectionTypes.SOUTH_WEST)
        //     }
        // });
    }

    void Start() {
        stop_watch = new Stopwatch();
        StartCoroutine(doCAStep());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void destroyGrid() {
        foreach(GameObject element in cell_grid.Select(_e => _e.Value)) {
            Destroy(element);
        }
    }
    public void generateGrid() {
        destroyGrid();
        float x_offset = (((cellSize*cellGridSize.x)/2)-cellSize/2)+cellPadding.x;
        float y_offset = (((cellSize*cellGridSize.y)/2)-cellSize/2)+cellPadding.y;
        for (int y = 0; y < cellGridSize.y; y++)
        {
            for (int x = 0; x < cellGridSize.x; x++)
            {
                GameObject cell_go = Instantiate(cellInstance, new Vector3(
                    (x*(cellSize+cellPadding.x))-x_offset,
                    ((-y)*(cellSize+cellPadding.y))+y_offset,
                    0),
                    Quaternion.identity).GetComponent<CellData>()
                        .setupCell(x,y,cellSize,ingredientInteractionConfig.getDefaultIngredient(), cellOverrideWidgetScript,this,rand);
                cell_go.transform.SetParent(gameObject.transform,false);
                cell_grid[new Vector2Int(x,y)] = cell_go;
            }
        }

        //List<(GameObject,Vector2Int)> test = cell_grid.getSiteNeighbourhood(cellGridSize,new Vector2Int(0,0));
        //Debug.Log(test.Aggregate("",(_a,_b) => _a+"x:"+_b.Item1.GetComponent<CellData>().grid_x_pos+" y:"+_b.Item1.GetComponent<CellData>().grid_y_pos+", "));
    }

    public void generateGrid(SerializableDictionary<Vector2Int,Ingredient> _stored_cell_grid) {
        destroyGrid();
        float x_offset = (((cellSize*cellGridSize.x)/2)-cellSize/2)+cellPadding.x;
        float y_offset = (((cellSize*cellGridSize.y)/2)-cellSize/2)+cellPadding.y;
        for (int y = 0; y < cellGridSize.y; y++)
        {
            for (int x = 0; x < cellGridSize.x; x++)
            {
                GameObject cell_go = Instantiate(cellInstance, new Vector3(
                    (x*(cellSize+cellPadding.x))-x_offset,
                    ((-y)*(cellSize+cellPadding.y))+y_offset,
                    0),
                    Quaternion.identity).GetComponent<CellData>()
                        .setupCell(x,y,cellSize, _stored_cell_grid[new Vector2Int(x,y)], cellOverrideWidgetScript,this,rand);
                cell_go.transform.SetParent(gameObject.transform,false);
                cell_grid[new Vector2Int(x,y)] = cell_go;
            }
        }
    }

    public void storeCurrentCAState() {
        Dictionary<Vector2Int,Ingredient> ingers = cell_grid.ToDictionary(_p => _p.Key, _p => _p.Value.tryGetCellData().getIngredient());
        SerializableDictionary<Vector2Int,Ingredient> singers = new SerializableDictionary<Vector2Int, Ingredient>();
        ingers.Keys.ToList().ForEach(_k => singers[_k] = ingers[_k]);

        ingredientInteractionConfig.setInternalState(cellGridSize,cellPadding,cellSize,CellularStepRateField.text,CellularStepRatePaused.isOn,singers);
        UnityEngine.Debug.Log("CA state saved");
    }

    public void loadSavedCAState() {
        if (!ingredientInteractionConfig.has_stored) {return;}
        destroyGrid();
        cellGridSize = ingredientInteractionConfig.stored_cell_grid_size;
        cellPadding = ingredientInteractionConfig.stored_cell_padding;
        cellSize = ingredientInteractionConfig.stored_cell_size;
        CellularStepRateField.text = ingredientInteractionConfig.stored_cell_step_rate_field_text;
        CellularStepRatePaused.isOn = ingredientInteractionConfig.stored_cellular_step_rate_paused;
        generateGrid(ingredientInteractionConfig.stored_cell_grid);

        UnityEngine.Debug.Log("CA state loaded");
    }

    private List<GameObject> executeCARule(Rule _r, (GameObject,Vector2Int) _cell) {
        switch(_r.resolution_type) {
            case RuleResolutionType.UPDATE_SELF: {
                _cell.Item1.tryGetCellData().setNewState(ingredientInteractionConfig.getIngredientFromName(_r.new_type.value));
                return new List<GameObject>() {_cell.Item1};
            }
            case RuleResolutionType.SWAP: {
                Maybe<(GameObject,Vector2Int)> nd = cell_grid.getDirectionNeighbour(cellGridSize,_cell.Item2,_r.swap_direction.value);
                if (nd.is_some) {
                    Ingredient original_1 = _cell.Item1.tryGetCellData().getIngredient();
                    Ingredient original_2= nd.value.Item1.tryGetCellData().getIngredient();

                    _cell.Item1.tryGetCellData().setNewState(original_2);
                    nd.value.Item1.tryGetCellData().setNewState(original_1);

                    //Debug.Log($"{ca_steps} | Swapped {_cell} with coord {_cell.Item2.x},{_cell.Item2.y}");

                    return new List<GameObject>() {_cell.Item1, nd.value.Item1};
                }
                return new List<GameObject>();
            }
            default: { throw new ArgumentException($"RuleResolutionType {_r.resolution_type} has not been added to executeCARule!");}
        }
    }

    public void stepCA() {
        //Debug.Log($"{ca_steps} | CA STEPPING");
        List<GameObject> expended_sites = new List<GameObject>();
        ca_rules.rules.ForEach(_r => {
            List<(GameObject,Vector2Int)> valid_rule_center_cells = cell_grid.filterCellsToOnlyRuleCenter(_r,ingredientInteractionConfig.getSortingBias()).filterCellsToRemoveExpended(expended_sites);
            valid_rule_center_cells.ForEach(_c => {
                Ingredient center_cell_ingredient = _c.Item1.tryGetCellData().getIngredient();
                List<(GameObject,Vector2Int)> neighbourhood = cell_grid.getSiteNeighbourhood(cellGridSize,_c.Item2);//.filterCellsToRemoveExpended(expended_sites);
                if (_r.chance > ca_rules.getNextPercentileNumber()) {
                    switch(_r.detection_type) {
                        case RuleDetectionType.AROUND: {
                            bool any_true = false;
                            neighbourhood.ForEach(_n => {if (_n.Item1.tryGetCellData().getIngredient().getIngredientName() == _r.target_site_type.value) {any_true = true;}});
                            if (any_true) {
                                executeCARule(_r, _c).ForEach(_e => expended_sites.Add(_e));
                            }
                            break;
                        }
                        case RuleDetectionType.SURROUND: {
                            bool all_true = true;
                            neighbourhood.ForEach(_n => {if (_n.Item1.tryGetCellData().getIngredient().getIngredientName() != _r.target_site_type.value) {all_true = false;}});
                            if (all_true) {
                                executeCARule(_r, _c).ForEach(_e => expended_sites.Add(_e));
                            }
                            break;
                        }
                        case RuleDetectionType.WITH: {
                            DirectionTypes dir = _r.detection_with_direction.is_some ? _r.detection_with_direction.value : throw new System.ArgumentException($"With detection type used with no direction. {_r.active_site_type}");
                            Maybe<(GameObject,Vector2Int)> nd = cell_grid.getDirectionNeighbour(cellGridSize,_c.Item2,dir);
                            if (nd.is_some && nd.value.Item1.tryGetCellData().getIngredient().getIngredientName() == _r.target_site_type.value) {
                                executeCARule(_r, _c).ForEach(_e => expended_sites.Add(_e));
                            }
                            break;
                        }
                        case RuleDetectionType.GROUP: {
                            executeCARule(_r,_c).ForEach(_e => expended_sites.Add(_e));
                            break;
                        }
                        default: {throw new System.ArgumentException();}
                    }
                }
            });
        });
    }

    public void setCell(Vector2Int _coord, string _new_type) {
        if (cellGridSize.isCoordInGrid(_coord)) {
            cell_grid[_coord].tryGetCellData().setNewState(ingredientInteractionConfig.getIngredientFromName(_new_type));
        }
    }

    public float getFadeTime() => ((float)Convert.ToDouble(CellularStepRateField.text))*0.8f;

    public IEnumerator doCAStep() {
        while (true) {
            if (!CellularStepRatePaused.isOn) {
                if (ca_steps == 0) {
                    stop_watch.Start();
                }
                stepCA();
                ca_steps+=1;
            }
            if (ca_steps == 100 ) {
                stop_watch.Stop();
                UnityEngine.Debug.Log($"Time taken: {stop_watch.ElapsedMilliseconds}");
            }
            yield return new WaitForSeconds((float)Convert.ToDouble(CellularStepRateField.text));
        }
    }

    public void processCA() {
        Dictionary<string,float> ret = new Dictionary<string, float>();

        cell_grid.Keys.ToList().ForEach(_l => {
            GameObject go = cell_grid[_l];
            CellData cd = go.tryGetCellData();
            Ingredient ing = cd.getIngredient();
            Dictionary<string,float> properties = ing.getProperties();
            properties.Keys.ToList().ForEach(_k => {
                float val = properties[_k];
                if (ret.ContainsKey(_k)) {
                    ret[_k] += val;
                } else {
                    ret[_k] = val;
                }
            });
        });

        process?.Invoke(ret);
    }
}