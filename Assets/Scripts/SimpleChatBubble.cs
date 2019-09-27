using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleChatBubble : MonoBehaviour {

	public Transform host;

	[SerializeField] private Text text;
	[SerializeField] private Image bubble;


    public void Init(string content, Transform host)
    {
        this.host = host;
        text.text = content;
        Invoke("SelfDestroy", 4f);
    }

    void Update () {
		transform.position = host.position + Vector3.up * host.localScale.y * 3;
		transform.LookAt (CameraSwitcher.main.ActiveCam.position);
	}
    
	void SelfDestroy() {
        Destroy(gameObject);
	}
}