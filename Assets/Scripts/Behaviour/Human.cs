using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Animal
{
    Coord buildingPlace;

    public Farm myFarm;
    public House myHouse = null;

    [Header("Building prefabs")]
    public House house;
    public Farm farm;
    public Barn barn;


    Barn barnTarget;
    
    
    protected Building buildingOrder;
    protected override void ChooseNextAction()
    {
        lastActionChooseTime = Time.time;
        if (currentAction == CreatureAction.Building || currentAction == CreatureAction.GoingToBarn)
            Act();

        bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget && hunger > 0;
        bool currentlyDrinking = currentAction == CreatureAction.Drinking && thirst > 0;

        if (currentAction == CreatureAction.Exploring && ((hunger >= thirst && hunger > 0.55) || currentlyEating && thirst < criticalPercent) 
                                                      && myHouse != null 
                                                      && currentAction != CreatureAction.GoingToMate
                                                      && myFarm != null) {
            buildingPlace = chooseNear(myHouse.coord);
            currentAction = CreatureAction.BuildingFarm;
            Act();
        }
        if (currentAction == CreatureAction.GoingToMate && mate != null)
        {
            Act();
            return;
        }


        if ((hunger >= thirst && hunger > 0.55) || currentlyEating && thirst < criticalPercent)
        {
            FindFood();
        }
        // More thirsty than hungry
        else if (thirst > 0.55 || currentlyDrinking)
            FindWater();
        else if (reproductionWill > 0.55)
        {
            currentAction = CreatureAction.SearchingForMate;
            FindMate();
        }
        else currentAction = CreatureAction.WorkingAtFarm;
        Act();
    }

    Coord chooseNear(Coord baseCoord)
    {   
        while (true)
        {
            Coord buildingPlace = baseCoord;            
            buildingPlace.x += Environment.getInt(-5, 5);
            buildingPlace.y += Environment.getInt(-5, 5);
            if (buildingPlace.x >= Environment.walkable.GetLength(0) || buildingPlace.y >= Environment.walkable.GetLength(1) || buildingPlace.x < 0 || buildingPlace.y < 0)
                continue;
            if (Environment.walkable[buildingPlace.x, buildingPlace.y] == true && buildingPlace != baseCoord)
                break;
        }
        return buildingPlace;
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
                    buildingPlace = chooseNear(nearestHouse);
                else
                    buildingPlace = chooseNear(coord);

                this.mate = mate;
                mate.mate = this;
                mate.currentAction = CreatureAction.GoingToMate;
                currentAction = CreatureAction.Building;
                mate.CreatePath(buildingPlace);                
            }                                    
        }
        else        
            base.Mate(mate);        
    }
    public override Coord getMatingPoint(Animal mate)
    {
        return (mate as Human).myHouse.coord;
    }    
    Building build()
    {        
        var entity = Instantiate(buildingOrder);        
        entity.Init(coord, this);                
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
                    Building result = build();         
                    if (result is House)                    
                        Mate(mate);
                    if (result is Farm)
                        currentAction = CreatureAction.WorkingAtFarm;                    
                }
                else
                {
                    StartMoveToCoord(path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.WorkingAtFarm:
                if (Coord.AreNeighbours(coord, myFarm.coord))
                {
                    LookAt(myFarm.coord);
                    myFarm.Work();
                    if (myFarm.fullness == 1)
                    {
                        myFarm.fullness = 0;
                        currentAction = CreatureAction.GoingToBarn;
                    }                    
                }
                else
                {
                    StartMoveToCoord(path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.GoingToBarn:
                if (Coord.AreNeighbours(coord, barnTarget.coord))
                {
                    LookAt(barnTarget.coord);
                    barnTarget.restock();
                    currentAction = CreatureAction.Resting;
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
