using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SkillsUI : MonoBehaviour
{

    public static SkillsUI Instance => _instance ??= FindObjectOfType<SkillsUI>();

    public List<Button> Skills;
    private static SkillsUI _instance;
    private readonly Dictionary<Texture2D, Sprite> _spriteCache = new Dictionary<Texture2D, Sprite>();

    // Use this for initialization
    void Start()
    {
        //Instance = this;
    }

    public void displayUnlockedSkills()
    {
        CharacterData data = Player.Instance.characterData;

        for (int i = 0; i < Skills.Count; i++)
        {
            bool isUnlocked = data.isSpellUnlocked(i);
            SetSkillVisualState(Skills[i], isUnlocked);
        }

        updateStatus();
    }

    public void unlockSpell(int spellIndex)
    {
        Player.Instance.characterData.unlockSpell(spellIndex);
        Player.Instance.characterData.save();

        if (spellIndex >= 0 && spellIndex < Skills.Count)
        {
            SetSkillVisualState(Skills[spellIndex], true);
        }

        updateStatus();
    }

    public void fillSlots()
    {
        Debug.Log($"[SkillsUI] fillSlots called. Skills: {Skills.Count}, Spells: {PlayerCombat.Instance.spellList.Count}");

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
                if (tooltip != null)
                {
                    tooltip.skillName = spell.spellName;
                    tooltip.description = spell.spellInfo;
                }

                ApplySpellIcon(button, spell, i);

                bool isUnlocked = Player.Instance != null && Player.Instance.characterData != null && Player.Instance.characterData.isSpellUnlocked(i);
                SetSkillVisualState(button, isUnlocked);
            }
            i++;
        }

        updateStatus();
    }

    private void ApplySpellIcon(Button button, Spell spell, int slotIndex)
    {
        if (spell.spellIcon == null)
        {
            Debug.LogWarning($"[SkillsUI] Slot {slotIndex} ({button.name}) spell '{spell.spellName}' has null spellIcon");
            return;
        }

        var rootImage = button.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.sprite = GetOrCreateSprite(spell.spellIcon);
            rootImage.preserveAspect = true;
            rootImage.enabled = true;
            Debug.Log($"[SkillsUI] Slot {slotIndex}: assigned icon to ROOT Image '{button.name}'");
        }

        var namedIcon = button.transform.Find("Icon");
        if (namedIcon != null)
        {
            var namedRaw = namedIcon.GetComponent<RawImage>();
            if (namedRaw != null)
            {
                namedRaw.texture = spell.spellIcon;
                namedRaw.enabled = true;
                Debug.Log($"[SkillsUI] Slot {slotIndex}: assigned icon to RawImage on child 'Icon'");
                return;
            }

            var namedImage = namedIcon.GetComponent<Image>();
            if (namedImage != null)
            {
                namedImage.sprite = GetOrCreateSprite(spell.spellIcon);
                namedImage.preserveAspect = true;
                namedImage.enabled = true;
                Debug.Log($"[SkillsUI] Slot {slotIndex}: assigned icon to Image on child 'Icon'");
                return;
            }
        }

        var childRaw = button.GetComponentsInChildren<RawImage>(true).FirstOrDefault();
        if (childRaw != null)
        {
            childRaw.texture = spell.spellIcon;
            childRaw.enabled = true;
            Debug.Log($"[SkillsUI] Slot {slotIndex}: assigned icon to fallback RawImage '{childRaw.gameObject.name}'");
            return;
        }

        var childImage = button.GetComponentsInChildren<Image>(true)
            .FirstOrDefault(img => img.gameObject != button.gameObject);
        if (childImage != null)
        {
            childImage.sprite = GetOrCreateSprite(spell.spellIcon);
            childImage.preserveAspect = true;
            childImage.enabled = true;
            Debug.Log($"[SkillsUI] Slot {slotIndex}: assigned icon to fallback child Image '{childImage.gameObject.name}'");
            return;
        }

        if (rootImage != null)
        {
            return;
        }

        Debug.LogWarning($"[SkillsUI] Slot {slotIndex} ({button.name}) has no RawImage/Image target for icon");
    }

    private Sprite GetOrCreateSprite(Texture2D texture)
    {
        if (texture == null)
        {
            return null;
        }

        if (_spriteCache.TryGetValue(texture, out var cached))
        {
            return cached;
        }

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        _spriteCache[texture] = sprite;
        return sprite;
    }

    public void execSkill(int num)
    {
        num--;

        if (!Player.Instance.characterData.isSpellUnlocked(num))
        {
            return;
        }

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
            int index = Skills.IndexOf(button);
            bool isUnlocked = Player.Instance.characterData.isSpellUnlocked(index);
            button.interactable = enabled && isUnlocked;
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

    private static void SetSkillVisualState(Button button, bool isUnlocked)
    {
        var rootImage = button.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.enabled = isUnlocked;
        }

        var childRaw = button.GetComponentsInChildren<RawImage>(true).FirstOrDefault();
        if (childRaw != null)
        {
            childRaw.enabled = isUnlocked;
        }

        var childImage = button.GetComponentsInChildren<Image>(true)
            .FirstOrDefault(img => img.gameObject != button.gameObject);
        if (childImage != null)
        {
            childImage.enabled = isUnlocked;
        }
    }
}
