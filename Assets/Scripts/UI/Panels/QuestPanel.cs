using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestPanel : MonoBehaviour
{
    public Text title;
    public Text text;
    public GameObject acceptButton;
    public GameObject completeButton;

    private Quest quest;

    public void setQuest(Quest questData)
    {
        quest = questData;
        title.text = quest.name;

        bool isActiveQuest = QuestManager.Instance.quests.ContainsKey(quest.id);

        if (quest.isCompleted && isActiveQuest) {
            // Quest tasks all done, player is turning it in
            text.text = processText(quest.after);
            acceptButton.SetActive(false);
            completeButton.SetActive(true);
        } else if (isActiveQuest) {
            // Quest accepted, but tasks not yet finished
            text.text = LanguageManager.get("QUEST_NOT_FINISHED");
            acceptButton.SetActive(false);
            completeButton.SetActive(false);
        } else {
            // Quest not yet accepted — show NPC dialogue and accept button
            text.text = processText(quest.pre);
            acceptButton.SetActive(true);
            completeButton.SetActive(false);
        }
    }

    private string processText (string message)
    {
        message = message.Replace("{{username}}", Player.Instance.characterData.name);

        return message;
    }

    public void OnAccept ()
    {
        QuestManager.Instance.addQuest(quest);

        text.text = LanguageManager.get("QUEST_ACCEPTED");
        acceptButton.SetActive(false);

        AudioSource sound = GetComponent<AudioSource>();
        sound.clip = SoundManager.get(SoundManager.Effect.QuestAccept);
        sound.Play();
    }

    public void OnAcceptReward()
    {
        QuestManager.Instance.completeQuest(quest);

        text.text = LanguageManager.get("QUEST_ACCEPTED");
        acceptButton.SetActive(false);

        AudioSource sound = GetComponent<AudioSource>();
        sound.clip = SoundManager.get(SoundManager.Effect.QuestComplete);
        sound.Play();
    }
}