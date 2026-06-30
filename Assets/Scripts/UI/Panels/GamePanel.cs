using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GamePanel : MonoBehaviour, IDragHandler, IDropHandler {

	public static bool isMovingAPanel = false;

	private RectTransform rect;

	// Callback: called by Unity once when this object first becomes active
	public void Start () {
		rect = GetComponent<RectTransform>();
	}

	// Callback: called by Unity when this panel is shown/enabled
	public void OnEnable () {
		if (rect == null) {
			rect = GetComponent<RectTransform>();
		}
		rect.SetAsLastSibling ();
	}

	/**
		Allows dragging the panel through game screen
		@return void
	*/
	// Callback: called by Unity EventSystem every frame while user drags this panel
	public void OnDrag (PointerEventData eventData) {

		rect.position += new Vector3(eventData.delta.x, eventData.delta.y);
		isMovingAPanel = true;
	}

	// Callback: called by Unity EventSystem when user drops onto this panel
	public void OnDrop (PointerEventData eventData) {
		isMovingAPanel = false;
	}

	public void closePanel () {
		gameObject.SetActive (false);
	}
}
