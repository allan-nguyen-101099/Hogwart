using UnityEngine;
using System.Collections;

/*
Collides with NPCs (using a FOV width sphere) to enable/disable them
*/

public class NPCActivator : MonoBehaviour
{

    // Callback: called by Unity every frame while an NPC is inside the player's activation sphere
    void OnTriggerStay(Collider col)
    {
        if (col.tag != "NPC" || col.isTrigger) {
            return;
        }

        // Just enable the NPC. Master client owns all NPC logic.
        col.gameObject.GetComponent<NPC>().setEnabled(true);
    }

    // Callback: called by Unity when an NPC exits the player's activation sphere
    void OnTriggerExit(Collider col)
    {
        if (col.tag != "NPC" || col.isTrigger) {
            return;
        }

        // Disable NPC when player leaves range
        col.gameObject.GetComponent<NPC>().setEnabled(false);
    }
}
