using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatBubbleManager : MonoBehaviour {

	public static ChatBubbleManager main;
	public GameObject chatBubblePrefab;
	public List<SimpleChatBubble> chatBubbleList;

	void Awake () {
		main = this;
	}

	void OnEnable () {
		CameraSwitcher.OnCameraSwitch += ChangeEventCamera;
	}
	void OnDisable () {
		CameraSwitcher.OnCameraSwitch -= ChangeEventCamera;
	}

    public void Speak(Transform host, string content)
    {
        var newSpeech = Instantiate(chatBubblePrefab, Vector3.zero, Quaternion.identity).GetComponent<SimpleChatBubble>();
        newSpeech.gameObject.transform.SetParent(transform);
        newSpeech.Init(content, host);
    }

	void ChangeEventCamera () {
		GetComponent<Canvas> ().worldCamera = Camera.main;
	}
}