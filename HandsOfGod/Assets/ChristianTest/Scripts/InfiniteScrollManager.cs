using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollManager : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 5f;
    public float scrollAcceleration = 0.1f;
    public float maxScrollSpeed = 15f;
    
    [Header("Game State")]
    public bool isScrolling = true;
    
    [Header("References")]
    public Transform player;
    
    // Static reference so other scripts can access
    public static InfiniteScrollManager Instance;
    
    // Events for other scripts to listen to
    public static System.Action OnGameStart;
    public static System.Action OnGameOver;
    
    private float currentScrollSpeed;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        currentScrollSpeed = scrollSpeed;
        StartGame();
    }
    
    void Update()
    {
        if (isScrolling)
        {
            // Gradually increase scroll speed over time
            if (currentScrollSpeed < maxScrollSpeed)
            {
                currentScrollSpeed += scrollAcceleration * Time.deltaTime;
                currentScrollSpeed = Mathf.Min(currentScrollSpeed, maxScrollSpeed);
            }
        }
        
        // Check for game over conditions (player fell too far, etc.)
        CheckGameOverConditions();
    }
    
    public float GetCurrentScrollSpeed()
    {
        return isScrolling ? currentScrollSpeed : 0f;
    }
    
    public void StartGame()
    {
        isScrolling = true;
        OnGameStart?.Invoke();
        Debug.Log("Infinite Scroll Game Started!");
    }
    
    public void StopGame()
    {
        isScrolling = false;
        OnGameOver?.Invoke();
        Debug.Log("Game Over!");
    }
    
    public void PauseGame()
    {
        isScrolling = false;
    }
    
    public void ResumeGame()
    {
        isScrolling = true;
    }
    
    void CheckGameOverConditions()
    {
        if (player != null)
        {
            // Game over if player falls too far below
            if (player.position.y < -10f)
            {
                StopGame();
            }
        }
    }
    
    void OnGUI()
    {
        // Simple debug info
        GUI.Label(new Rect(10, 10, 200, 20), "Speed: " + currentScrollSpeed.ToString("F1"));
        GUI.Label(new Rect(10, 30, 200, 20), "Scrolling: " + isScrolling);
        
        if (!isScrolling && GUI.Button(new Rect(10, 60, 100, 30), "Restart"))
        {
            // Simple restart by reloading scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}