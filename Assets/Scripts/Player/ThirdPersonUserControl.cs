using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Characters.ThirdPerson
{
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class ThirdPersonUserControl : MonoBehaviour
    {
        private Transform m_Cam; // A reference to the main camera in the scenes transform
        private Vector3 m_CamForward; // The current forward direction of the camera
        private ThirdPersonCharacter m_Character; // A reference to the ThirdPersonCharacter on the object

        private bool
            m_Jump; // the world-relative desired move direction, calculated from the camForward and user input.

        private Vector3 m_Move;

        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
                m_Cam = Camera.main.transform;
            else
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
            if (m_Cam != null)
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            else
                m_CamForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
        }


        private void Update()
        {
            if (Chat.Instance.isWritting) return;

            // Try InputSystemAgent first, fall back to old Input system
            if (!m_Jump)
                m_Jump = InputSystemAgent.GetKeyDown("Space") || Input.GetKeyDown(KeyCode.Space);
        }


        // Fixed update is called in sync with physics
        private void FixedUpdate()
        {
            if (Chat.Instance.isWritting)
            {
                return;
            }
            
            // Use old Input system for reliable WASD input
            float h = Input.GetAxis("Horizontal");  // A/D
            float v = Input.GetAxis("Vertical");    // W/S
            
            var crouch = InputSystemAgent.GetKey("LCtrl") || Input.GetKey(KeyCode.LeftControl);
            
            // calculate move direction to pass to character
            if (m_Cam != null)
            {
                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;

                m_Move = v * m_CamForward + h * m_Cam.right;
            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }
            // walk speed multiplier
            if (InputSystemAgent.GetKey("LShift") || Input.GetKey(KeyCode.LeftShift)) 
                m_Move *= 0.5f;

            // pass all parameters to the character control script
            // Also pass camera direction for CS:GO/Valorant style movement
            m_Character.Move(m_Move, crouch, m_Jump, m_Cam != null ? m_Cam.eulerAngles.y : 0);
            m_Jump = false;
        }
    }
}