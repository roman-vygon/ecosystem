using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building
{
    
    Human occupant1, occupant2;
    public override void Init(Coord coord, Human builder)
    {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];
        occupant1 = builder;
        occupant2 = builder.mate as Human;
        (builder.mate as Human).myHouse = this;
    }

    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        if (occupant1 == null || occupant2 == null)
            Destroy(gameObject);
    }
}
