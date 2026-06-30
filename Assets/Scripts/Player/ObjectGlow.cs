using UnityEngine;
using System.Collections;
using HighlightingSystem;

public class ObjectGlow : MonoBehaviour {

	private Highlighter h;
	public Color c;

	// Callback: called by Unity once when this object first becomes active
	void Start () {
		gameObject.AddComponent<Highlighter> ();
		h = gameObject.GetComponent<Highlighter> ();
		h.OccluderOn();
	}

	// Callback: called by Unity every frame while mouse cursor is over this object's collider
	void OnMouseOver () {
		h.On (c);
	}
}
