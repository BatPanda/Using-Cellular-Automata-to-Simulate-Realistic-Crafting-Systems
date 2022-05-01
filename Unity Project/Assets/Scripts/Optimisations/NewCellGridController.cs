using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DissertationTool.Assets.Scripts.Rules;
using DissertationTool.Assets.Scripts.DSL;
using TMPro;

using System.Diagnostics;

public enum CARunMode {
    NORMAL,
    //THREADED,
    SMART
}

public class NewCellGridController : MonoBehaviour //The new cell grid controller is the optimsed version of the cell grid controller that uses a render target and compute shader. It also uses the smart method and has the remaines of the threaded method inside it.
{
    [Header("Grid Settings:")] 
    [SerializeField] private Vector2Int cellGridSize = new Vector2Int(3,3);

    [SerializeField] private int cellResolution = 1024;

    [SerializeField] private RawImage renderTarget;

    [SerializeField] private ComputeShader computeShader; 
    [SerializeField] private CARunMode runMode = CARunMode.SMART;
    
    // [Range(1,16)]
    // [SerializeField] private int numberOfUpdateThreads = 4; //Most of the threaded content was left here if you wish to uncomment it and try it out. I will warn you that while it will work, it is very strange.


    private RenderTexture render_texture;

    [Header("CA Settings")]
    [SerializeField] private float stepRate;
    [SerializeField] private bool stepRatePaused;

    [SerializeField] private bool useMooreMethod = true;

    [Header("Ingredient Settings:")]
    [SerializeField] private IngredientInteractionConfig ingredientInteractionConfig;

    [Header("Aggregate Settings:")]
    [SerializeField] private DictSFUnityEvent process;

    private ConcurrentDictionary<Vector2Int, StreamlinedCell> cell_grid;

    //private ConcurrentDictionary<Vector2Int,StreamlinedCell> thread_expended_sites;
    private Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>> old_cell_grid;
    private Ruleset ca_rules;

    private HashSet<StreamlinedCell> smart_cells;
    private bool smart_redraw = false;

    private System.Random rand;

    private int ca_steps = 0;

    Stopwatch stop_watch;

    void Awake()
    {
        rand = new System.Random(Guid.NewGuid().GetHashCode());
        cell_grid = new ConcurrentDictionary<Vector2Int, StreamlinedCell>();
        old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>();
        smart_cells = new HashSet<StreamlinedCell>();
        //thread_expended_sites = new ConcurrentDictionary<Vector2Int, StreamlinedCell>();
        ca_rules = ingredientInteractionConfig.getRulesFromConfig();
        UnityEngine.Debug.Log("Language Evaluated");
    }

    // Start is called before the first frame update
    void Start()
    {
        generateGrid();

        stop_watch = new Stopwatch();
        
        render_texture = new RenderTexture(cellResolution,cellResolution,24);
        render_texture.format = RenderTextureFormat.ARGB32;
        render_texture.enableRandomWrite = true;
        render_texture.Create();

        computeShader.SetTexture(0, "Result", render_texture);
        computeShader.SetFloat("Length",(float)cellGridSize.x);
        computeShader.SetFloat("Width",(float)cellGridSize.y);
        computeShader.SetFloat("Resolution",(float)cellResolution);

        //StartCoroutine(doCAStep());
        renderTarget.texture = render_texture;

        smart_redraw = true;
        updateRT();
        
        StartCoroutine(doCAStep());
    }

    private void updateRT() {


        Vector4[] colours = getColourListFromCells();
        int[] should_update = getShouldUpdateListFromCells();

        ComputeBuffer colour_buffer = new ComputeBuffer(colours.Length,sizeof(float)*4);
        colour_buffer.SetData(colours);

        ComputeBuffer update_buffer = new ComputeBuffer(should_update.Length,sizeof(int));
        update_buffer.SetData(should_update);

        computeShader.SetBuffer(0, "colour_buffer", colour_buffer);
        computeShader.SetBuffer(0, "update_buffer", update_buffer);

        computeShader.Dispatch(0,render_texture.width/8,render_texture.height/8,1);

        colour_buffer.Release();
        update_buffer.Release();
    }

