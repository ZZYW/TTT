﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;


public class Thing : MonoBehaviour
{
    protected bool InWater { get; private set; }
    protected int NeighborCount { get { return neighborList.Count; } }
    protected string MyName { get; private set; }

    internal float DesiredFollowDistance { get; private set; }
    internal float NeighborDistanceThreshold { get; private set; }
    protected Boid boid { get; private set; }
    //cool down stuff to avoid crash
    Cooldown speakCD;
    Cooldown playSoundCD;
    // float speakCooldown;
    // bool speakInCD;
    // float spokeTimeStamp;

    float detectEnterExitCooldown;
    bool detectingEnter;
    bool detectingExit;

    bool stopWalkingAround;
    bool stopTalking;

    SphereCollider neighborDetector;
    new Rigidbody rigidbody;
    ParticleSystem explodePS;
    AudioSource audioSource;
    List<GameObject> neighborList;

    static GameObject generatedCubeContainer;
    static string thingTag = "Thing";

    Color originalColor;
    StringBuilder stringBuilder;

    Material mMat;

    private void OnEnable()
    {
        TTTEventsManager.OnSomeoneSpeaking += OnSomeoneSpeaking;
        TTTEventsManager.OnSomeoneSparking += OnSomeoneSparking;
        TOD_Data.OnSunset += OnSunset;
        TOD_Data.OnSunrise += OnSunrise;
    }

    private void OnDisable()
    {
        TTTEventsManager.OnSomeoneSpeaking -= OnSomeoneSpeaking;
        TTTEventsManager.OnSomeoneSparking -= OnSomeoneSparking;
        TOD_Data.OnSunset -= OnSunset;
        TOD_Data.OnSunrise -= OnSunrise;
        CancelInvoke();
    }

    private void Awake()
    {
        speakCD = new Cooldown(2);
        playSoundCD = new Cooldown(2);
        DesiredFollowDistance = 10;
        NeighborDistanceThreshold = 10;

        if (GetComponent<Rigidbody>() == null)
        {
            rigidbody = gameObject.SafeAddComponent<Rigidbody>();
        }

        MyName = gameObject.name;
        stringBuilder = new StringBuilder();
        gameObject.tag = thingTag;
        gameObject.layer = 14;
        neighborList = new List<GameObject>();

        explodePS = Instantiate(ResourceManager.main.sparkPSPrefab, transform, false).GetComponent<ParticleSystem>();
        audioSource = gameObject.SafeAddComponent<AudioSource>();
        gameObject.SafeAddComponent<BoxCollider>();
        boid = gameObject.SafeAddComponent<Boid>();
        boid.host = this;
        gameObject.SafeAddComponent<MeshFilter>().mesh = ResourceManager.main.cubeMesh.mesh;
        mMat = gameObject.SafeAddComponent<MeshRenderer>().material;
        originalColor = mMat.GetColor("_Color");

        ThingAwake();
    }

