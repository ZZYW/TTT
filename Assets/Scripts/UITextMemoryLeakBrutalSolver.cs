using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITextMemoryLeakBrutalSolver : MonoBehaviour {

	[SerializeField] GameObject TTTConsoleGameobject;
	[SerializeField] GameObject ChatBubbleCanvasGameobject;

	[Header ("Prefab to be reinstantiated")]
	[SerializeField] GameObject TTTConsolePrefab;
	[SerializeField] GameObject ChatBubbleManagerPrefab;

	// public float TConsoleDestroyInterval = 3000;
	public float TConsoleRebornWait = 2;

	// public float ChatBubbleDestroyInterval = 300;
	public float ChatbubbleRebornWait = 10;

	// Use this for initialization
	void Start () {
		InvokeRepeating ("DeleteAllCanvases", 120, 120);
		// InvokeRepeating ("DestroyChatBubbles", ChatBubbleDestroyInterval, ChatBubbleDestroyInterval);
	}
    

}