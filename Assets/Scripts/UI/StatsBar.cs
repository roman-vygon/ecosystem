using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StatsBar : MonoBehaviour
{
    // Start is called before the first frame update
    private Animal parent;
    private Slider hunger;
    private Slider thirst;
    void Start()
    {   
        Canvas c = gameObject.GetComponentsInChildren<Canvas>()[0];
        Debug.Log(c);
        hunger = c.GetComponentsInChildren<Slider>()[0];
        thirst = c.GetComponentsInChildren<Slider>()[1];
        parent = gameObject.GetComponent<Animal>();
    }

    // Update is called once per frame
    void Update()
    {
        
        hunger.value = parent.hunger;
        thirst.value = parent.thirst;
    }
}