    public IngredientInteractionConfig getInteractionConfig() => ingredientInteractionConfig;

    public void clearGrid() {
        smart_cells.Clear();
        smart_redraw = true;
        old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>();
        generateGrid();
        updateRT();
        ca_steps = 0;
    }

    public void generateGrid() {
        cell_grid.Clear();
        for (int y = 0; y < cellGridSize.y; y++)
        {
            for (int x = 0; x < cellGridSize.y; x++)
            {
                cell_grid[new Vector2Int(x,y)] = new StreamlinedCell(rand,x,y,ingredientInteractionConfig.getDefaultIngredient());
            }
        }
    }

    private Vector4[] getColourListFromCells() {
        Vector4[] colours = new Vector4[cellGridSize.x*cellGridSize.y];
        
        if (runMode == CARunMode.SMART && !smart_redraw) {
            smart_cells.ToList().ForEach(_e => {
                colours[cellGridSize.x*_e.grid_x_pos+_e.grid_y_pos] = _e.colour;
            });
        } else {
            cell_grid.ToList().ForEach(_e => {
                colours[cellGridSize.x*_e.Key.x+_e.Key.y] = _e.Value.colour;
            });
        }


        return colours;
    }

    private int[] getShouldUpdateListFromCells() {
        int size = cellGridSize.x*cellGridSize.y;
        int[] ret = new int[size];

        if (runMode == CARunMode.SMART) {
            int base_val = smart_redraw ? 1 : 0;
            for (int i = 0; i < size; i++)
            {
                ret[i] = base_val;
            }
            smart_redraw = false;
            smart_cells.ToList().ForEach(_e => {
                ret[cellGridSize.x*_e.grid_x_pos+_e.grid_y_pos] = 1;
            });
        } else {
            if (old_cell_grid.is_some) {
                ConcurrentDictionary<Vector2Int,StreamlinedCell> oc = old_cell_grid.value;
                oc.ToList().ForEach(_e => {
                    ret[cellGridSize.x*_e.Key.x+_e.Key.y] = (oc[_e.Key].ingredient.getIngredientName().Equals(cell_grid[_e.Key].ingredient.getIngredientName())) ? 0 : 1; 
                });
            } else {
                for (int i = 0; i < size; i++)
                {
                    ret[i] = 1;
                }
            }
        }
        return ret;
    }

    public void storeCurrentCAState() {
        throw new NotImplementedException();
    }

    public void loadSavedCAState() {
        throw new NotImplementedException();
    }

    public bool isCAStepPaused() => stepRatePaused;
    public void setCAStepPaused(bool _paused) { stepRatePaused = _paused; }

    public float getCAStepRate() => stepRate;
    public void setCAStepRate(float _new_rate) { stepRate = _new_rate; }

    public int getRTResolution() => cellResolution;
    public Vector2Int getCellLengthAndHeight() => cellGridSize;


    public IEnumerator doCAStep() {
        while (true) {
            if (!stepRatePaused) {
                // if (ca_steps == 1) { //Used for time tests
                //     stop_watch.Start();
                // }
                // if (runMode == CARunMode.THREADED) {
                //     //stepCAWithThreads(); 
                // }
                if (runMode == CARunMode.NORMAL) {
                    ConcurrentDictionary<Vector2Int,StreamlinedCell> test = new ConcurrentDictionary<Vector2Int, StreamlinedCell>();
                    cell_grid.ToList().ForEach(_e => test[_e.Key] = new StreamlinedCell(_e.Value));
                    old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>(test);
                    stepCA();
                } else if (runMode == CARunMode.SMART) {
                    stepCASmart();
                    //UnityEngine.Debug.Log(smart_cells.Count);
                }
                updateRT();
                // if (ca_steps == 101) {
                //     stop_watch.Stop();
                //     UnityEngine.Debug.Log($"Time taken: {stop_watch.ElapsedMilliseconds}");
                // }
                ca_steps+=1;
            }
            yield return new WaitForSeconds(stepRate);
        }
    }

