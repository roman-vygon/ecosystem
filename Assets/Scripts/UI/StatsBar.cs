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
    private Text action;
    private Text gender;
    Canvas c;
    void Start()
    {   
        c = gameObject.GetComponentsInChildren<Canvas>()[0];
        foreach (Text t in c.GetComponentsInChildren<Text>())
            if (t.gameObject.name == "actionText")
                action = t;
        
        foreach (Text t in c.GetComponentsInChildren<Text>())
            if (t.gameObject.name == "genderText")            
                gender = t;
            

        



        hunger = c.GetComponentsInChildren<Slider>()[0];
        thirst = c.GetComponentsInChildren<Slider>()[1];
        parent = gameObject.GetComponent<Animal>();
    }

    // Update is called once per frame
    void Update()
    {
        Camera camera = Camera.main;

        c.transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward, camera.transform.rotation * Vector3.up);
        action.text = parent.currentAction.ToString();
        hunger.value = parent.hunger;
        thirst.value = parent.thirst;
        gender.text = ((parent.genes.isMale) ? "Male" : "Female");

    }
}
