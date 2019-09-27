# This is a new version made for NEW INC's bootcamp.

...kind of a WIP thing...

# Steps:

1. Download a text editor [choose one]

   - [sublime](https://www.sublimetext.com/3)
   - [atom](https://atom.io/)
   - [visualstudiocode](https://code.visualstudio.com/)

     if you absolutely do not want to download&install anything, you can use the built-in Notebook(windows) or TextEdit(mac). they just won't highlight syntax for you, thus a bit more easier to make syntax mistakes.

2. Download the template script

3. Change the class's name AND the file name, make sure they are exactly the same.
   
   if your class name is *Bar*, then your file name needs to be *Bar.cs*

4. Start coding your Thing's behaviour

5. Upload your script to this [Google Drive Folder](https://drive.google.com/drive/folders/1Wnx_6O_GamQsRJSwt4HX6tk0XkiqEUYd?usp=sharing)

6. Now wait us to compile the final simulation, we can send you a copy of it.

# Technical Document

## Ready to Use Methods

```csharp

//Movement

SetMaxSpeed(10);//max speed , 10 is the default.

//By default all Thigns paricipate in the group flocking behaviour
//you can set the three key variables' weight(multiplier) to change its social flocking patterns
//use value 0 to 3, out of range value will not crash the program but will produce strange behaviours
//1. Alignment: Alignment is a behavior that causes a particular agent to line up with agents close by.
SetAlignmentWeight(1);
//2. Cohesion:
//Cohesion is a behavior that causes agents to steer towards the "center of mass" - that is, the average position of the agents within a certain radius.
SetCohesionWeight(1);
//3. Separation:
//Separation is the behavior that causes an agent to steer away from all of its neighbors.
SetSeperationWeight(1);

StopMoving(); //Stop paricipating in flocking
StopMoving(float forHowManySeconds); //stop pariipating in flocking for x seconds, then it will resume automatically
RestartWalking(); //resume flocking

//Shape and form
SetScale(Vector3 newScale); //size of your Thing. you need to set its width, height and depth.
ChangeColor(Color newColor); //change color, might not work well if you have more than one renderer or more than one material 更改颜色

//social
Speak(string content, float stayLength);
Speak(string content);
Mute(); //Speak no longer works //开始沉默，不再讲话
DeMute(); //regain ability to Speak again //不再沉默
Spark(Color particleColor, int numberOfParticles);

//Reproduce
CreateChild();//Produce a similar looking Thing, it won't interact with anyone or say/spark.

```

## Properties/Fields For You To GET

```csharp
//Environment
float TOD_Data.main.TimeNow; //e.x. 3:30PM will be represented as 15.5
bool TOD_Data.main.IsDay;
bool TOD_Data.main.IsNight;
int NeighborCount; //how many neighbors do you have currently
```

## Events You can Use to Fill Code Inside

```csharp
protected override void OnSunset(){}//日落时
protected override void OnSunrise(){}//日出时
protected override void OnMeetingSomeone(GameObject other){}//碰到其他物时
protected override void OnLeavingSomeone(GameObject other){}//离开了其他物时
protected override void OnNeighborSpeaking(){}//有邻居说话时
protected override void OnNeigborSparkingParticles(){}//有邻居发出发光时
```

# Tips

## Some Basic about C# Programming

[If Else Statement](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/if-else)
AND
[For Loop](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/for)

OR

[Everything Else About C#](https://docs.microsoft.com/en-us/dotnet/csharp/index)

## Some useful methods from Unity

list a few here, but please refer back to link above for more.

```csharp
//Print things to Console for debugging
print(object message);

//Invokes the method methodName in time seconds.
Invoke(string methodName, float time);

//Invokes the method methodName in time seconds, then repeatedly every repeatRate seconds.
InvokeRepeating(string methodName, float time, float repeatRate);

//Cancels all Invoke calls on this MonoBehaviour.
CancelInvoke();
```
