using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamlinedCell //The pure c# optimised cell data used in the new cell grid system. replaced cell data. 
{
    public int grid_x_pos {get; private set;} = -1;
    public int grid_y_pos {get; private set;} = -1;
    public Ingredient ingredient {get; private set;}
    
    //public Maybe<IngredientTS> thread_ingredient {get; private set;}

    public Color colour {get; private set;}

    public System.Random rnd {get; private set;}

    public StreamlinedCell(System.Random _rnd, int _x, int _y, Ingredient _starting_ingredient) {
        rnd = _rnd;
        grid_x_pos = _x;
        grid_y_pos = _y;
        ingredient = _starting_ingredient;
        //thread_ingredient = new Maybe<IngredientTS>(new IngredientTS(ingredient));
        colour = ingredient.getColours().getRandomColourFromList(rnd);
    }

    public StreamlinedCell(StreamlinedCell _cell) {
        //Debug.Log(_cell);
        grid_x_pos = _cell.grid_x_pos;
        grid_y_pos = _cell.grid_y_pos;
        ingredient = _cell.ingredient;
        //thread_ingredient = new Maybe<IngredientTS>(new IngredientTS(ingredient));
        rnd = _cell.rnd;
    }

    public void setNewState(Ingredient _new_ingredient) {
        ingredient = _new_ingredient;
        //thread_ingredient = new Maybe<IngredientTS>(new IngredientTS(ingredient));
        colour = ingredient.getColours().getRandomColourFromList(rnd);
    }

    public Vector2Int getGridPos() => new Vector2Int(grid_x_pos,grid_y_pos); 
}
