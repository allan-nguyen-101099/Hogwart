using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour {

    // Callback: called by Unity once when this script instance is loaded (before Start)
    private void Awake() {
        GetComponent<VideoPlayer>().loopPointReached += EndReached;
    }
	
	// Callback: called by Unity every frame
	void Update () {
		if (Input.anyKeyDown) {
			SceneManager.LoadScene("MainMenu");
		}
	}

	void EndReached(VideoPlayer vp) {
        SceneManager.LoadScene("MainMenu");
	}
}
