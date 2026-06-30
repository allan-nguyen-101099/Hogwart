using UnityEngine;

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

        // Callback: called by Unity once when this object first becomes active
        private void Start()
        {
            // get the transform of the main camera
            if (Camera.main != null)
                m_Cam = Camera.main.transform;
            else
                Debug.Log(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
            // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            // get the third person character ( this should never be null due to require component )
            m_Character = GetComponent<ThirdPersonCharacter>();
            if (m_Cam != null)
                m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
            else
                m_CamForward = Vector3.Scale(transform.forward, new Vector3(1, 0, 1)).normalized;
        }


        // Callback: called by Unity every frame (polls jump key input)
        private void Update()
        {
            if (Chat.Instance != null && Chat.Instance.isWritting) return;

            // Keep jump input in a single input system for easier monitoring.
            if (!m_Jump)
                m_Jump = InputSystemAgent.GetKeyDown("Space");
        }


        // Callback: called by Unity every fixed physics timestep (reads WASD movement and passes to character motor)
        private void FixedUpdate()
        {
            if (Chat.Instance != null && Chat.Instance.isWritting)
            {
                return;
            }
            
            // Use InputSystemAgent for WASD/left-stick movement.
            Vector2 moveInput = InputSystemAgent.NormalMove;
            float h = moveInput.x;  // A/D
            float v = moveInput.y;  // W/S
            
            bool crouch = InputSystemAgent.GetKey("LCtrl");
            
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
            if (InputSystemAgent.GetKey("LShift")) 
                m_Move *= 0.5f;

            // pass all parameters to the character control script
            // Also pass camera direction for CS:GO/Valorant style movement
            m_Character.Move(m_Move, crouch, m_Jump, m_Cam != null ? m_Cam.eulerAngles.y : 0);
            m_Jump = false;
        }
    }
}