using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CreatePanel : MonoBehaviour {

	public InputField nameInput;
	public Dropdown houseDropdown;
	public Text errorText;
	public Button createCharacterButton;

	void OnEnable() {
		// Auto-find UI elements if not assigned
		if (nameInput == null) {
			nameInput = GetComponentInChildren<InputField>();
			if (nameInput == null) {
				nameInput = GameObject.Find("Canvas/CreatePanel/NameInput")?.GetComponent<InputField>();
			}
		}
		if (houseDropdown == null) {
			houseDropdown = GetComponentInChildren<Dropdown>();
			if (houseDropdown == null) {
				houseDropdown = GameObject.Find("Canvas/CreatePanel/HouseDropdown")?.GetComponent<Dropdown>();
			}
		}
		if (errorText == null) {
			errorText = GetComponentInChildren<Text>();
			if (errorText == null) {
				errorText = GameObject.Find("Canvas/CreatePanel/ErrorText")?.GetComponent<Text>();
			}
		}
		if (createCharacterButton == null) {
			createCharacterButton = GetComponentInChildren<Button>();
			if (createCharacterButton == null) {
				createCharacterButton = GameObject.Find("Canvas/CreatePanel/CreateButton")?.GetComponent<Button>();
			}
		}

		if (createCharacterButton != null) {
			createCharacterButton.interactable = true;
		}
	}

	public void createButton () {
		// Re-check for UI elements
		if (nameInput == null) {
			nameInput = GetComponentInChildren<InputField>();
		}
		
		if (nameInput == null) {
			Debug.LogError("Name Input field not found! Cannot create character.");
			return;
		}

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

		// This game supports one local character profile; replace stale rows to avoid insert conflicts.
		foreach (CharacterData existing in Service.db.Select<CharacterData>("FROM characters").ToList()) {
			Service.db.Delete("characters", existing.id);
		}

		bool success = character.create();

		if (!success) {
			// Fallback: if id 1 still exists due a stale DB state, update it.
			CharacterData existingById = Service.db.SelectKey<CharacterData>("characters", character.id);
			if (existingById != null) {
				character.save();
				success = true;
			}
		}

		if (!success) {
			showError("Failed to create character. Please clear existing save data and try again.");
			if (createCharacterButton != null) {
				createCharacterButton.interactable = true;
			}
			return;
		}

		if (createCharacterButton != null) {
			createCharacterButton.interactable = false;
		}
		showError("Connecting...");

		// Auto-start the game immediately after character creation
		Hashtable h = new Hashtable(1);
		h.Add("characterId", character.id);
		PhotonNetwork.player.SetCustomProperties(h);
		PhotonNetwork.player.NickName = character.name;
		NetworkManager.Instance.startConnection();
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