    public void setCell(Vector2Int _coord, string _new_type) {
        if (cellGridSize.isCoordInGrid(_coord)) {
            cell_grid[_coord].setNewState(ingredientInteractionConfig.getIngredientFromName(_new_type));
            makeDirty(cell_grid[_coord]);
            old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>();
            //smart_redraw = true;
            updateRT();
        }
    }

    public void setCellNonDestructive(Vector2Int _coord, string _new_type) {
        if (cellGridSize.isCoordInGrid(_coord) && cell_grid[_coord].ingredient.getIngredientName() == ingredientInteractionConfig.getDefaultIngredient().getIngredientName()) {
            cell_grid[_coord].setNewState(ingredientInteractionConfig.getIngredientFromName(_new_type));
            makeDirty(cell_grid[_coord]);
            old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>();
            //smart_redraw = true;
            updateRT();
        }
    }

    public void setCells(List<Vector2Int> _cells, string _new_type) {
        _cells.ForEach(_e => {
            if (cellGridSize.isCoordInGrid(_e)) {
                cell_grid[_e].setNewState(ingredientInteractionConfig.getIngredientFromName(_new_type));
                makeDirty(cell_grid[_e]);
            }
        });
        old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>();
        //smart_redraw = true;
        updateRT();
    }

    public void setCellsNonDestructive(List<Vector2Int> _cells, string _new_type) {
        _cells.ForEach(_e => {
            if (cellGridSize.isCoordInGrid(_e) && cell_grid[_e].ingredient.getIngredientName() == ingredientInteractionConfig.getDefaultIngredient().getIngredientName()) {
                cell_grid[_e].setNewState(ingredientInteractionConfig.getIngredientFromName(_new_type));
                makeDirty(cell_grid[_e]);
            }
        });
        old_cell_grid = new Maybe<ConcurrentDictionary<Vector2Int, StreamlinedCell>>();
        //smart_redraw = true;
        updateRT();
    }

    public void processCA() { //The function used to create the data for the process scripts
         Dictionary<string,float> ret = new Dictionary<string, float>();

        cell_grid.Keys.ToList().ForEach(_l => {
            StreamlinedCell sc = cell_grid[_l];
            Ingredient ing = sc.ingredient;
            Dictionary<string,float> properties = ing.getProperties();
            properties.Keys.ToList().ForEach(_k => {
                string lower_key = _k.ToLower();
                float val = properties[_k];
                if (ret.ContainsKey(lower_key)) {
                    ret[lower_key] += val;
                } else {
                    ret[lower_key] = val;
                }
            });
        });

        process?.Invoke(ret);
    }

    // public void stepCAWithThreads() { //RIP threaded method.
    //     ThreadPool.SetMinThreads(1,1);
    //     ThreadPool.SetMaxThreads(numberOfUpdateThreads,1);
        
    //     //current_jobs = numberOfUpdateThreads;
    //     ManualResetEvent[] tasks = new ManualResetEvent[numberOfUpdateThreads];

    //     thread_expended_sites.Clear();



    //     for (int i = 0; i < numberOfUpdateThreads; i++)
    //     {
    //         tasks[i] = new ManualResetEvent(false);
    //         //Debug.Log(2*i*threadSliceHeight + " <>");
    //         ThreadPool.QueueUserWorkItem(new WaitCallback(ProccessSliceCallback(tasks[i],2*i*threadSliceHeight,(2*i+1)*threadSliceHeight-1,ingredientInteractionConfig.getSortingBias())));
    //     }

    //     WaitHandle.WaitAll(tasks);

    //     tasks = new ManualResetEvent[numberOfUpdateThreads];

    //     //Debug.Log("Finished First Set of Slices");

