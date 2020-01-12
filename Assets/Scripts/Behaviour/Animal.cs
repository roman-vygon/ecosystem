using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : LivingEntity {

    public const int maxViewDistance = 50;

    [EnumFlags]
    public Species diet;    
    public CreatureAction currentAction;
    public Genes genes;
    public Color maleColour;
    public Color femaleColour;

    // Settings:
    float timeBetweenActionChoices = 1;
    public float moveSpeed = 2.5f;
    public float timeToDeathByHunger = 128;
    float timeToGrowth = 32;
    public float timeToDeathByThirst = 64;
    public float timeToDeathByReproduction = 128;
    float drinkDuration = 3;
    float eatDuration = 5;

    protected float criticalPercent = 0.7f;

    public int numOffsprings;
    // Visual settings:
    float moveArcHeight = .2f;

    
    // State:
    [Header ("State")]
    public float hunger;
    public float thirst;
    public float size;
    public float reproductionWill;

    [Space(20)]

    protected LivingEntity foodTarget;
    protected Coord waterTarget;

    Vector3 baseScale;
    public Animal hunter = null;
    public Animal mate = null;    
    // Move data:
    bool animatingMovement;
    Coord moveFromCoord;
    Coord moveTargetCoord;
    Vector3 moveStartPos;
    Vector3 moveTargetPos;
    public bool Male;
    float moveTime;
    float moveSpeedFactor;
    float moveArcHeightFactor;
    protected Coord[] path;
    protected int pathIndex;    
    // Other
    protected float lastActionChooseTime;
    const float sqrtTwo = 1.4142f;
    const float oneOverSqrtTwo = 1 / sqrtTwo;

    public override void Init (Coord coord) {
        base.Init (coord);        
        moveFromCoord = coord;
        genes = Genes.RandomGenes (1);
        
        size = (float)Environment.getRandomDouble() * 0.3f + 0.3f;

        hunger = (float)Environment.getRandomDouble() * 0.3f + 0.3f;
        thirst = (float)Environment.getRandomDouble() * 0.2f + 0.2f;
        reproductionWill = 0f;

        baseScale = transform.localScale;
        Male = genes.isMale;
        ChooseNextAction();
    }

    protected virtual void Update () {
        base.Update();
        // Increase hunger and thirst over time
        hunger += Time.deltaTime * 1 / timeToDeathByHunger ;
        reproductionWill += Time.deltaTime  * size / timeToDeathByReproduction;
        reproductionWill = Mathf.Clamp01(reproductionWill);
        size += Time.deltaTime * 1 / timeToGrowth;
        size = Mathf.Clamp01(size);
        transform.localScale = size * baseScale;
        thirst += Time.deltaTime * 1 / timeToDeathByThirst;

        

        // Animate movement. After moving a single tile, the animal will be able to choose its next action
        if (animatingMovement) {
            AnimateMove ();
        } else {
            // Handle interactions with external things, like food, water, mates
            HandleInteractions ();
            float timeSinceLastActionChoice = Time.time - lastActionChooseTime;
            if (timeSinceLastActionChoice > timeBetweenActionChoices) {
                ChooseNextAction ();
            }
        }

        if (hunger >= 1)
        {
            Die(CauseOfDeath.Hunger);
        }
        else if (thirst >= 1)
        {
            Die(CauseOfDeath.Thirst);
        }

    }

    // Animals choose their next action after each movement step (1 tile),
    // or, when not moving (e.g interacting with food etc), at a fixed time interval
    protected virtual void ChooseNextAction () {
        lastActionChooseTime = Time.time;
        // Get info about surroundings

        hunter = Environment.SensePredators(this);
        if (hunter != null)
        {
            currentAction = CreatureAction.RunningAway;
            Act();
            return;
        }
        
        if (currentAction == CreatureAction.GoingToMate && mate != null)
        {
            Act();
            return;
        }

        //if ((currentAction == CreatureAction.Drinking && thirst > 0.01) || (currentAction == CreatureAction.Eating && hunger > 0.01))
          //  return;
        // Decide next action:
        // Eat if (more hungry than thirsty) or (currently eating and not critically thirsty)
        bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget && hunger > 0;
        bool currentlyDrinking = currentAction == CreatureAction.Drinking && thirst > 0;
        if ((hunger >= thirst && hunger > 0.55) || currentlyEating && thirst < criticalPercent) {
            FindFood ();
        }
        // More thirsty than hungry
        else {
            if (thirst > 0.55 || currentlyDrinking)
                FindWater();
            else
            if (reproductionWill > 0.55)
            {
                currentAction = CreatureAction.SearchingForMate;
                FindMate();
            }
            else
                if (Environment.getRandomDouble() > 0.9)
                currentAction = CreatureAction.Resting;
            else
                currentAction = CreatureAction.Exploring;
        }

        
        Act ();

    }

    protected virtual void FindFood () {
        LivingEntity foodSource = Environment.SenseFood (coord, this, FoodPreferencePenalty);
        if (foodSource) {
            currentAction = CreatureAction.GoingToFood;
            foodTarget = foodSource;
            CreatePath (foodTarget.coord);

        } else {
            currentAction = CreatureAction.Exploring;
        }
    }

    protected virtual void FindWater () {
        Coord waterTile = Environment.SenseWater (coord);
        if (waterTile != Coord.invalid) {
            currentAction = CreatureAction.GoingToWater;
            waterTarget = waterTile;
            CreatePath (waterTarget);

        } else {
            currentAction = CreatureAction.Exploring;
        }
    }
    public virtual Coord getMatingPoint(Animal mate)
    {
        return mate.coord;
    }
    protected virtual void Mate(Animal mate)
    {
        this.mate = mate;
        mate.mate = this;
        Coord meetingPoint = getMatingPoint(mate);
        CreatePath(meetingPoint);                
        currentAction = CreatureAction.GoingToMate;
        mate.CreatePath(meetingPoint);
        mate.currentAction = CreatureAction.GoingToMate;        
        
    }
    protected virtual void FindMate()
    {
        if (mate != null)
        {
            Mate(mate);
            return;
        }
        List<Animal> mates = Environment.SensePotentialMates(coord, this);

        if (mates.Count != 0)                    
            foreach(Animal possibleMate in mates)            
                if (possibleMate.reproductionWill > 0.65)                
                    if (genes.isMale)
                    {
                        Mate(possibleMate);                        
                        return;
                    }             
        else        
            currentAction = CreatureAction.SearchingForMate;
        
    }

    // When choosing from multiple food sources, the one with the lowest penalty will be selected
    protected virtual int FoodPreferencePenalty (LivingEntity self, LivingEntity food) {
        return Coord.SqrDistance (self.coord, food.coord);
    }

    
    protected void giveBirth()
    {
        mate.currentAction = CreatureAction.Resting;
        currentAction = CreatureAction.Resting;
        int fSeconds = Environment.getInt(1, 4);
        mate.Wait(fSeconds);
        Wait(fSeconds);        
        for (int i = 0; i < numOffsprings; ++i)
        {
            var entity = Instantiate(EnvironmentUtility.prefabBySpecies[species]);
            entity.Init(coord);
            Environment.RegisterBirth(entity, coord);
        }

        
        mate.reproductionWill = 0;
        mate.path = null;
        reproductionWill = 0;
        if (!(this is Human))
        {
            mate.mate = null;
            mate = null;
        }
    }
    public void Wait(int seconds)
    {
        new WaitForSeconds(seconds);
        return;
    }
    protected virtual void Act () {
        switch (currentAction) {
            case CreatureAction.RunningAway:
                StartMoveToCoord(Environment.GetNextTileAway(coord, hunter.coord));
                break;
            case CreatureAction.Exploring:                
                StartMoveToCoord (Environment.GetNextTileWeighted (coord, moveFromCoord));
                break;
            case CreatureAction.SearchingForMate:
                StartMoveToCoord(Environment.GetNextTileWeighted(coord, moveFromCoord));
                break;
            case CreatureAction.GoingToMate:                
                    if (Coord.AreNeighbours(coord, mate.coord))
                    {
                        LookAt(mate.coord);                    
                        if (genes.isMale == false)                    
                            giveBirth();                                                                                            
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
            case CreatureAction.GoingToFood:
                if (Coord.AreNeighbours (coord, foodTarget.coord)) {
                    LookAt (foodTarget.coord);
                    currentAction = CreatureAction.Eating;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.GoingToWater:
                if (Coord.AreNeighbours (coord, waterTarget)) {
                    LookAt (waterTarget);
                    currentAction = CreatureAction.Drinking;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
        }
    }

    public void CreatePath (Coord target) {
        // Create new path if current is not already going to target
        if (path == null)
        {
            path = EnvironmentUtility.GetPath(coord.x, coord.y, target.x, target.y);
            pathIndex = 0;
        }
        else
        {
            bool a = (pathIndex >= path.Length), b = false, c = false;
            if (path.Length != 0)
            {
                b = (path[path.Length - 1] != target);
                if (pathIndex != 0)
                    c = (path[pathIndex - 1] != moveTargetCoord);
             }
            if (a || b || c)
            {
                path = EnvironmentUtility.GetPath(coord.x, coord.y, target.x, target.y);
                pathIndex = 0;
            }
        }            
    }

    protected void StartMoveToCoord (Coord target) {
        moveFromCoord = coord;
        moveTargetCoord = target;
        moveStartPos = transform.position;
        moveTargetPos = Environment.tileCentres[moveTargetCoord.x, moveTargetCoord.y];
        animatingMovement = true;

        bool diagonalMove = Coord.SqrDistance (moveFromCoord, moveTargetCoord) > 1;
        moveArcHeightFactor = (diagonalMove) ? sqrtTwo : 1;
        moveSpeedFactor = (diagonalMove) ? oneOverSqrtTwo : 1;        
        LookAt (moveTargetCoord);        
    }

    protected void LookAt (Coord target) {
        if (target != coord) {
            Coord offset = target - coord;
            transform.eulerAngles = Vector3.up * Mathf.Atan2 (offset.x, offset.y) * Mathf.Rad2Deg;
        }
    }

    void HandleInteractions () {
        if (currentAction == CreatureAction.Eating) {            
            if (foodTarget && hunger > 0) {
                float eatAmount = Mathf.Min (hunger, Time.deltaTime * 1 / eatDuration);                
                if (foodTarget is Animal)
                {
                    foodTarget.Die (CauseOfDeath.Eaten);
                    hunger = 0.0f;                    
                }
                else
                {
                    eatAmount = ((Plant)foodTarget).Consume(eatAmount);
                    hunger -= eatAmount;
                }
            }
        } else if (currentAction == CreatureAction.Drinking) {
            if (thirst > 0) {
                thirst -= Time.deltaTime * 1 / drinkDuration;
                thirst = Mathf.Clamp01 (thirst);
            }
        }
    }

    void AnimateMove () {
        // Move in an arc from start to end tile
        moveTime = Mathf.Min (1, moveTime + Time.deltaTime * moveSpeed * moveSpeedFactor);
        float height = (1 - 4 * (moveTime - .5f) * (moveTime - .5f)) * moveArcHeight * moveArcHeightFactor;
        transform.position = Vector3.Lerp (moveStartPos, moveTargetPos, moveTime) + Vector3.up * height;

        // Finished moving
        if (moveTime >= 1) {
            Environment.RegisterMove (this, moveFromCoord, moveTargetCoord);
            coord = moveTargetCoord;

            animatingMovement = false;
            moveTime = 0;
            
            ChooseNextAction ();
        }
    }

    void OnDrawGizmosSelected () {
        if (Application.isPlaying) {
            var surroundings = Environment.Sense (coord);
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, Animal.maxViewDistance);
            if (surroundings.nearestFoodSource != null) {
                Gizmos.DrawLine (transform.position, surroundings.nearestFoodSource.transform.position);
            }
            if (surroundings.nearestWaterTile != Coord.invalid) {
                Gizmos.DrawLine (transform.position, Environment.tileCentres[surroundings.nearestWaterTile.x, surroundings.nearestWaterTile.y]);
            }
            if (currentAction == CreatureAction.Exploring)
            {
                
                Gizmos.color = Color.red;                
                Gizmos.DrawSphere(Environment.tileCentres[moveFromCoord.x, moveFromCoord.y], .2f);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(Environment.tileCentres[moveTargetCoord.x, moveTargetCoord.y], .2f);

            }
            if (currentAction == CreatureAction.GoingToMate)
            {                
                Gizmos.color = Color.black;
                if (path != null)
                {
                    for (int i = 0; i < path.Length; i++)
                    {
                        Gizmos.DrawSphere(Environment.tileCentres[path[i].x, path[i].y], .2f);
                    }
                }
            }

            if (currentAction == CreatureAction.GoingToFood) {
                var path = EnvironmentUtility.GetPath (coord.x, coord.y, foodTarget.coord.x, foodTarget.coord.y);
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
    }

}