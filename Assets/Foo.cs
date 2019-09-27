using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Foo : Thing
{

    //THIS IS A COMMENT, will be ignored by computer and meant for us humans.
    //text after a double slash is a comment.

    //===========================================================
    //EVENTS, put functions inside the code blocks
    //===========================================================

    //Execute onece when the Thing is spawned
    //Usually for setting up variables
    protected override void ThingAwake()
    {

        //Set flocking variables
        //tips: this is how you call a function and pass a variable into it
        SetAlignmentWeight(1);
        SetCohesionWeight(1);
        SetSeperationWeight(1);

        //10 is the default
        SetMaxSpeed(10);

        //  scale parameters: width, height and depth
        // this is how you do random -> Random.Range(low, high);
        // example: random scale
        // SetScale(new Vector3(Random.Range(1, 3), Random.Range(1, 3), Random.Range(1, 3)));
        SetScale(new Vector3(1, 1, 1));
        //color parameters: Red, Green, BLue -- 0 means NO, 1 means FULL
        ChangeColor(new Color(1, 0, 1));
    }

    //execute once, execute after ThingAwake    
    protected override void ThingStart()
    {
        //produce a chat bubble on top of "me"
        Speak("hey yo!");
    }

    //execute every frame, somewhat between 30-120 times per seconds. 
    //becareful of putting functions here, you might crash the program :)
    protected override void ThingUpdate()
    {

    }



    protected override void OnSunset()
    {
        //spark particles around it. parameter: Color, Amount
        //example: Color.blue or Color.red, or if you want to use RGB: new Color(1,0.2f,1);        
        Spark(Color.blue, 100);
        //create a child, smaller than "me", and less active
        CreateChild();

    }

    protected override void OnSunrise()
    {
        //produce a chat bubble on top of "me"
        Speak("Hey!");
    }

    protected override void OnMeetingSomeone(GameObject other)
    {
        //this is how you chain text with variables
        Speak(other.name + "hello!");

        Spark(Color.black, 10);
    }
    protected override void OnLeavingSomeone(GameObject other)
    {
        CreateChild();
    }

    protected override void OnNeighborSpeaking() { }

    protected override void OnNeigborSparkingParticles() { }


}
