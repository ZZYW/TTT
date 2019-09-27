using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YW : Thing
{
    protected override void ThingAwake()
    {
        settings.acceleration = 4;
        settings.newDestinationRange = 40;
        settings.neighborDetectorRadius = 10;
    
        //scale parameters: width, height and depth
        SetScale(new Vector3(Random.Range(1,3), Random.Range(1,3), Random.Range(1,3)));
        //color parameters: Red, Green, BLue -- 0 means NO, 1 means FULL
        ChangeColor(new Color(1, 0, 1));
    }

    protected override void ThingStart()
    {
        Speak("hey yo!");
        PlaySound();
    
    }

    protected override void ThingUpdate()
    {

    }

    protected override void OnSunset()
    {
        Spark(Color.blue, 100);
        //Speak("sad day..");
        CreateChildren();
     
    }

    protected override void OnSunrise()
    {
        Speak("Hey!");
    }

    protected override void OnMeetingSomeone(GameObject other) {
        Speak(other.name + "hello!");
        Spark(Color.black, 10);
    }
    protected override void OnLeavingSomeone(GameObject other) {
        CreateChildren();
    }
    protected override void OnNeighborSpeaking() { }
    protected override void OnNeigborSparkingParticles() { }

}
