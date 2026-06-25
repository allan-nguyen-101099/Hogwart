using UnityEngine;
using System.Collections.Generic;

public class Quester : NPC
{
    public override void OnClick()
    {
        List<int> questIds = QuestManager.Instance.getByNPC(Id);

        if (questIds.Count == 0) {
            return;
        }

        // Find the first quest that hasn't been fully rewarded yet
        Quest questToShow = null;
        foreach (int qid in questIds) {
            if (!QuestManager.Instance.allQuests.ContainsKey(qid)) continue;
            Quest q = QuestManager.Instance.allQuests[qid];
            // Skip quests that are completed AND already removed from active quests (fully done)
            if (q.isCompleted && !QuestManager.Instance.quests.ContainsKey(qid)) continue;
            questToShow = q;
            break;
        }

        if (questToShow == null) {
            return;
        }

        GameObject panel = Menu.Instance.showPanel("QuestPanel", false);
        panel.GetComponent<QuestPanel>().setQuest(questToShow);
    }
}