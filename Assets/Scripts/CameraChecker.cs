using UnityEngine;

public class CameraChecker : MonoBehaviour
{
    private float timer = 0f;

    private static Camera cachedCam;

    static int Count = 0;

    public static void SetCamera(Camera cam)
    {
        if (cam == null)
        {
            Debug.Log("[CameraChecker] Attempted to set cached camera to null!");
            return;
        }
        else {
            Debug.Log($"[CameraChecker] Caching MainCamera: {cam.gameObject.name} at position {cam.transform.position}");
        }
        cachedCam = cam;
    }

    void Start()
    {
        Count++;
        Debug.Log($"[CameraChecker] CameraChecker instance created. Total instances: {Count}");
    }

    void Update()
    {
        // Check every 2 seconds
        timer += Time.deltaTime;
        if (timer >= 2f)
        {
            timer = 0f;

            if (cachedCam != null && cachedCam.enabled)
            {
                Debug.Log("[CameraChecker] Cached MainCamera active at " + cachedCam.transform.position);
            }
            else if (cachedCam != null)
            {
                Debug.Log("[CameraChecker] Cached MainCamera found but disabled!");
            }
            else
            {
                Debug.Log("[CameraChecker] Cached MainCamera missing or disabled!");
            }
        }
    }
}
