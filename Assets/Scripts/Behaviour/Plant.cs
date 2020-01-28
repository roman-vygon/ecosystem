using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : LivingEntity {
    float amountRemaining = 1;
    public float consumeSpeed = 1;
    float lastReproductionTime;
    
    public float timeToReproduct = 64;
    
    public float Consume (float amount) {
        float amountConsumed = Mathf.Max (0, Mathf.Min (amountRemaining, amount));
        amountRemaining -= amount * consumeSpeed;

        transform.localScale = Vector3.one * amountRemaining;

        if (amountRemaining <= 0) {
            Die (CauseOfDeath.Eaten);                   
        }

        return amountConsumed;
    }
    public void onEnable()
    {
        birthTime = Time.time;
        lastReproductionTime = Time.time;
    }
    void Reproduct()
    {
        lastReproductionTime = Time.time;
        //LivingEntity prefab = EnvironmentUtility.prefabBySpecies[species];
        Coord spwnPoint = Environment.GetNextTileRandom(coord);
        if (spwnPoint == coord)
            return;
        var entity = Environment.objectPools[species].Get() as Plant;
        entity.Init(spwnPoint);
        entity.onEnable();
        Debug.Log(Time.time - entity.lastReproductionTime, this);
        Environment.RegisterBirth(entity, spwnPoint);
    }
    void Update()
    {
        base.Update();
        
        if (Time.time - lastReproductionTime >= timeToReproduct)
            Reproduct();
    }
    public float AmountRemaining {
        get {
            return amountRemaining;
        }
    }
}