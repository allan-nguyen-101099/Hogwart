using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talker : NPC {

    // Callback: called by NPC.OnMouseDown when player clicks this Talker NPC
    public override void OnClick() {
        Menu.Instance.showPanel("TalkPanel", false).GetComponent<TalkPanel>().showNPCText(Id);
    }
}