    //     //current_jobs = numberOfUpdateThreads;

    //     for (int i = 0; i < numberOfUpdateThreads; i++)
    //     {
    //         //Debug.Log(2*i*threadSliceHeight+threadSliceHeight-1);
    //         tasks[i] = new ManualResetEvent(false);
    //         ThreadPool.QueueUserWorkItem(new WaitCallback(ProccessSliceCallback(tasks[i],
    //             2*i*threadSliceHeight+threadSliceHeight-1,
    //             Mathf.Min((2*i+1)*threadSliceHeight-1+threadSliceHeight+1 , cellGridSize.y),
    //             ingredientInteractionConfig.getSortingBias())));
    //         }

    //     WaitHandle.WaitAll(tasks);
        
    //     //Debug.Log("Finished Second Set of Slices");
    // }


    // public Action<object> ProccessSliceCallback(ManualResetEvent _mre, int _top_most, int _bottom_most, SortingBiasOrderTypes _sort_type) {
    //     //Debug.Log($"true start!");
    //     Ruleset ca_rule_copy = new Ruleset(ca_rules);
    //     int width = cellGridSize.x;
    //     int height = cellGridSize.y;
    //     int slice_height = _bottom_most-_top_most;
    //     bool use_moore = useMooreMethod;

    //     int current_step = ca_steps;

    //     ConcurrentDictionary<string,Ingredient> safe_name_to_ingredient = new ConcurrentDictionary<string, Ingredient>();
    //     ingredientInteractionConfig.getIngredients().ForEach(_e => {
    //         safe_name_to_ingredient[_e.getIngredientName()] = _e; 
    //     });

    //     ConcurrentDictionary<Vector2Int, StreamlinedCell> slice_cells = new ConcurrentDictionary<Vector2Int, StreamlinedCell>();
        
    //     for (int y = _top_most; y < _bottom_most; y++)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             cell_grid.TryGetValue(new Vector2Int(x,y),out StreamlinedCell val);
    //             //Debug.Log($"{x},{y} | cell: {val}");
    //             slice_cells.TryAdd(new Vector2Int(x,y),new StreamlinedCell(val));
    //         }
    //     }

    //     void ProcessSlice (object _obj) {
            
            
    //         //Debug.Log($"Test {Thread.CurrentThread}");

