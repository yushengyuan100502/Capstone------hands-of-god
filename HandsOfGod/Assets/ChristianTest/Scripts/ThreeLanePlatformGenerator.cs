using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeLanePlatformGenerator : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab; // Assign platform prefab or leave null for default cubes
    public float[] laneYPositions = {0f, 3f, 6f}; // Must match ThreeLaneController
    public float platformWidth = 3f;
    public float platformHeight = 0.5f;
    public float platformDepth = 1f;
    
    [Header("Generation Settings")]
    public float spawnDistance = 25f; // How far ahead to generate platforms
    public float platformSpacing = 3f; // Distance between platform segments
    public int initialPlatformCount = 10; // Platforms to spawn at start
    
    [Header("Visual Customization")]
    public bool enableLaneColoring = true; // Toggle to enable/disable lane-specific coloring
    public Material[] laneMaterials = new Material[3]; // Different materials for each lane
    public Color[] laneColors = {Color.blue, Color.blue, Color.blue}; // Fallback colors if no materials
    public bool useGradientColors = false;
    public Gradient laneGradient;
    
    [Header("Platform Variation")]
    public bool addRandomHeightVariation = false;
    public float heightVariationAmount = 0.2f;
    
    // Private variables
    private float lastSpawnX = 0f;
    private List<GameObject>[] lanePlatforms; // Separate lists for each lane
    private GameObject[] defaultPlatformPrefabs; // Generated default prefabs for each lane
    
    void Start()
    {
        // Initialize lane platform lists
        lanePlatforms = new List<GameObject>[laneYPositions.Length];
        for (int i = 0; i < lanePlatforms.Length; i++)
        {
            lanePlatforms[i] = new List<GameObject>();
        }
        
        // Create default platform prefabs if none assigned
        CreateDefaultPlatforms();
        
        // Generate initial platforms
        GenerateInitialPlatforms();
        
        Debug.Log("ThreeLanePlatformGenerator initialized with " + laneYPositions.Length + " lanes");
    }
    
    void Update()
    {
        // Continue generating platforms as the world scrolls
        if (InfiniteScrollManager.Instance != null && InfiniteScrollManager.Instance.isScrolling)
        {
            GeneratePlatformsIfNeeded();
        }
        else
        {
            // Debug why generation isn't happening
            if (InfiniteScrollManager.Instance == null)
                Debug.LogWarning("InfiniteScrollManager.Instance is null!");
            else if (!InfiniteScrollManager.Instance.isScrolling)
                Debug.LogWarning("InfiniteScrollManager.isScrolling is false!");
        }
        
        // Clean up old platforms
        CleanupOldPlatforms();
    }
    
    void CreateDefaultPlatforms()
    {
        if (platformPrefab == null)
        {
            defaultPlatformPrefabs = new GameObject[laneYPositions.Length];
            
            for (int laneIndex = 0; laneIndex < laneYPositions.Length; laneIndex++)
            {
                // Create a cube for this lane
                GameObject lanePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lanePrefab.name = "Lane" + laneIndex + "Platform";
                
                // Set size
                lanePrefab.transform.localScale = new Vector3(platformWidth, platformHeight, platformDepth);
                
                // Apply lane-specific visual
                Renderer renderer = lanePrefab.GetComponent<Renderer>();
                if (renderer != null)
                {
                    ApplyLaneVisual(renderer, laneIndex);
                }
                
                // Deactivate the prefab
                lanePrefab.SetActive(false);
                defaultPlatformPrefabs[laneIndex] = lanePrefab;
            }
        }
    }
    
    void ApplyLaneVisual(Renderer renderer, int laneIndex)
    {
        // Skip lane-specific coloring if disabled
        if (!enableLaneColoring)
        {
            return;
        }
        
        // Apply materials first if available
        if (laneMaterials != null && laneIndex < laneMaterials.Length && laneMaterials[laneIndex] != null)
        {
            renderer.material = laneMaterials[laneIndex];
        }
        // Otherwise apply colors
        else if (useGradientColors && laneGradient != null)
        {
            float t = (float)laneIndex / (laneYPositions.Length - 1);
            renderer.material.color = laneGradient.Evaluate(t);
        }
        else if (laneColors != null && laneIndex < laneColors.Length)
        {
            renderer.material.color = laneColors[laneIndex];
        }
        else
        {
            // Default colors
            Color[] defaultColors = {Color.red, Color.green, Color.blue, Color.yellow, Color.cyan};
            renderer.material.color = defaultColors[laneIndex % defaultColors.Length];
        }
    }
    
    void GenerateInitialPlatforms()
    {
        // Generate platforms from current position to spawn distance
        for (float x = -10f; x < spawnDistance; x += platformSpacing)
        {
            GeneratePlatformSegment(x);
        }
        lastSpawnX = spawnDistance;
    }
    
    void GeneratePlatformsIfNeeded()
    {
        // In infinite scroller, generate based on world scroll distance, not camera position
        // Calculate how far the world has scrolled based on time and scroll speed
        float worldScrollDistance = 0f;
        
        if (InfiniteScrollManager.Instance != null)
        {
            // Use accumulated distance based on scroll speed over time
            worldScrollDistance = Time.time * InfiniteScrollManager.Instance.GetCurrentScrollSpeed();
        }
        else
        {
            // Fallback: use time-based generation
            worldScrollDistance = Time.time * 5f; // Assume 5 units/second
        }
        
        // The "virtual camera position" moves right as world scrolls left
        float virtualCameraX = worldScrollDistance;
        
        //Debug.Log($"Platform Gen Check - VirtualCameraX: {virtualCameraX:F1}, LastSpawnX: {lastSpawnX:F1}, SpawnDistance: {spawnDistance}, Need to spawn: {lastSpawnX < virtualCameraX + spawnDistance}");
        
        if (lastSpawnX < virtualCameraX + spawnDistance)
        {
            //Debug.Log($"Generating platform segment at X={lastSpawnX}");
            GeneratePlatformSegment(lastSpawnX);
            lastSpawnX += platformSpacing;
        }
    }
    
    void GeneratePlatformSegment(float x)
    {
        // Generate one platform for each lane at this X position
        for (int laneIndex = 0; laneIndex < laneYPositions.Length; laneIndex++)
        {
            CreatePlatformForLane(x, laneIndex);
        }
    }
    
    void CreatePlatformForLane(float x, int laneIndex)
    {
        // Determine Y position with optional variation
        float y = laneYPositions[laneIndex];
        if (addRandomHeightVariation)
        {
            y += Random.Range(-heightVariationAmount, heightVariationAmount);
        }
        
        Vector3 position = new Vector3(x, y, 0f);
        
        // Choose prefab
        GameObject prefabToUse = platformPrefab;
        if (prefabToUse == null && defaultPlatformPrefabs != null)
        {
            prefabToUse = defaultPlatformPrefabs[laneIndex];
        }
        
        if (prefabToUse == null)
        {
            Debug.LogError("No platform prefab available for lane " + laneIndex);
            return;
        }
        
        // Instantiate platform
        GameObject platform = Instantiate(prefabToUse, position, Quaternion.identity);
        platform.SetActive(true);
        platform.name = "Lane" + laneIndex + "Platform_" + x.ToString("F1");
        
        //Debug.Log($"Created platform: {platform.name} at position {position}");
        
        // Apply lane visual if using single prefab
        if (platformPrefab != null)
        {
            Renderer renderer = platform.GetComponent<Renderer>();
            if (renderer != null)
            {
                ApplyLaneVisual(renderer, laneIndex);
            }
        }
        
        // Add WorldMover component
        WorldMover mover = platform.GetComponent<WorldMover>();
        if (mover == null)
        {
            mover = platform.AddComponent<WorldMover>();
        }
        mover.speedMultiplier = 1f;
        mover.destroyWhenOffScreen = true;
        
        // Add to tracking list
        lanePlatforms[laneIndex].Add(platform);
        
        // Ensure proper collider
        if (platform.GetComponent<Collider>() == null)
        {
            BoxCollider collider = platform.AddComponent<BoxCollider>();
            collider.size = new Vector3(platformWidth, platformHeight, platformDepth);
        }
        
        // Add platform tag
        platform.tag = "Platform";
    }
    
    void CleanupOldPlatforms()
    {
        // Remove destroyed platforms from tracking lists
        for (int laneIndex = 0; laneIndex < lanePlatforms.Length; laneIndex++)
        {
            for (int i = lanePlatforms[laneIndex].Count - 1; i >= 0; i--)
            {
                if (lanePlatforms[laneIndex][i] == null)
                {
                    lanePlatforms[laneIndex].RemoveAt(i);
                }
            }
        }
    }
    
    /// <summary>
    /// Get all platforms in a specific lane
    /// </summary>
    public List<GameObject> GetLanePlatforms(int laneIndex)
    {
        if (laneIndex >= 0 && laneIndex < lanePlatforms.Length)
        {
            return new List<GameObject>(lanePlatforms[laneIndex]); // Return copy
        }
        return new List<GameObject>();
    }
    
    /// <summary>
    /// Get the nearest platform in a lane to a given X position
    /// </summary>
    public GameObject GetNearestPlatformInLane(int laneIndex, float xPosition)
    {
        if (laneIndex < 0 || laneIndex >= lanePlatforms.Length) return null;
        
        GameObject nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (GameObject platform in lanePlatforms[laneIndex])
        {
            if (platform != null)
            {
                float distance = Mathf.Abs(platform.transform.position.x - xPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = platform;
                }
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// Force generate platforms at a specific X position (useful for testing)
    /// </summary>
    public void ForceGenerateAt(float x)
    {
        GeneratePlatformSegment(x);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw lane positions
        Gizmos.color = Color.yellow;
        for (int i = 0; i < laneYPositions.Length; i++)
        {
            Vector3 start = new Vector3(-20f, laneYPositions[i], 0f);
            Vector3 end = new Vector3(spawnDistance + 10f, laneYPositions[i], 0f);
            Gizmos.DrawLine(start, end);
            
            // Draw lane number
            Gizmos.DrawWireCube(new Vector3(-15f, laneYPositions[i], 0f), new Vector3(1f, 0.5f, 1f));
        }
        
        // Draw spawn distance
        Gizmos.color = Color.green;
        float minY = laneYPositions.Length > 0 ? laneYPositions[0] - 1f : -1f;
        float maxY = laneYPositions.Length > 0 ? laneYPositions[laneYPositions.Length - 1] + 1f : 7f;
        Gizmos.DrawLine(new Vector3(spawnDistance, minY, 0f), new Vector3(spawnDistance, maxY, 0f));
        
        // Draw platform spacing
        Gizmos.color = Color.cyan;
        for (float x = 0f; x < spawnDistance; x += platformSpacing)
        {
            Gizmos.DrawWireCube(new Vector3(x, laneYPositions[1], 0f), new Vector3(0.5f, 0.2f, 0.5f));
        }
    }
}