using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;


public class ThingManager : MonoBehaviour
{
    public static ThingManager main;
    public List<GameObject> things;
    public List<Boid> boids
    {
        get
        {
            return things.Select(item => item.GetComponent<Boid>()).ToList();
        }
    }


    private void Awake()
    {
        main = this;
        var allThings = gameObject.GetComponentsInChildren<Thing>();
        foreach (Thing thing in allThings)
        {
            things.Add(thing.gameObject);
        }


    }

}