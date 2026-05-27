using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class CreatePanel : MonoBehaviour {

	public void createButton () {
		// Load the Hogwarts scene
		SceneManager.LoadScene("Hogwarts");
	}
}
