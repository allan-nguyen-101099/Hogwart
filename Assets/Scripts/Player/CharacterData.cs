public class CharacterData {

	protected string TABLE_NAME = "characters";
	
	public int id;
	public string name;
	public string model;
	public string position;
	public int level;
	public int house;
	
	public int health;
	public int maxHealth;
	public int mana;
	public int maxMana;
	public int exp;
	
	public int money;

	// Bitmask: bit 0 = spell 1, bit 1 = spell 2, etc. Spell 1 always unlocked.
	public int unlockedSpells = 1;

	public bool isSpellUnlocked(int spellIndex) {
		return (unlockedSpells & (1 << spellIndex)) != 0;
	}

	public void unlockSpell(int spellIndex) {
		unlockedSpells |= (1 << spellIndex);
	}

	public void save () {
		Service.db.Update (TABLE_NAME, this);
	}

	public bool create () {
		return Service.db.Insert (TABLE_NAME, this);
	}
}