    private void Start()
    {


        //Sound
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.bypassListenerEffects = false;

        audioSource.spatialBlend = 0.9f;
        audioSource.maxDistance = 200;
        audioSource.dopplerLevel = 5;
        audioSource.clip = ResourceManager.main.sound;


        //set its initial position
        Vector3 initialPosition = new Vector3(Random.Range(-100, 100), 10, Random.Range(-100, 100));
        transform.position = initialPosition;

        ThingStart();

        speakCD.GoCooldown();

    }
    private void Update()
    {
        //check cooldown
        speakCD.Check();
        playSoundCD.Check();

        var scale = transform.localScale;
        rigidbody.mass = scale.magnitude * 4;


        //check neighbors
        foreach (GameObject t in ThingManager.main.things)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < NeighborDistanceThreshold)
            {
                if (!neighborList.Contains(t))
                {
                    neighborList.Add(t);
                    OnMeetingSomeone(t);
                }
            }
            else
            {
                if (neighborList.Contains(t))
                {
                    neighborList.Remove(t);
                    OnLeavingSomeone(t);
                }
            }
        }

        ThingUpdate();
    }

    private void OnSomeoneSpeaking(GameObject who)
    {
        if (neighborList.Contains(who))
        {
            OnNeighborSpeaking();
        }
    }

    private void OnSomeoneSparking(GameObject who)
    {
        if (neighborList.Contains(who))
        {
            OnNeigborSparkingParticles();
        }
    }

    //use string builder to concat string to avoid memory leak
    private string FormatString(string format, params object[] args)
    {
        stringBuilder.Length = 0;
        stringBuilder.AppendFormat(format, args);
        return stringBuilder.ToString();
    }


    public void OnWaterEnter()
    {
        InWater = true;
        Invoke("RescueFromWater", 60f);
        OnTouchWater();
    }

    public void OnWaterExit()
    {
        InWater = false;
        CancelInvoke("RescueFromWater");
        OnLeaveWater();
    }

    protected void SetCohesionWeight(float cohesionWeight)
    {
        boid.cohWeight = cohesionWeight;
    }
    protected void SetSeperationWeight(float separationWeight)
    {
        boid.sepWeight = separationWeight;
    }
    protected void SetAlignmentWeight(float alignmentWeight)
    {
        boid.aliWeight = alignmentWeight;
    }

    protected void SetMaxSpeed(float maxSpeed)
    {
        boid.maxSpeed = maxSpeed;
    }

    protected void StopMoving()
    {
        //todo
    }

    protected void StopMoving(float seconds)
    {
        StopMoving();
        Invoke("RestartWalking", seconds);
    }

    protected void Mute()
    {
        stopTalking = true;
    }

    protected void DeMute()
    {
        stopTalking = false;
    }

    protected void RestartWalking()
    {
        stopWalkingAround = false;
    }
   
    protected void AddForce(Vector3 f)
    {
        boid.rb.AddForce(f);
    }

    protected void SetScale(Vector3 newScale)
    {
        transform.localScale = newScale;
    }

    protected void Speak(string content)
    {
        if (stopTalking) return;
        if (speakCD.inCD) return;
        PlaySound();
        TTTEventsManager.main.SomeoneSpoke(gameObject);
        ChatBubbleManager.main.Speak(transform, content);
        speakCD.GoCooldown();
    }

    protected void Spark(Color particleColor, int numberOfParticles)
    {

        if (explodePS == null)
        {
            explodePS = GetComponentInChildren<ParticleSystem>();
            if (explodePS == null) return;
        }

        ParticleSystem.MainModule particleMain = explodePS.main;
        particleMain.startColor = particleColor;
        var newBurst = new ParticleSystem.Burst(0f, numberOfParticles);
        explodePS.emission.SetBurst(0, newBurst);
        explodePS.Play();
        TTTEventsManager.main.SomeoneSparked(gameObject);

    }

    protected void CreateChild()
    {
        GameObject newborne = new GameObject(MyName + "'s copy");
        newborne.transform.position = transform.position + Vector3.down;
        newborne.SafeAddComponent<Rigidbody>();
        newborne.SafeAddComponent<BoxCollider>();
        var mf = newborne.SafeAddComponent<MeshFilter>();
        mf.mesh = transform.GetComponent<MeshFilter>().sharedMesh;
        var rend = newborne.SafeAddComponent<MeshRenderer>();
        rend.sharedMaterial = mMat;

        //set child scale
        newborne.transform.localScale = transform.localScale / 2;

        if (generatedCubeContainer == null)
        {
            generatedCubeContainer = new GameObject();
            generatedCubeContainer.name = "all children";
            generatedCubeContainer.SafeAddComponent<ChildrenCounter>();
        }

        newborne.transform.parent = generatedCubeContainer.transform;
        generatedCubeContainer.GetComponent<ChildrenCounter>().list.Add(newborne);
    }

    protected void ResetColor()
    {
        if (mMat == null) return;
        mMat.color = originalColor;
    }

    protected void ChangeColor(Color c)
    {
        if (mMat == null) return;
        mMat.SetColor("_Color", c);
    }

    private void PlaySound()
    {
        if (playSoundCD.inCD) return;
        playSoundCD.GoCooldown();
        if (audioSource.isPlaying) return;
        if (audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    protected Boid GetBoid()
    {
        return boid;
    }

    //VIRTUAL
    protected virtual void ThingAwake() { }
    protected virtual void ThingStart() { }
    protected virtual void ThingUpdate() { }
    protected virtual void OnMeetingSomeone(GameObject other) { }
    protected virtual void OnLeavingSomeone(GameObject other) { }
    protected virtual void OnNeighborSpeaking() { }
    protected virtual void OnNeigborSparkingParticles() { }
    protected virtual void OnTouchWater() { }
    protected virtual void OnLeaveWater() { }
    protected virtual void OnSunset() { }
    protected virtual void OnSunrise() { }




}


//It is common to create a class to contain all of your
//extension methods. This class must be static.
public static class ExtensionMethods
{
    //Even though they are used like normal methods, extension
    //methods must be declared static. Notice that the first
    //parameter has the 'this' keyword followed by a Transform
    //variable. This variable denotes which class the extension
    //method becomes a part of.
    public static T SafeAddComponent<T>(this GameObject me) where T : Component
    {
        if (me.GetComponent<T>() != null)
        {
            return me.GetComponent<T>();
        }
        else
        {
            return me.AddComponent<T>();
        }
    }
}