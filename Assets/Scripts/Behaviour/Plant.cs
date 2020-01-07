using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : LivingEntity {
    float amountRemaining = 1;
    const float consumeSpeed = 1;
    float lastReproductionTime;
    float timeToReproduct = 64;
    public float Consume (float amount) {
        float amountConsumed = Mathf.Max (0, Mathf.Min (amountRemaining, amount));
        amountRemaining -= amount * consumeSpeed;

        transform.localScale = Vector3.one * amountRemaining;

        if (amountRemaining <= 0) {
            Die (CauseOfDeath.Eaten);
        }

        return amountConsumed;
    }
    void Start()
    {
        lastReproductionTime = Time.time;
    }
    void Update()
    {
        if (Time.time - lastReproductionTime >= timeToReproduct)
        {
            lastReproductionTime = Time.time;
            LivingEntity prefab = EnvironmentUtility.prefabBySpecies[species];
            Coord spwnPoint = Environment.GetNextTileRandom(coord);
            var entity = Instantiate(prefab);
            entity.Init(spwnPoint);
        }
    }
    public float AmountRemaining {
        get {
            return amountRemaining;
        }
    }
}