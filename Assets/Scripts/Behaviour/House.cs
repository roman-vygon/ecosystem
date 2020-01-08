using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public Coord coord;
    Human occupant1, occupant2;
    public virtual void Init(Coord coord, Human occupant1, Human occupant2)
    {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];
        this.occupant1 = occupant1;
        this.occupant2 = occupant2;
    }
     
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (occupant1 == null || occupant2 == null)
            Destroy(gameObject);
    }
}
