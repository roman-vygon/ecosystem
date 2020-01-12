using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]

public class ObjectPoolItem
{
    public Species objectToPool;
    public int amountToPool;
    public bool shouldExpand;
}

public class ObjectPooler : MonoBehaviour
{

    public static ObjectPooler SharedInstance;
    public List<ObjectPoolItem> itemsToPool;
    public Dictionary<Species, List<LivingEntity>> pooledObjects;    

    void Awake()
    {
        SharedInstance = this;
    }

    // Use this for initialization
    void Start()
    {
        pooledObjects = new Dictionary<Species, List<LivingEntity>>();
        foreach (ObjectPoolItem item in itemsToPool)
        {
            for (int i = 0; i < item.amountToPool; i++)
            {
                LivingEntity obj = Instantiate(EnvironmentUtility.prefabBySpecies[item.objectToPool]);
                obj.gameObject.SetActive(false);
                pooledObjects[obj.species].Add(obj);
            }
        }
    }

    public LivingEntity GetPooledObject(Species species)
    {
        if (!pooledObjects.ContainsKey(species))
            pooledObjects[species] = new List<LivingEntity>();
        for (int i = 0; i < pooledObjects[species].Count; i++)
        {
            if (!pooledObjects[species][i].gameObject.activeInHierarchy)
            {
                return pooledObjects[species][i];
            }
        }

        LivingEntity obj = Instantiate(EnvironmentUtility.prefabBySpecies[species]);
        obj.gameObject.SetActive(false);
        pooledObjects[species].Add(obj);
        return obj;        
    }    
}
