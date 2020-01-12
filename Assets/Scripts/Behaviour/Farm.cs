using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Building
{    
    Human worker;
    public float fullness = 0f;
    public float timeToFull = 14f;
    public float numFood = 4f;

    public void Work()
    {
        fullness += Time.deltaTime * 1 / timeToFull;
        if (fullness > 1)
            fullness = 1;
    }
    public override void Init(Coord coord, Human worker)
    {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];
        this.worker = worker;
        worker.myFarm = this;
        buildingType = BuildingTypes.Farm;
    }
}
