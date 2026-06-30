using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainPanel : MonoBehaviour {
    public Text nickLabel;
	public Text LevelLabel;
	public Button JoinButton;
	public Button NewGameButton;

	private int playerId;

	// Callback: called by Unity when this panel is shown/enabled
	public void OnEnable () {
        bool hasPlayer = false;

		if (Service.db.SelectCount("FROM item") < 1) {
			DBSetup.start();
		}

        //NetworkManager.validateGameVersion();

        // @ToDo: create a UI for selection
        foreach (CharacterData character in Service.db.Select<CharacterData>("FROM characters")) {
			hasPlayer = true;
			playerId = character.id;
         
            nickLabel.text = character.name;
			LevelLabel.text = character.level.ToString();
			JoinButton.onClick.RemoveAllListeners();
			JoinButton.onClick.AddListener(
				delegate {
				this.joinGame(character.id, character.name);
			});
			break;
		}
		if (hasPlayer) {
			nickLabel.transform.gameObject.SetActive(true);
			LevelLabel.transform.gameObject.SetActive(true);
			JoinButton.transform.gameObject.SetActive(true);
			
			// Setup New Game button
			if (NewGameButton == null)
			{
				NewGameButton = GameObject.Find("Canvas/MainPanel/LoginOptions/NewGameButton")?.GetComponent<Button>();
			}
			// if (NewGameButton != null)
			// {
				NewGameButton.onClick.RemoveAllListeners();
				NewGameButton.onClick.AddListener(showNewGameConfirm);
				NewGameButton.gameObject.SetActive(true);
			// }
			
			#if UNITY_EDITOR
			GameObject.Find ("Canvas/MainPanel/LoginOptions/TestButton").SetActive(true);
			#endif

			GameObject.Find ("Canvas/MainPanel/LoginOptions/CreateButton").SetActive(false);
		} else {
			nickLabel.transform.gameObject.SetActive(false);
			LevelLabel.transform.gameObject.SetActive(false);
			JoinButton.transform.gameObject.SetActive(false);
			if (NewGameButton != null) NewGameButton.gameObject.SetActive(false);
			GameObject.Find ("Canvas/MainPanel/LoginOptions/TestButton").SetActive(false);
		}
		 
	}

	public void joinGame (int characterId, string name) {

		if (characterId < 1) {
			return;
		}

        Hashtable h = new Hashtable(1);
		h.Add("characterId", characterId);

		PhotonNetwork.player.SetCustomProperties(h);
		PhotonNetwork.player.NickName = name;
		
		NetworkManager.Instance.startConnection();
		GameObject.Find ("Canvas/MainPanel/LoginOptions/JoinButton/Text").GetComponent<Text> ().text = LanguageManager.get("CONNECTING") + "...";
	}

	public void joinTest () {
		Menu.defaultLevel = Menu.debugLevel;
		joinGame (playerId, "Tester");
	}

	public void showNewGameConfirm()
	{
		// Show confirmation dialog
		if (ConfirmationPanel.Instance != null)
		{
			ConfirmationPanel.Instance.Show(
				"Start New Game?\n\nThis will delete your current character and all progress.",
				() => clearDatabase(),
				() => { } // On cancel, do nothing
			);
		}
		else
		{
			Debug.LogError("[MainPanel] ConfirmationPanel not found!");
		}
	}

	private void clearDatabase()
	{
		try
		{
			Debug.Log("[MainPanel] Clearing all game data...");
			
			// Delete character by its actual key (id = 1)
			try
			{
				Service.db.Delete("characters", 1);
				Debug.Log("[MainPanel] Cleared table: characters");
			}
			catch (System.Exception ex)
			{
				Debug.Log($"[MainPanel] Could not clear table 'characters': {ex.Message}");
			}

			// Delete all tasks by their actual taskId keys
			try
			{
				foreach (Task task in Service.db.Select<Task>("FROM tasks"))
				{
					Service.db.Delete("tasks", task.taskId);
				}
				Debug.Log("[MainPanel] Cleared table: tasks");
			}
			catch (System.Exception ex)
			{
				Debug.Log($"[MainPanel] Could not clear table 'tasks': {ex.Message}");
			}

			// Delete all inventory items by their actual item keys
			try
			{
				foreach (CharacterItem item in Service.db.Select<CharacterItem>("FROM inventory"))
				{
					Service.db.Delete("inventory", item.item);
				}
				Debug.Log("[MainPanel] Cleared table: inventory");
			}
			catch (System.Exception ex)
			{
				Debug.Log($"[MainPanel] Could not clear table 'inventory': {ex.Message}");
			}
			
			Debug.Log("[MainPanel] Database cleared successfully!");
			
			// Navigate directly to character creation screen
			Menu.Instance.showPanel("CreatePanel");
		}
		catch (System.Exception ex)
		{
			Debug.LogError($"[MainPanel] Unexpected error clearing database: {ex.Message}\n{ex.StackTrace}");
		}
	}
}
