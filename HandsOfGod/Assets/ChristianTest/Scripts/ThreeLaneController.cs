using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeLaneController : MonoBehaviour
{
    [Header("Lane Settings")]
    public float[] laneYPositions = {0f, 3f, 6f}; // Bottom, Middle, Top
    public float fixedXPosition = -5f; // Player stays at this X position
    public int startingLane = 1; // Start in middle lane (0=bottom, 1=middle, 2=top)
    
    [Header("Visual Settings")]
    public bool flipSpriteForDirection = true;
    public Sprite normalSprite;
    public Sprite flashSprite;
    
    [Header("Components")]
    public Animator animator;
    
    // Private variables
    private int currentLane;
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private bool canMove = true;
    private bool isFlashing = false;
    private float flashDuration = 0.2f;
    private float flashTimer = 0f;
    
    // Public properties for other scripts
    public int CurrentLane => currentLane;
    public Vector3 CurrentLanePosition => new Vector3(fixedXPosition, laneYPositions[currentLane], 0f);
    public bool IsMoving => false; // Always false since we're locked to X position
    
    // Events for other scripts to listen to
    public System.Action<int> OnLaneChanged;
    public System.Action OnMoveBlocked;
    
    void Start()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null)
            animator = GetComponent<Animator>();
        
        // Set starting position
        currentLane = Mathf.Clamp(startingLane, 0, laneYPositions.Length - 1);
        MoveToCurrentLane();
        
        // Lock X and Z movement
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }
        
        Debug.Log("ThreeLaneController initialized. Starting lane: " + currentLane);
    }
    
    void Update()
    {
        HandleInput();
        UpdateVisuals();
        UpdateFlash();
    }
    
    void HandleInput()
    {
        if (!canMove) return;
        
        // Move up (arrow key or W)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            MoveUp();
        }
        
        // Move down (arrow key or S)
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            MoveDown();
        }
    }
    
    /// <summary>
    /// Move player to the lane above (public function)
    /// </summary>
    public void MoveUp()
    {
        if (!canMove) return;
        
        int targetLane = currentLane + 1;
        if (targetLane < laneYPositions.Length)
        {
            currentLane = targetLane;
            MoveToCurrentLane();
            OnLaneChanged?.Invoke(currentLane);
            TriggerFlash();
            Debug.Log("Moved to lane: " + currentLane);
        }
        else
        {
            OnMoveBlocked?.Invoke();
            Debug.Log("Cannot move up - already at top lane");
        }
    }
    
    /// <summary>
    /// Move player to the lane below (public function)
    /// </summary>
    public void MoveDown()
    {
        if (!canMove) return;
        
        int targetLane = currentLane - 1;
        if (targetLane >= 0)
        {
            currentLane = targetLane;
            MoveToCurrentLane();
            OnLaneChanged?.Invoke(currentLane);
            TriggerFlash();
            Debug.Log("Moved to lane: " + currentLane);
        }
        else
        {
            OnMoveBlocked?.Invoke();
            Debug.Log("Cannot move down - already at bottom lane");
        }
    }
    
    /// <summary>
    /// Move directly to a specific lane (public function)
    /// </summary>
    public void MoveToLane(int laneIndex)
    {
        if (!canMove) return;
        
        if (laneIndex >= 0 && laneIndex < laneYPositions.Length)
        {
            currentLane = laneIndex;
            MoveToCurrentLane();
            OnLaneChanged?.Invoke(currentLane);
            TriggerFlash();
            Debug.Log("Moved directly to lane: " + currentLane);
        }
        else
        {
            Debug.LogWarning("Invalid lane index: " + laneIndex);
        }
    }
    
    void MoveToCurrentLane()
    {
        // Instantly snap to the lane position
        Vector3 targetPosition = new Vector3(fixedXPosition, laneYPositions[currentLane], transform.position.z);
        transform.position = targetPosition;
        
        // Reset any vertical velocity
        if (rb != null)
        {
            rb.velocity = new Vector3(0, 0, 0);
        }
    }
    
    void TriggerFlash()
    {
        if (flashSprite != null)
        {
            isFlashing = true;
            flashTimer = flashDuration;
            spriteRenderer.sprite = flashSprite;
            transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            
            // Disable animator during flash
            if (animator != null)
                animator.enabled = false;
        }
    }
    
    void UpdateFlash()
    {
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            
            if (flashTimer <= 0f)
            {
                // End flash
                isFlashing = false;
                if (normalSprite != null)
                    spriteRenderer.sprite = normalSprite;
                transform.localScale = new Vector3(0.62f, 0.62f, 1f);
                
                // Re-enable animator
                if (animator != null)
                    animator.enabled = true;
            }
        }
    }
    
    void UpdateVisuals()
    {
        if (animator != null && !isFlashing)
        {
            // Update animator parameters
            //animator.SetBool("moving", false); // Never moving horizontally
            //animator.SetBool("grounded", true); // Always grounded in lane system
            //animator.SetFloat("lane", currentLane); // New parameter for lane-specific animations
        }
    }
    
    /// <summary>
    /// Temporarily disable movement (useful for skills, damage, etc.)
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
    
    /// <summary>
    /// Get the world position of a specific lane
    /// </summary>
    public Vector3 GetLanePosition(int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < laneYPositions.Length)
        {
            return new Vector3(fixedXPosition, laneYPositions[laneIndex], 0f);
        }
        return transform.position;
    }
    
    /// <summary>
    /// Check if player is in a specific lane
    /// </summary>
    public bool IsInLane(int laneIndex)
    {
        return currentLane == laneIndex;
    }
    
    /// <summary>
    /// Get lane name for UI/debugging
    /// </summary>
    public string GetCurrentLaneName()
    {
        switch (currentLane)
        {
            case 0: return "Bottom";
            case 1: return "Middle";
            case 2: return "Top";
            default: return "Unknown";
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw all lane positions
        Gizmos.color = Color.cyan;
        for (int i = 0; i < laneYPositions.Length; i++)
        {
            Vector3 lanePos = new Vector3(fixedXPosition, laneYPositions[i], 0f);
            Gizmos.DrawWireCube(lanePos, new Vector3(1f, 0.5f, 1f));
            
            // Highlight current lane
            if (Application.isPlaying && i == currentLane)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(lanePos, new Vector3(1.2f, 0.7f, 1.2f));
                Gizmos.color = Color.cyan;
            }
        }
        
        // Draw fixed X position line
        Gizmos.color = Color.red;
        float minY = laneYPositions.Length > 0 ? laneYPositions[0] - 1f : -1f;
        float maxY = laneYPositions.Length > 0 ? laneYPositions[laneYPositions.Length - 1] + 1f : 7f;
        Gizmos.DrawLine(new Vector3(fixedXPosition, minY, 0f), new Vector3(fixedXPosition, maxY, 0f));
    }
    
    void OnGUI()
    {
        // Debug info
        if (Application.isPlaying)
        {
            GUI.Label(new Rect(10, 100, 200, 20), "Current Lane: " + GetCurrentLaneName() + " (" + currentLane + ")");
            GUI.Label(new Rect(10, 120, 200, 20), "Position: " + transform.position.ToString("F1"));
            GUI.Label(new Rect(10, 140, 200, 20), "Can Move: " + canMove);
        }
    }
}