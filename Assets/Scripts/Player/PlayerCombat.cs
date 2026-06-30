using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour {

	public static PlayerCombat Instance;
	public bool castingSpell = false;
	private float spellAnimTime = 1.6f;
	
	public List<Spell> spellList = new List<Spell> ();

	// Callback: called by Unity once when this object first becomes active
	void Start () {
		Instance = this;

		// TMP TEMPORAL THIS SHOULD COME FROM DB
		spellList.Add((Spell)Resources.Load("Spells/Fireball"));
		spellList.Add((Spell)Resources.Load("Spells/Frostbolt"));
		spellList.Add((Spell)Resources.Load("Spells/Area test"));
		
		// Safely update UI if it exists
		if (SkillsUI.Instance != null) {
			SkillsUI.Instance.fillSlots();
		} else {
			Debug.Log("SkillsUI.Instance not found during PlayerCombat initialization");
		}
	}

	// for numeric hotkeys
	public void spellCast(int key)
	{
		Debug.Log($"[PlayerCombat.spellCast] Called with key index: {key}");

		if (!Player.Instance.characterData.isSpellUnlocked(key)) {
			Debug.Log($"[PlayerCombat.spellCast] Spell index {key} is locked.");
			return;
		}
		
		if (Player.Instance.target == null) {
			Debug.LogError("[PlayerCombat.spellCast] NO TARGET! Returning without casting.");
			return;
		}
		
		if (key < 0 || key >= spellList.Count) {
			Debug.LogError($"[PlayerCombat.spellCast] Invalid spell index: {key}, spellList count: {spellList.Count}");
			return;
		}
		
		Spell spell = spellList[key];
		Debug.Log($"[PlayerCombat.spellCast] Spell: {spell.spellName}, Mana Cost: {spell.spellManaCost}, Min Level: {spell.minLevel}");
		Debug.Log($"[PlayerCombat.spellCast] Player Mana: {Player.Instance.mana}/{Player.Instance.maxMana}, Player Level: {Player.Instance.level}");
		
		StartCoroutine(SpellCast(spellList[key], key));
	}

	public IEnumerator SpellCast(Spell spell, int uiPos)
	{
        Debug.Log($"[PlayerCombat.SpellCast] Started casting {spell.spellName}");
        
        if (spell.spellManaCost > Player.Instance.mana) {
            Debug.LogError($"[PlayerCombat.SpellCast] NOT ENOUGH MANA! Need: {spell.spellManaCost}, Have: {Player.Instance.mana}");
            yield break;
        }
        
        if (spell.minLevel > Player.Instance.level) {
            Debug.LogError($"[PlayerCombat.SpellCast] LEVEL TOO LOW! Need: {spell.minLevel}, Have: {Player.Instance.level}");
            yield break;
        }

		SkillsUI.Instance.disableSkill(uiPos);
		castingSpell = true;
		Player.Instance.mana -= spell.spellManaCost;
        Debug.Log($"[PlayerCombat.SpellCast] Mana deducted. New mana: {Player.Instance.mana}");

		// Wait for choosen spell cast time.
		Player.Instance.anim.SetBool("InvokeSpell", true);
        Debug.Log($"[PlayerCombat.SpellCast] Anim InvokeSpell set to true, cast time: {spell.spellCastTime}");
        PlayerPanel.Instance.castingPanel.Cast(spell.spellName, spell.spellCastTime);
		yield return new WaitForSeconds(spell.spellCastTime-spellAnimTime);

		Player.Instance.anim.SetInteger("SpellType", 1);
		Player.Instance.anim.SetBool("InvokeSpell", false);
        Debug.Log($"[PlayerCombat.SpellCast] Animation parameters updated");

		yield return new WaitForSeconds(spellAnimTime);

		// Set up a spell and cast it.
		SpellSetUp(spell);
        Debug.Log($"[PlayerCombat.SpellCast] SpellSetUp called for {spell.spellName}");

		SkillsUI.Instance.enableSkill(uiPos);
		
		castingSpell = false;
        Debug.Log($"[PlayerCombat.SpellCast] SPELL CAST COMPLETE!");
	}

	void SpellSetUp(Spell spell)
	{
		
		if(spell.spellPrefab == null)
		{
			Debug.Log("Spell prefab is null.Assign a spell prefab.");
			return;
			
		}
		
		GameObject spellObject = null;
		
		//We will find what type of spell we are using
		//
		//********************************SINGLE*********************************************
		if(spell.spellType == Spell.SpellType.Single)
		{
			//Instantiated object will move straight forward.
			if(spell.spellDirection == Spell.SpellDirection.Directional)
			{
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name, transform.position + transform.up, transform.rotation, 0);
				spellObject.name = spell.spellName;
				
			}
			
			//Instantiated object will follow target.
			if(spell.spellDirection == Spell.SpellDirection.Follow)
			{
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,transform.position + transform.up, Quaternion.identity, 0);
				spellObject.name = spell.spellName;
				spellObject.GetComponent<SpellObjectConfigurator>().myTarget = Player.Instance.target.transform;
			}
			
			//Instantiating to target's position.
			if(spell.spellDirection == Spell.SpellDirection.Point)
			{
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,Player.Instance.target.transform.position,Quaternion.identity, 0);
				spellObject.name = spell.spellName;
				spellObject.GetComponent<SpellObjectConfigurator>().myTarget = Player.Instance.target.transform;
			}
			
			
		}
		
		//********************************AOE*********************************************
		else if(spell.spellType == Spell.SpellType.Aoe)
		{
			if(spell.spellPosition == Spell.SpellPosition.TargetTransform)
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,Player.Instance.target.transform.position,Quaternion.identity, 0);
			else
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,transform.position,Quaternion.identity, 0);
			
			spellObject.name = spell.spellName;
			
			
		}
		
		//********************************BUFF*********************************************
		else
		{
			//Spell type is a buff.And we are checking what type of buff spell is used.
			if(spell.buffType == Spell.BuffType.Heal)
			{
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,transform.position,Quaternion.identity, 0);
				spellObject.name = spell.spellName;
				//currentHealth += (Random.Range(spell.minBuffAmount,spell.maxBuffAmount));	
				
			}
			else if(spell.buffType == Spell.BuffType.MagicalDefense)
			{
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,transform.position,Quaternion.identity, 0);
				spellObject.name = spell.spellName;
				//magicalDefense += (Random.Range(spell.minBuffAmount,spell.maxBuffAmount));	
				
			}
			else
			{
				//Physical Defense
				spellObject = (GameObject)PhotonNetwork.Instantiate("Particles/" + spell.spellPrefab.name,transform.position,Quaternion.identity, 0);
				spellObject.name = spell.spellName;
				//physicalDefense += (Random.Range(spell.minBuffAmount,spell.maxBuffAmount));	
			}
		}
	}
}
