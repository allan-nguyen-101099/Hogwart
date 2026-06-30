using UnityEngine;
using System.Collections;

public class NPCSpawner : MonoBehaviour {

	// Callback: called by Unity once when this object first becomes active
	void Start () {
		PhotonNetwork.Instantiate("NPC/Spider", transform.position, Quaternion.identity, 0);
	}
}
