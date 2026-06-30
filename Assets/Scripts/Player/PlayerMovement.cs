using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float groundDrag = 5f;
    public float groundDragThreshold = 0.1f;
    public Camera mainCamera;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private float groundCheckDistance = 0.1f;
    private LayerMask groundLayer;

    // Callback: called by Unity once when this object first becomes active
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Player needs a Rigidbody component!");
        }

        // Find the main camera if not assigned
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Use default layer for ground check
        groundLayer = LayerMask.GetMask("Default");
    }

    // Callback: called by Unity every frame
    private void Update()
    {
        // Get WASD input
        float moveX = Input.GetAxis("Horizontal");   // A/D
        float moveZ = Input.GetAxis("Vertical");     // W/S
        
        // Get camera direction
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Remove Y component so movement is always on the ground plane
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Create movement direction relative to camera
        moveDirection = (cameraForward * moveZ + cameraRight * moveX).normalized;

        // Check if player is on ground - raycast down from player position
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDistance + 0.5f);
        
        Debug.DrawLine(transform.position, transform.position + Vector3.down * (groundCheckDistance + 0.5f), 
            isGrounded ? Color.green : Color.red); // Visualize raycast

        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        // Apply ground drag
        if (isGrounded && rb.velocity.y < 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, -groundDragThreshold, rb.velocity.z);
        }
    }

    // Callback: called by Unity every fixed physics timestep
    private void FixedUpdate()
    {
        if (rb == null) return;
        
        // Move the player horizontally
        if (moveDirection.magnitude > 0)
        {
            Vector3 newPosition = rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    private void Jump()
    {
        // Apply jump force
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // Reset Y velocity
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}