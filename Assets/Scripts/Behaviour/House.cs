using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public Coord coord;
    public virtual void Init(Coord coord)
    {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];
    }
     
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
