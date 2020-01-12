using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Building : MonoBehaviour, ICoordInterface
{
    public Coord coord { get; set; }
    public Coord mapCoord { get; set; }
    public int mapIndex { get; set; }

    public BuildingTypes buildingType;
    public abstract void Init(Coord coord, Human builder);   
}
