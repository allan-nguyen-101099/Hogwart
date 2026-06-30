using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	public List<GameObject> Menus;
	public GameObject ItemTooltipPanel;
	public GameObject SkillTooltip;

	public static string defaultLevel = "Hogwarts";
	public static string debugLevel = "Test";
	public const string GAME_VERSION = "0.01"; // remember to also change latest_build.txt

	public static Menu _instance;
	
	public static Menu Instance {
		get
		{
			return _instance;
		}
	}

	private void EnsureMenusInitialized()
	{
		if (Menus == null)
		{
			Menus = new List<GameObject>();
		}

		if (Menus.Count > 0)
		{
			return;
		}

		foreach (Transform child in transform)
		{
			if (child == null)
			{
				continue;
			}

			Menus.Add(child.gameObject);
		}

		if (Menus.Count == 0)
		{
			Debug.LogWarning("[Menu] Menus list is empty and no child panels were found. Assign Menus in Inspector.");
		}
	}

    // Callback: called by Unity once when this script instance is loaded (before Start)
    void Awake() {
        DontDestroyOnLoad(transform.gameObject);
        DontDestroyOnLoad(GameObject.Find("EventSystem"));
		EnsureMenusInitialized();

        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        _instance = this;
    }

    public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {

        switch (scene.name) {
		    case "MainMenu": // Main
			    TryShowPanel("MainPanel");
			    break;
		    default:
                TryShowPanel("PlayerPanel");
			    TryShowPanel("ChatPanel", false);
			    TryShowPanel("TopMenu", false);
			    TryShowPanel("MiniMap", false);
                // Canvas Scaler was making the bags and menus look broken
                //gameObject.GetComponent<CanvasScaler>().enabled = false;
                break;
		}
	}

	private bool TryShowPanel(string name, bool hidePanels = true)
	{
		try
		{
			showPanel(name, hidePanels);
			return true;
		}
		catch (UnityException ex)
		{
			Debug.LogWarning("[Menu] " + ex.Message);
			return false;
		}
	}

	public void showPanelVoid (string name) {
		showPanel(name);
	}
	
	public GameObject showPanel (string name, bool hidePanels = true) {
		EnsureMenusInitialized();

		if (hidePanels) {
			hideAllPanels ();
		}
		
		GameObject panel = this.getPanel (name);
		panel.SetActive (true);
		
		return panel;
	}

	public GameObject getPanel (string name) {
		EnsureMenusInitialized();

		foreach (GameObject panel in Menus) {
			if (panel != null && panel.name == name) {
				return panel;
			}
		}

		GameObject fallbackPanel = GameObject.Find("Canvas/" + name);
		if (fallbackPanel != null)
		{
			Menus.Add(fallbackPanel);
			return fallbackPanel;
		}

		throw new UnityException ("UI Panel "+ name +" not found");
	}
	
	public void togglePanel (string name) {
		GameObject panel = getPanel (name);
		
		panel.SetActive (!panel.GetActive());
	}
	
	public void hideAllPanels() {
		EnsureMenusInitialized();

		foreach (GameObject panel in Menus) {
			if (panel != null) {
				panel.SetActive(false);
			}
		}
	}

	/**
		Shows a tooltip near to item slot
		@param Vector3 pos Position to show the tooltip
		@param Item item Item which we want to show its information

		@return void
	 */
	public void showTooltip (Vector3 pos, Item item) {
		ItemTooltipPanel.SetActive (true);
		ItemTooltipPanel.GetComponent<RectTransform> ().SetAsLastSibling ();
		ItemTooltipPanel.transform.position = pos;
		
		ItemTooltipPanel.transform.Find("TitleLabel").GetComponent<Text>().text = item.name;
		ItemTooltipPanel.transform.Find("TextLabel").GetComponent<Text>().text = item.description;
	}

	public void showSkillTooltip(string description, string cooldown){
		SkillTooltip.transform.Find ("Description").GetComponent<Text> ().text = description;
		SkillTooltip.transform.Find ("Cooldown").GetComponent<Text> ().text = cooldown;
		SkillTooltip.GetComponent<RectTransform> ().sizeDelta = new Vector2 (416, SkillTooltip.transform.Find ("Description").GetComponent<RectTransform> ().sizeDelta.y + 12);
		SkillTooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(Input.mousePosition.x ,68);
		SkillTooltip.SetActive (true);
	}
	
	/**
		hides the tooltip

		@return void
	*/
	public void hideTooltip () {
		ItemTooltipPanel.SetActive (false);
	}
}