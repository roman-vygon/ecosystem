using UnityEngine;

public class LivingEntity : MonoBehaviour, ICoordInterface {

    public int colourMaterialIndex;
    public Species species;
    public Material material;    
    public Coord coord { get; set; }

    public float timeToDie;
    float birthTime;
    
    [HideInInspector]
    public int mapIndex { get; set; }
    [HideInInspector]
    public Coord mapCoord { get; set; }

    protected bool dead;

    public virtual void Init (Coord coord) {
        this.coord = coord;
        transform.position = Environment.tileCentres[coord.x, coord.y];

        // Set material to the instance material
        var meshRenderer = transform.GetComponentInChildren<MeshRenderer> ();
        for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
        {
            if (meshRenderer.sharedMaterials[i] == material) {
                material = meshRenderer.materials[i];
                break;
            }
        }
    }
    public virtual void Start()
    {
        birthTime = Time.time;
    }

    public virtual void Die (CauseOfDeath cause) {
        if (!dead) {
            dead = true;
            Environment.RegisterDeath (this);
            Destroy (gameObject);
            Environment.AddDeath(species, cause);
        }
        else
        {

        }
    }
    public virtual void Update()
    {
        if (Time.time - birthTime >= timeToDie)        
            Die(CauseOfDeath.Age);
        
    }
}