    //         ca_rule_copy.rules.ForEach(_r => {
    //             List<(StreamlinedCell,Vector2Int)> valid_rule_center_cells = slice_cells.filterCellToOnlyRuleCenterTS(_r,_sort_type).filterCellsToRemoveExpendedTS(thread_expended_sites);
    //             valid_rule_center_cells.ForEach(_c => {
    //                 //Debug.Log($"it begins!");
    //                 IngredientTS ingredient = _c.Item1.thread_ingredient.value;
    //                 List<(StreamlinedCell, Vector2Int)> neighbourhood = slice_cells.getSiteNeighbourhoodTS(new Vector2Int(width,slice_height),_c.Item2-new Vector2Int(0,_top_most),_top_most,use_moore);// <- the problem
    //                // if (current_step > 1) {_mre.Set();}
    //                 if (_r.chance > ca_rule_copy.getNextPercentileNumber()) {
    //                     switch(_r.detection_type) {
    //                         case RuleDetectionType.AROUND: {
    //                             bool any_true = false;
    //                             neighbourhood.ForEach(_n => {if (_n.Item1.thread_ingredient.value.getIngredientName() == _r.target_site_type.value) {any_true = true;}});
    //                             if (any_true) {
    //                                 executeCARuleTS(_r, _c, safe_name_to_ingredient,width,slice_height,slice_cells,new Maybe<(StreamlinedCell, Vector2Int)>()).ForEach(_e => thread_expended_sites[_e] = slice_cells[_e]);
    //                             }
    //                             break;
    //                         }
    //                         case RuleDetectionType.SURROUND: {
    //                             bool all_true = true;
    //                             neighbourhood.ForEach(_n => {if (_n.Item1.thread_ingredient.value.getIngredientName() != _r.target_site_type.value) {all_true = false;}});
    //                             if (all_true) {
    //                                 //Debug.Log()
    //                                 executeCARuleTS(_r, _c,safe_name_to_ingredient,width,slice_height,slice_cells,new Maybe<(StreamlinedCell, Vector2Int)>()).ForEach(_e => thread_expended_sites[_e] = slice_cells[_e]);
    //                             }
    //                             break;
    //                         }
    //                         case RuleDetectionType.WITH: {
    //                             DirectionTypes dir = _r.detection_with_direction.is_some ? _r.detection_with_direction.value : throw new System.ArgumentException($"With detection type used with no direction. {_r.active_site_type}");
    //                             //Debug.Log("uh oh");
    //                             Maybe<(StreamlinedCell,Vector2Int)> nd = slice_cells.getDirectionNeighbourTS(width,slice_height,_c.Item2-new Vector2Int(0,_top_most),dir).fmap(_e => (_e.Item1,_e.Item2+new Vector2Int(0,_top_most)));
    //                             if (nd.is_some && nd.value.Item1.thread_ingredient.value.getIngredientName() == _r.target_site_type.value) {
    //                                 //Debug.Log("we reach here");
    //                                 executeCARuleTS(_r, _c, safe_name_to_ingredient,width,slice_height, slice_cells,nd).ForEach(_e => thread_expended_sites[_e] = slice_cells[_e]);
    //                             } else if (!nd.is_some) {
    //                                 //Debug.Log("Whyine");
    //                             }
    //                             break;
    //                         }
    //                         case RuleDetectionType.GROUP: {
    //                             executeCARuleTS(_r,_c,safe_name_to_ingredient,width,slice_height,slice_cells,new Maybe<(StreamlinedCell, Vector2Int)>()).ForEach(_e => thread_expended_sites[_e] = slice_cells[_e]);
    //                             break;
    //                         }
    //                         default: {throw new System.ArgumentException();}
    //                     }
    //                 }
    //             });
    //         });

    //         if (_top_most <= 1 && 1 <= _bottom_most) {
    //             if (slice_cells[new Vector2Int(1,1)].thread_ingredient.value.getIngredientName() == "sand") {
    //                 Debug.Log("it moved");
    //             }
    //         }

    //         if (_top_most <= 1 && 1 <= _bottom_most) {
    //             foreach (var _e in slice_cells)
    //             {
    //                 cell_grid.AddOrUpdate(_e.Key,_e.Value, (_a,_b) => _a.Equals(_e.Key) ? _e.Value : _b);
    //             }
    //         }

    //         _mre.Set();

    //     }
    //     return ProcessSlice;
    // }

