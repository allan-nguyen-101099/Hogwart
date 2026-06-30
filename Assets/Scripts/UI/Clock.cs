using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Clock : MonoBehaviour {
	
	// Callback: called by Unity every frame
	void Update () {

		gameObject.GetComponent<Text>().text = System.DateTime.Now.ToString ("HH:mm");
	}
}
