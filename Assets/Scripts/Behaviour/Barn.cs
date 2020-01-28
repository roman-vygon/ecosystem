using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barn : Building
{
    public int numFood {  get; private set; }

    public override void Init(Coord coord, Human builder)
    {
        this.coord = coord;
        numFood = 0;
        buildingType = BuildingTypes.Barn;
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];
    }    
    public void restock()
    {
        numFood += 10;
    }
    static public int fullBarnPenalty(ICoordInterface self, ICoordInterface other)
    {
        return -1 * (other as Barn).numFood;
    }
    public void getFood()
    {
        numFood -= 1;
    }
}
