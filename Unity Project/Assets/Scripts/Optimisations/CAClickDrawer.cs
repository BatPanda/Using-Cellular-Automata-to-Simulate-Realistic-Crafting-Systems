using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BrushTypes {
    SQUARE,
    CIRCLE
}

public class CAClickDrawer : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerMoveHandler // A tool that handles all things drawing related! Works with the new system!
{
    [Header("Draw Settings")]
    [SerializeField] private string ingredientName;

    [Range(1,100)]
    [SerializeField] private int brushSize;
    [SerializeField] private BrushTypes brushType;

    [SerializeField] private bool destructive = true;

    [SerializeField] private bool canDraw = true;

    [Header("Misc")]
    public RectTransform renderTarget;
    public NewCellGridController gridController;

    private int resolution;
    private float grid_size_x;
    private float grid_size_y;


    public void setBrushIngredient(string _new_brush_ingredient) {
        ingredientName = _new_brush_ingredient;
    }

    public void setBrushSize(int _new_size) {
        brushSize = _new_size;
    }

    public void setIsDestructive(bool _is_destructive) {
        destructive = _is_destructive;
    }

    // Start is called before the first frame update
    void Start()
    {
        resolution = gridController.getRTResolution();
        grid_size_x = gridController.getCellLengthAndHeight().x;
        grid_size_y = gridController.getCellLengthAndHeight().y;
    }

    public void setCanDraw(bool _can_draw) {
        canDraw = _can_draw;
    }

    private void drawCell(PointerEventData _event_data) {
        if (canDraw) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(renderTarget,_event_data.position,Camera.main, out Vector2 local_click);

            int center_x = Mathf.FloorToInt((local_click.x/renderTarget.sizeDelta.x)*grid_size_x);
            int center_y = Mathf.FloorToInt(Mathf.Abs((local_click.y/renderTarget.sizeDelta.y)*grid_size_y));

            // Debug.Log($"Clicked at: {local_click.x}:{local_click.y}");
            // Debug.Log($"Trying to place at: {x}:{y}");

            proccessCells(brushType switch {
                BrushTypes.SQUARE => new Vector2Int(center_x,center_y).makeSquare(brushSize),
                BrushTypes.CIRCLE => new Vector2Int(center_x,center_y).makeCircle(brushSize),
                _ => throw new System.ArgumentException("Brush type is unregistered!")
            });
        }
    }

    private void proccessCells(List<Vector2Int> _cells) {
        if (destructive) { 
            gridController.setCells(_cells,ingredientName); 
        } else {
            gridController.setCellsNonDestructive(_cells,ingredientName);
        }
    }

    public void OnPointerDown(PointerEventData _pointerEventData)
    {
        drawCell(_pointerEventData);
    }

     public void OnPointerMove(PointerEventData _pointerEventData)
    {
        if (Input.GetMouseButton(0)) {
           drawCell(_pointerEventData);
        }
    }

    public void OnPointerEnter(PointerEventData _pointerEventData)
    {
        if (Input.GetMouseButton(0)) {
           drawCell(_pointerEventData);
        }
    }
}
