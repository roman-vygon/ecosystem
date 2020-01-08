using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Animal
{
    Coord buildingPlace;
    public House House;
    public House myHouse = null;
    protected override void ChooseNextAction()
    {
        if (currentAction == CreatureAction.Building)
            Act();      
        base.ChooseNextAction();
    }
    protected override void Mate(Animal mate)
    {
        if (myHouse == null)
        {
            if (genes.isMale)
            {
                House nearest = Environment.SenseHouse(coord, 500);
                Coord nearestHouse;
                if (nearest == null)
                    nearestHouse = coord;
                else
                    nearestHouse = nearest.coord;

                if (nearestHouse != null)
                {

                    
                    buildingPlace = nearestHouse;
                    while (true)
                    {
                        buildingPlace.x += Environment.getInt(-1, 1);
                        buildingPlace.y += Environment.getInt(-1, 1);                        
                        if (buildingPlace.x >= Environment.walkable.GetLength(0) || buildingPlace.y >= Environment.walkable.GetLength(1) || buildingPlace.x < 0 || buildingPlace.y < 0)
                            continue;
                        if (Environment.walkable[buildingPlace.x, buildingPlace.y] == true)
                            break;
                    }

                }
                this.mate = mate;
                mate.mate = this;
                mate.currentAction = CreatureAction.GoingToMate;
                currentAction = CreatureAction.Building;
                mate.CreatePath(buildingPlace);
                buildingPlace = Environment.GetNextTileRandom(coord);                
            }                                    
        }
        else        
            base.Mate(mate);        
    }
    public override Coord getMatingPoint(Animal mate)
    {
        return (mate as Human).myHouse.coord;
    }
    House BuildHouse()
    {
        
        var entity = Instantiate(House);        
        entity.Init(coord, this, mate as Human);                
        return entity;
    }
    protected override void Act()
    {
        switch (currentAction)
        {
            case CreatureAction.Building:
                if (Coord.AreNeighbours(coord, buildingPlace))
                {
                    LookAt(buildingPlace);
                    myHouse = BuildHouse();
                    (mate as Human).myHouse = myHouse;
                    Mate(mate);
                }
                else
                {
                    StartMoveToCoord(path[pathIndex]);
                    pathIndex++;
                }
                break;
        }
        base.Act();
    }
    // Start is called before the first frame update
    

    // Update is called once per frame
 
}
