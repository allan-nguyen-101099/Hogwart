using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillsUI : MonoBehaviour
{

    public static SkillsUI Instance => _instance ??= FindObjectOfType<SkillsUI>();

    public List<Button> Skills;
    private static SkillsUI _instance;

    // Use this for initialization
    void Start()
    {
        //Instance = this;
    }

    public void displayUnlockedSkills()
    {
        CharacterData data = Player.Instance.characterData;

        // Hide the image of Skill2 (index 1) if not yet unlocked
        // Skill1 (index 0) is always visible
        GameObject skill2 = Skills[1].gameObject;
        skill2.GetComponent<Image>().enabled = data.isSpellUnlocked(1);
    }

    public void unlockSpell(int spellIndex)
    {
        Player.Instance.characterData.unlockSpell(spellIndex);
        Player.Instance.characterData.save();
        Skills[spellIndex].gameObject.GetComponent<Image>().enabled = true;
    }

    public void fillSlots()
    {
        int i = 0;
        int total = PlayerCombat.Instance.spellList.Count;
        SkillTooltip tooltip;
        Spell spell;

        foreach (Button button in Skills)
        {
            tooltip = button.transform.GetComponent<SkillTooltip>();

            if (i < total)
            {
                spell = PlayerCombat.Instance.spellList[i];

                //tooltip.id = spell.id;
                tooltip.name = spell.spellName;
                tooltip.description = spell.spellInfo;
            }
            i++;
        }
    }

    public void execSkill(int num)
    {
        num--;
        PlayerCombat.Instance.spellCast(num);
    }

    public void disableSkill(int num)
    {
        Skills[num].interactable = false;
    }

    public void updateStatus()
    {
        bool enabled = false;

        if (Player.Instance.target)
        {
            enabled = true;
        }

        foreach (Button button in Skills)
        {
            button.interactable = enabled;
        }
    }

    public void enableSkill(int num)
    {
        Skills[num].interactable = true;
    }

    public void toggleBroomStick()
    {
        PlayerHotkeys.Instance.toggleBroomStick();
    }

    public void toggleLight()
    {
        PlayerHotkeys.Instance.toggleLight();
    }
}
