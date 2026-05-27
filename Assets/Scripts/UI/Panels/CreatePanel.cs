using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreatePanel : MonoBehaviour {

	public InputField nameInput;
	public Dropdown houseDropdown;
	public Text errorText;

	public void createButton () {
		string characterName = nameInput.text.Trim();

		// Validate name
		if (string.IsNullOrEmpty(characterName)) {
			showError("Please enter a character name.");
			return;
		}

		if (characterName.Length < 3) {
			showError("Name must be at least 3 characters.");
			return;
		}

		// Initialize DB if needed
		if (Service.db.SelectCount("FROM item") < 1) {
			DBSetup.start();
		}

		// Create character data
		CharacterData character = new CharacterData();
		character.id = 1;
		character.name = characterName;
		character.model = "Default";
		character.level = 1;
		character.house = houseDropdown != null ? houseDropdown.value : 0;
		character.health = 270;
		character.maxHealth = 270;
		character.mana = 100;
		character.maxMana = 100;
		character.exp = 0;
		character.money = 100;
		character.position = "(633.51, 161.38, 415.70)";

		bool success = character.create();

		if (!success) {
			showError("Failed to create character. Try a different name.");
			return;
		}

		// Go back to MainPanel which will now show the Join button for the new character
		Menu.Instance.showPanel("MainPanel");
	}

	private void showError(string message) {
		if (errorText != null) {
			errorText.text = message;
			errorText.gameObject.SetActive(true);
		} else {
			Debug.LogError(message);
		}
	}
}
