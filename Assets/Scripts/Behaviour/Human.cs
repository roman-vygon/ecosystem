using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Animal
{
    Coord buildingPlace;
    public House House;
    House myHouse = null;
    protected override void ChooseNextAction()
    {
        if (mate != null && hunger < 0.5 && thirst < 0.5 && myHouse == null)
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
                    buildingPlace.x += Environment.getInt(-6, 6);
                    buildingPlace.y += Environment.getInt(-6, 6);

                    if (buildingPlace.x >= Environment.walkable.GetLength(0) || buildingPlace.y >= Environment.walkable.GetLength(1))
                        continue;
                    if (Environment.walkable[buildingPlace.x, buildingPlace.y] == true)
                        break;
                }
            }
            currentAction = CreatureAction.Building;
            buildingPlace = Environment.GetNextTileRandom(coord);
            Act();
            return;
        }
        base.ChooseNextAction();
    }
    House BuildHouse()
    {
        var entity = Instantiate(House);
        entity.Init(coord);        
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
                    Debug.Log(myHouse);
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
