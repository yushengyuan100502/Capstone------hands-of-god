using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speedMultiplier = 1f; // Allows different objects to move at different speeds
    public bool destroyWhenOffScreen = true;
    public float destroyXPosition = -15f; // Destroy when this far to the left
    
    [Header("Optional Settings")]
    public bool moveOnlyWhenScrolling = true;
    
    private float originalX;
    
    void Start()
    {
        originalX = transform.position.x;
    }
    
    void Update()
    {
        // Only move if scrolling (unless specified otherwise)
        if (moveOnlyWhenScrolling && InfiniteScrollManager.Instance != null)
        {
            if (!InfiniteScrollManager.Instance.isScrolling)
                return;
        }
        
        // Move the object leftward
        float moveSpeed = GetMoveSpeed();
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        
        // Destroy if moved too far left
        if (destroyWhenOffScreen && transform.position.x < destroyXPosition)
        {
            Destroy(gameObject);
        }
    }
    
    float GetMoveSpeed()
    {
        if (InfiniteScrollManager.Instance != null)
        {
            return InfiniteScrollManager.Instance.GetCurrentScrollSpeed() * speedMultiplier;
        }
        return 5f * speedMultiplier; // Default speed if no manager found
    }
    
    // Method to reset position (useful for background loops)
    public void ResetPosition()
    {
        transform.position = new Vector3(originalX, transform.position.y, transform.position.z);
    }
}