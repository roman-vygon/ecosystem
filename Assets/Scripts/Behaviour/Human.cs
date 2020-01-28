using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Animal
{
    public Coord buildingPlace;

    public Farm myFarm;
    public House myHouse = null;

    [Header("Building prefabs")]
    public House house;
    public Farm farm;
    public Barn barn;


    Barn barnTarget = null;
    
    
    public Building buildingOrder;
    protected override void ChooseNextAction()
    {
        lastActionChooseTime = Time.time;
        if (currentAction == CreatureAction.Building || currentAction == CreatureAction.GoingToBarn)
        {
            Act();
            return;
        }

        bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget != null && !foodTarget.Equals(null)&& hunger > 0;
        bool currentlyDrinking = currentAction == CreatureAction.Drinking && thirst > 0;

        if (currentAction == CreatureAction.Exploring && (hunger >= thirst && hunger > 0.55) 
                                                      && myHouse != null) {
            
            buildingPlace = chooseNear(myHouse.coord);
            
            currentAction = CreatureAction.Building;
            CreatePath(buildingPlace);
            buildingOrder = farm;
            Act();
            return;
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
        else if (myFarm != null)
        {
            currentAction = CreatureAction.WorkingAtFarm;
            CreatePath(myFarm.coord);
        }
        else
            currentAction = CreatureAction.Exploring;

        Act();
    }

    Coord chooseNear(Coord baseCoord)
    {
        Coord buildingPlace = baseCoord;
        int k = 0;
        while (true)
        {
            k++;
            buildingPlace = baseCoord;

            buildingPlace.x += Environment.getInt(-5, 5);
            buildingPlace.y += Environment.getInt(-5, 5);
            if (buildingPlace.x >= Environment.walkable.GetLength(0) || buildingPlace.y >= Environment.walkable.GetLength(1) || buildingPlace.x < 0 || buildingPlace.y < 0)
                continue;
            if (Environment.walkable[buildingPlace.x, buildingPlace.y] == true && buildingPlace != baseCoord &&   (EnvironmentUtility.TileIsVisibile(coord.x, coord.y, buildingPlace.x, buildingPlace.y)))                
                break;
            if (k > 1000)
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
                House nearest = Environment.senseBuilding(BuildingTypes.House, this, Coord.CoordPenalty) as House;                
                    if (nearest != null)
                        buildingPlace = chooseNear(nearest.coord);
                    else
                        buildingPlace = chooseNear(coord);

                    this.mate = mate;
                    mate.mate = this;
                mate.currentAction = CreatureAction.GoingToMate;
               currentAction = CreatureAction.Building;
               buildingOrder = house;
               CreatePath(buildingPlace);
               mate.CreatePath(buildingPlace);
            }                                    
        }
        else        
            base.Mate(mate);        
    }
    protected override void FindFood()
    {
        LivingEntity foodSource = Environment.SenseFood(coord, this, FoodPreferencePenalty);

        Barn barn = Environment.senseBuilding(BuildingTypes.Barn, this, Barn.fullBarnPenalty) as Barn;

        if (barn)
        {
            if (barn.numFood != 0)
            {
                currentAction = CreatureAction.GoingToFood;
                foodTarget = barn;
                CreatePath(foodTarget.coord);
                return;
            }
        }
        if (foodSource)
        {
            currentAction = CreatureAction.GoingToFood;
            foodTarget = foodSource;
            CreatePath(foodTarget.coord);
            return;
        }

        if (myFarm != null)
            currentAction = CreatureAction.WorkingAtFarm;
        else
            currentAction = CreatureAction.Exploring;
    }
    public override Coord getMatingPoint(Animal mate)
    {
        return (mate as Human).myHouse.coord;
    }    
    Building build()
    {        
        var entity = Instantiate(buildingOrder);        
        entity.Init(coord, this);
        Environment.RegisterBirth(entity, coord);
        return entity;
    }

    protected override void HandleInteractions()
    {
        if (currentAction == CreatureAction.Eating)
        {
            if (foodTarget != null && !foodTarget.Equals(null) && hunger > 0)
            {

                float eatAmount = Mathf.Min(hunger, Time.deltaTime * 1 / eatDuration);

                if (foodTarget is Barn)
                {
                    (foodTarget as Barn).getFood();
                    hunger = 0.0f;
                    return;
                }
                if (foodTarget is Animal)
                {
                    (foodTarget as Animal).Die(CauseOfDeath.Eaten);
                    hunger = 0.0f;
                }               

                if (foodTarget is Plant)
                {
                    eatAmount = ((Plant)foodTarget).Consume(eatAmount);
                    hunger -= eatAmount;
                }
            }
        }
        else if (currentAction == CreatureAction.Drinking)
        {
            if (thirst > 0)
            {
                thirst -= Time.deltaTime * 1 / drinkDuration;
                thirst = Mathf.Clamp01(thirst);
            }
        }
    }

    protected override void Act()
    {
        switch (currentAction)
        {
            case CreatureAction.GoingToMate:
                if (Coord.AreNeighbours(coord, mate.coord))
                {
                    
                    if (myHouse != null)
                    {
                        LookAt(mate.coord);
                        if (genes.isMale == false)
                            giveBirth();
                    }
                }
                else
                {
                    if (path != null && pathIndex < path.Length)
                    {
                        StartMoveToCoord(path[pathIndex]);
                        pathIndex++;
                    }
                }
                break;
            case CreatureAction.Building:
                if (Coord.AreNeighbours(coord, buildingPlace))
                {
                    LookAt(buildingPlace);
                    Building result = build();         
                    if (result is House)                    
                        Mate(mate);
                    if (result is Barn)
                    {
                        currentAction = CreatureAction.WorkingAtFarm;
                        CreatePath(myFarm.coord);
                    }
                    if (result is Farm)
                    {
                        currentAction = CreatureAction.WorkingAtFarm;
                        Barn nearestBarn = Environment.senseBuilding(BuildingTypes.Barn, this, Coord.CoordPenalty) as Barn;

                        if (nearestBarn == null)
                        {
                            currentAction = CreatureAction.Building;
                            buildingPlace = chooseNear(myFarm.coord);
                            buildingOrder = barn;
                            CreatePath(buildingPlace);
                        }
                        else
                        {                            
                            currentAction = CreatureAction.WorkingAtFarm;
                            CreatePath(myFarm.coord);
                        }
                    }
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
                        barnTarget = Environment.senseBuilding(BuildingTypes.Barn, this, Coord.CoordPenalty) as Barn;
                        if (barnTarget is null)
                        {
                            currentAction = CreatureAction.Building;
                            buildingPlace = chooseNear(myFarm.coord);
                            buildingOrder = barn;
                            CreatePath(buildingPlace);
                        }                    
                        currentAction = CreatureAction.GoingToBarn;
                        CreatePath(barnTarget.coord);
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
    protected override void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, Environment.tileCentres[buildingPlace.x, buildingPlace.y]);
            if (currentAction == CreatureAction.WorkingAtFarm)
            {
                var path = EnvironmentUtility.GetPath(coord.x, coord.y, foodTarget.coord.x, foodTarget.coord.y);
                Gizmos.color = Color.black;
                if (path != null)
                {
                    for (int i = 0; i < path.Length; i++)
                    {
                        Gizmos.DrawSphere(Environment.tileCentres[path[i].x, path[i].y], .2f);
                    }
                }
            }
        }
        base.OnDrawGizmosSelected();

            
    }
}
