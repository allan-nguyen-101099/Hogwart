using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItem : MonoBehaviour {

    [Tooltip("Item id")]
    public int id;
    public int quantity = 1;
    [Tooltip("Seconds, Set to 0 to not hide this object after a player click")]
    public int respawnAfter = 30;

    private bool isHidden = false;

    // Callback: called by Unity once when this object first becomes active
    public void Start ()
    {
        if (id < 1 || quantity < 1) {
            Debug.LogError("There is a QuestItem in this scene without a proper id/quantity set");
        }
    }

    // Callback: called by Unity when player clicks this quest item with the mouse
    public void OnMouseDown()
    {
        if (isHidden) {
            return;
        }

        Player.Instance.addItem(id, quantity);

        if (respawnAfter > 0) {
            this.gameObject.GetComponent<MeshRenderer>().enabled = false;
            isHidden = true;
            StartCoroutine(respawn());
        }
    }

    private IEnumerator respawn ()
    {
        yield return new WaitForSeconds(respawnAfter);

        this.gameObject.GetComponent<MeshRenderer>().enabled = true;
        isHidden = false;
    }
}
