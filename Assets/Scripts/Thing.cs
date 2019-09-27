using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Thing : MonoBehaviour
{

    public class Settings
    {
        public int cameraOffset;
        public float acceleration;
        public float drag;
        public float mass;
        public int newDestinationRange;
        public Color myCubeColor;
        public int neighborDetectorRadius;

        public Settings()
        {
            cameraOffset = 15;
            acceleration = 4;
            drag = 1.8f;
            mass = 10f;
            newDestinationRange = 40;
            neighborDetectorRadius = 10;
        }
    }

    public SimpleChatBubble myChatBubble;
    public Settings settings { get; protected set; }



    protected bool InWater { get; private set; }
    protected int NeighborCount { get { return neighborList.Count; } }
    protected string MyName { get; private set; }

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
    Boid boid;
    SphereCollider neighborDetector;

    ParticleSystem explodePS;
    AudioSource audioSource;
    List<GameObject> neighborList;

    static string soundFilePath = "Sounds/";
    static string matColor = "_Color";
    static GameObject generatedCubeContainer;
    static string thingTag = "Thing";

    Color originalColor;
    StringBuilder stringBuilder;

    Material mMat;

    public int DesiredFollowDistance { get { return settings.cameraOffset; } }

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
        speakCD = new Cooldown(Random.Range(5f, 10f));
        playSoundCD = new Cooldown(1);

        

        MyName = gameObject.name;
        settings = new Settings();
        stringBuilder = new StringBuilder();
        gameObject.tag = thingTag;
        gameObject.layer = 14;
        neighborList = new List<GameObject>();

        explodePS = Instantiate(ResourceManager.main.sparkPSPrefab, transform, false).GetComponent<ParticleSystem>();
        audioSource = gameObject.AddComponent<AudioSource>();
        gameObject.AddComponent<BoxCollider>();
        boid = gameObject.AddComponent<Boid>();
        boid.host = this;
        gameObject.AddComponent<MeshFilter>().mesh = ResourceManager.main.cubeMesh.mesh;
        mMat = gameObject.AddComponent<MeshRenderer>().material;

        ThingAwake();
    }

    private void Start()
    {

  
        //Sound
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.bypassListenerEffects = false;
        audioSource.spatialBlend = 1f;
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


        //check neighbors
        foreach (GameObject t in ThingManager.main.AllThings)
        {
            float dist = Vector3.Distance(transform.position, t.transform.position);
            if (dist < settings.neighborDetectorRadius)
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

    protected void SetTarget(Vector3 target)
    {
        if (!stopWalkingAround)
        {
            // boid.SetTarget(target);
        }
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

    protected void SetRandomTarget(float area)
    {
        SetTarget(new Vector3(Random.Range(-area, area), 0, Random.Range(-area, area)));
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
        if (myChatBubble == null) return;
        if (stopTalking) return;
        if (speakCD.inCD) return;

        TTTEventsManager.main.SomeoneSpoke(gameObject);
        myChatBubble.Speak(content);
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

    protected void CreateChildren()
    {
        GameObject acube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        acube.layer = 12;

        acube.transform.localScale = Vector3.one / 4;
        acube.transform.position = transform.position;

        if (generatedCubeContainer == null)
        {
            generatedCubeContainer = new GameObject();
            generatedCubeContainer.name = MyName + "'s child";
            generatedCubeContainer.AddComponent<ChildrenCounter>();
        }

        acube.transform.parent = generatedCubeContainer.transform;
        generatedCubeContainer.GetComponent<ChildrenCounter>().list.Add(acube);

        acube.AddComponent<Rigidbody>();
        acube.AddComponent<ProducedCube>().Init(settings.myCubeColor);

    }

    protected void ResetColor()
    {
        if (mMat == null) return;
        mMat.color = originalColor;
    }

    protected void ChangeColor(Color c)
    {
        if (mMat == null) return;
        mMat.SetColor(matColor, c);
    }

    protected void PlaySound()
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

    protected void RandomSetDestination()
    {
        SetRandomTarget(settings.newDestinationRange);
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