using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraCollisionSpeed = 2f;

    public Transform cameraTarget;

    public float cameraTargetHeight = 1.0f;
    private float correctedDistance;
    private float currentDistance;
    public float desiredDistance;
    private float distance = 6;
    private bool isHitting;
    public float lastDistance;
    private readonly int lerpRate = 5;

    public float maxViewDistance = 25;
    public float minViewDistance = 1;

    private readonly int mouseXSpeedMod = 5;
    private readonly int mouseYSpeedMod = 3;
    private float oldDistance;
    private bool reachedDist = true;
    private float x;
    private float y;
    public int zoomRate = 30;
    public float cameraSmoothSpeed = 0.1f; // Add smoothing

    // Callback: called by Unity once when this object first becomes active
    private void Start()
    {
        var angles = transform.eulerAngles;
        x = angles.x;
        y = angles.y;
        distance = PlayerPrefs.GetFloat("CameraDistance", distance);
        currentDistance = distance;
        desiredDistance = distance;
        correctedDistance = distance;
    }

    // Callback: called by Unity every frame after all Update calls (handles camera rotation, zoom, and collision)
    private void LateUpdate()
    {
        try
        {
            if (GamePanel.isMovingAPanel) return;
        }
        catch { }

        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseXSpeedMod;
            float mouseY = Input.GetAxis("Mouse Y") * mouseYSpeedMod;
            
            x += mouseX;
            y -= mouseY;
        }

        y = ClampAngle(y, -50, 80);

        var rotation = Quaternion.Euler(y, x, 0);
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        desiredDistance -= scrollInput * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
        desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);
        correctedDistance = desiredDistance;
        currentDistance = correctedDistance;

        var position = 
            cameraTarget.position -
                   (rotation * Vector3.forward * currentDistance + new Vector3(0, -cameraTargetHeight, 0));
        
        transform.rotation = rotation;
        // Smooth camera position to prevent jittering during movement
        transform.position = Vector3.Lerp(transform.position, position, cameraSmoothSpeed);

        if (isHitting)
        {
            desiredDistance -= 0.01f * (Time.deltaTime * cameraCollisionSpeed) * zoomRate * Mathf.Abs(desiredDistance);
            desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);
        }
        else
        {
            Debug.DrawLine(transform.position - transform.forward * 0.5f,
                transform.position - transform.forward * (lastDistance - desiredDistance));
            if (desiredDistance < lastDistance)
            {
                if (!Physics.Raycast(transform.position - transform.forward * 0.5f, -transform.forward,
                        lastDistance - desiredDistance) && !Physics.Raycast(
                        transform.position - transform.forward * (lastDistance - desiredDistance + 0.5f), Vector3.down,
                        0.5f))
                {
                    desiredDistance += 0.01f * (Time.deltaTime * cameraCollisionSpeed) * zoomRate *
                                       Mathf.Abs(desiredDistance);
                    desiredDistance = Mathf.Clamp(desiredDistance, minViewDistance, maxViewDistance);
                }
            }
            else
            {
                reachedDist = true;
                lastDistance = 0;
            }
        }
    }

    // set play camera preferences before quit
    private void OnApplicationQuit()
    {
        PlayerPrefs.SetFloat("CameraDistance", currentDistance);
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

    // Callback: called by Unity when the camera enters a wall/obstacle collider (triggers zoom-in)
    private void OnTriggerEnter(Collider col)
    {
        if (!col.isTrigger)
        {
            isHitting = true;
            if (reachedDist)
            {
                lastDistance = currentDistance;
                reachedDist = false;
            }
        }
    }

    // Callback: called by Unity every frame while the camera stays inside a collider
    private void OnTriggerStay(Collider col)
    {
        if (!col.isTrigger) isHitting = true;
    }

    // Callback: called by Unity when the camera exits a collider (resumes normal zoom behavior)
    private void OnTriggerExit(Collider col)
    {
        if (!col.isTrigger) isHitting = false;
    }
}