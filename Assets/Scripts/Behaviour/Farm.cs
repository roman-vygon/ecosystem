using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Building
{    
    Human worker;
    public float fullness = 0f;
    public float timesToFull = 14f;
    public float numFood = 4f;

    public void Work()
    {
        fullness +=  1 / timesToFull;
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
    void Update()
    {
        if (worker == null)
        {
            Environment.RegisterDeath(this);
            Destroy(gameObject);
        }
    }
}
