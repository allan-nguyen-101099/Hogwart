using UnityEngine;
using System.Collections;

public class TimedObjectDestruction : MonoBehaviour {

	public float timeOut = 1.0f;
	public bool detachChildren = false;
	
	// Callback: called by Unity once when this script instance is loaded (before Start)
	public void Awake ()
	{
		Invoke ("DestroyNow", timeOut);
	}
	
	public void DestroyNow ()
	{
		if (detachChildren) {
			transform.DetachChildren ();
		}
		Destroy(gameObject);
	}
}
