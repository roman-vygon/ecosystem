using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barn : Building
{
    int numFood;

    public override void Init(Coord coord, Human builder)
    {
        this.coord = coord;
        numFood = 0;
    }    
    public void restock()
    {
        numFood += 10;
    }
    public void getFood()
    {
        numFood -= 1;
    }
}