    public void stepCA() { //The bog standard CA step algorithm!
        //A list that contains cells locations that have already been updated
        List<Vector2Int> expended_sites = new List<Vector2Int>();

        ca_rules.rules.ForEach(_r => {
            //For each rule, 
            //Get a list of the center cells for the rules.
            List<(StreamlinedCell,Vector2Int)> valid_rule_center_cells = cell_grid.filterCellToOnlyRuleCenter(_r,ingredientInteractionConfig.getSortingBias()).filterCellsToRemoveExpended(expended_sites);

            valid_rule_center_cells.ForEach(_c => {
                //For each center cell,
                //Get its ingredient and its neighbourhood.
                Ingredient center_cell_ingredient = _c.Item1.ingredient;
                List<(StreamlinedCell,Vector2Int)> neighbourhood = cell_grid.getSiteNeighbourhood(cellGridSize,_c.Item2, useMooreMethod);

                //If next rule is able to run, do so. (Some rules are chance based and may skip execution here)
                if (_r.chance > ca_rules.getNextPercentileNumber()) {
                    switch(_r.detection_type) {
                        case RuleDetectionType.AROUND: {
                            bool any_true = false;
                            neighbourhood.ForEach(_n => {if (_n.Item1.ingredient.getIngredientName() == _r.target_site_type.value) {any_true = true;}});
                            if (any_true) {
                                executeCARule(_r, _c).ForEach(_e => expended_sites.Add(_e));
                            }
                            break;
                        }
                        case RuleDetectionType.SURROUND: {
                            bool all_true = true;
                            neighbourhood.ForEach(_n => {if (_n.Item1.ingredient.getIngredientName() != _r.target_site_type.value) {all_true = false;}});
                            if (all_true) {
                                executeCARule(_r, _c).ForEach(_e => expended_sites.Add(_e));
                            }
                            break;
                        }
                        case RuleDetectionType.WITH: {
                            DirectionTypes dir = _r.detection_with_direction.is_some ? _r.detection_with_direction.value : throw new System.ArgumentException($"With detection type used with no direction. {_r.active_site_type}");
                            Maybe<(StreamlinedCell,Vector2Int)> nd = cell_grid.getDirectionNeighbour(cellGridSize,_c.Item2,dir);
                            if (nd.is_some && nd.value.Item1.ingredient.getIngredientName() == _r.target_site_type.value) {
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



    private List<Vector2Int> executeCARule(Rule _r, (StreamlinedCell,Vector2Int) _cell) {
        switch(_r.resolution_type) {
            case RuleResolutionType.UPDATE_SELF: {
                _cell.Item1.setNewState(ingredientInteractionConfig.getIngredientFromName(_r.new_type.value));
                return new List<Vector2Int>() {_cell.Item2};
            }
            case RuleResolutionType.SWAP: {
                Maybe<(StreamlinedCell,Vector2Int)> nd = cell_grid.getDirectionNeighbour(cellGridSize,_cell.Item2,_r.swap_direction.value);
                if (nd.is_some) {
                    Ingredient original_1 = _cell.Item1.ingredient;
                    Ingredient original_2= nd.value.Item1.ingredient;

                    _cell.Item1.setNewState(original_2);
                    nd.value.Item1.setNewState(original_1);

                    return new List<Vector2Int>() {_cell.Item2, nd.value.Item2};
                }
                return new List<Vector2Int>();
            }
            default: { throw new ArgumentException($"RuleResolutionType {_r.resolution_type} has not been added to executeCARule!");}
        }
    }


    //Thread safe execute
    private List<Vector2Int> executeCARuleTS(Rule _r, (StreamlinedCell,Vector2Int) _cell, ConcurrentDictionary<string,Ingredient> _safe_names, int _width, int _height, ConcurrentDictionary<Vector2Int,StreamlinedCell> _slice, Maybe<(StreamlinedCell,Vector2Int)> _nd) {
        switch(_r.resolution_type) {
            case RuleResolutionType.UPDATE_SELF: {
               // Debug.Log("Test2");
                _cell.Item1.setNewState(_safe_names[_r.new_type.value]);
                return new List<Vector2Int>() {_cell.Item2};
            }
            case RuleResolutionType.SWAP: {
                //Debug.Log("Test3");
                if (_nd.is_some) {
                    Ingredient original_1 = _cell.Item1.ingredient;
                    Ingredient original_2= _nd.value.Item1.ingredient;

                    _cell.Item1.setNewState(original_2);
                    _nd.value.Item1.setNewState(original_1);

                    return new List<Vector2Int>() {_cell.Item2, _nd.value.Item2};
                }

                return new List<Vector2Int>();
            }
            default: { throw new ArgumentException($"RuleResolutionType {_r.resolution_type} has not been added to executeCARule!");}
        }
    }


    //private int threadSliceHeight { get => ((cellGridSize.y/numberOfUpdateThreads)/2)+1; }


    //The smart ca step algorithm 
    private void stepCASmart() {
        List<Vector2Int> expended_sites = new List<Vector2Int>();
        HashSet<StreamlinedCell> smart_buffer = new HashSet<StreamlinedCell>();
        smart_cells.ToList().ForEach(_c => smart_buffer.Add(_c));

        smart_cells.ToList().ForEach(_c => {
            smart_buffer.Remove(_c);
            ca_rules.rules.ForEach(_r => {
                if (_c.ingredient.getIngredientName() == _r.active_site_type && !expended_sites.Contains(_c.getGridPos())) {
                    Ingredient center_cell_ingredient = _c.ingredient;
                    List<(StreamlinedCell,Vector2Int)> neighbourhood = cell_grid.getSiteNeighbourhood(cellGridSize,_c.getGridPos(), useMooreMethod);

                    switch(_r.detection_type) {
                        case RuleDetectionType.AROUND: {
                            bool any_true = false;
                            neighbourhood.ForEach(_n => {if (_n.Item1.ingredient.getIngredientName() == _r.target_site_type.value) {any_true = true;}});
                            if (any_true) {
                                if (_r.chance > ca_rules.getNextPercentileNumber()) {
                                    executeCARule(_r, (_c,_c.getGridPos())).ForEach(_e => {expended_sites.Add(_e); });
                                }
                                else {
                                    smart_buffer.Add(_c);
                                }
                            }
                            break;
                        }
                        case RuleDetectionType.SURROUND: {
                            bool all_true = true;
                            neighbourhood.ForEach(_n => {if (_n.Item1.ingredient.getIngredientName() != _r.target_site_type.value) {all_true = false;}});
                            if (all_true) {
                                if (_r.chance > ca_rules.getNextPercentileNumber()) {
                                    executeCARule(_r, (_c,_c.getGridPos())).ForEach(_e => {expended_sites.Add(_e); });
                                }
                                else {
                                    smart_buffer.Add(_c);
                                }
                            }
                            break;
                        }
                        case RuleDetectionType.WITH: {
                            DirectionTypes dir = _r.detection_with_direction.is_some ? _r.detection_with_direction.value : throw new System.ArgumentException($"With detection type used with no direction. {_r.active_site_type}");
                            Maybe<(StreamlinedCell,Vector2Int)> nd = cell_grid.getDirectionNeighbour(cellGridSize,_c.getGridPos(),dir);
                            if (nd.is_some && nd.value.Item1.ingredient.getIngredientName() == _r.target_site_type.value) {
                                if (_r.chance > ca_rules.getNextPercentileNumber()) {
                                    executeCARule(_r, (_c,_c.getGridPos())).ForEach(_e => {expended_sites.Add(_e); });
                                }
                                else {
                                    smart_buffer.Add(_c);
                                }
                            }
                            break;
                        }
                        case RuleDetectionType.GROUP: {
                            if (_r.chance > ca_rules.getNextPercentileNumber()) {
                                executeCARule(_r,(_c,_c.getGridPos())).ForEach(_e => {expended_sites.Add(_e); });
                            }
                            else {
                                smart_buffer.Add(_c);
                            }
                            break;
                        }
                        default: {throw new System.ArgumentException();}
                    }
                }
            });
        });
        smart_cells = smart_buffer;
        expended_sites.ForEach(_l => makeDirty(cell_grid[_l]));
    }

    private void makeDirty(StreamlinedCell _cell) {
        smart_cells.Add(_cell);
        _cell.getGridPos().makeMooreNeighbourhoodCoordsAroundCenter().Where(_v => cellGridSize.isCoordInGrid(_v)).Select(_p => cell_grid[_p]).ToList().ForEach(_e => smart_cells.Add(_e));
    }

    private HashSet<StreamlinedCell> makeDirty(StreamlinedCell _cell, HashSet<StreamlinedCell> _buffer) {
        _buffer.Add(_cell);
        _cell.getGridPos().makeMooreNeighbourhoodCoordsAroundCenter().Where(_v => cellGridSize.isCoordInGrid(_v)).Select(_p => cell_grid[_p]).ToList().ForEach(_e => _buffer.Add(_e));
        return _buffer;
    }
}


