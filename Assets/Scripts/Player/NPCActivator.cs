using UnityEngine;
using System.Collections;

/*
Collides with NPCs (using a FOV width sphere) to enable/disable them
*/

public class NPCActivator : MonoBehaviour
{

    void OnTriggerStay(Collider col)
    {
        if (col.tag != "NPC" || col.isTrigger) {
            return;
        }

        // Just enable the NPC. Master client owns all NPC logic.
        col.gameObject.GetComponent<NPC>().setEnabled(true);
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag != "NPC" || col.isTrigger) {
            return;
        }

        // Disable NPC when player leaves range
        col.gameObject.GetComponent<NPC>().setEnabled(false);
    }
}
