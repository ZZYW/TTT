using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;


public class ThingManager : MonoBehaviour
{
    public static ThingManager main;
    public List<GameObject> AllThings;
    public List<Boid> boids
    {
        get
        {
            return AllThings.Select(item => item.GetComponent<Boid>()).ToList();
        }
    }


    private void Awake()
    {
        main = this;
        var allThings = gameObject.GetComponentsInChildren<Thing>();
        foreach (Thing thing in allThings)
        {
            AllThings.Add(thing.gameObject);
        }

        //Instantiate chat bubble objects
        ChatBubbleManager.main.Init(allThings.Length);

    }

}