using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Building : MonoBehaviour
{
    public Coord coord;
    public abstract void Init(Coord coord, Human builder);   
}
