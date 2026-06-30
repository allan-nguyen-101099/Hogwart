using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CastingPanel : MonoBehaviour {

	private static CastingPanel _instance;
	public static CastingPanel Instance => _instance ??= FindObjectOfType<CastingPanel>();
    public Text name;
	public Image bar;
	private float skillTime = 10f;
	private float curTime = 10f;
	public bool isCasting = false;

	// Callback: called by Unity once when this object first becomes active
	void Start () {
		//Instance = this;
	}
	
	// Callback: called by Unity every frame
	void Update () {
			curTime -= Time.deltaTime;
			bar.fillAmount = curTime / skillTime;
		if (curTime / skillTime <= 0) {
			gameObject.SetActive (false);
			isCasting = false;
		} else {
			isCasting = true;
		}
	}

	public void Cast(string n, float t){
		gameObject.SetActive (true);
		name.text = n;
		skillTime = t;
		curTime = t;
	}
}
