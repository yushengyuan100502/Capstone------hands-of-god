using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreeLaneObstacleSpawner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float[] laneYPositions = {0f, 3f, 6f}; // Must match ThreeLaneController
    public float spawnDistance = 20f;
    public float obstacleSpacing = 5f; // Minimum distance between obstacle groups
    
    [Header("Obstacle Prefabs")]
    public GameObject[] obstaclePrefabs; // Different obstacle types
    public GameObject enemyPrefab; // SlimeEnemy or other enemy prefab
    
    [Header("Spawn Rates")]
    public float obstacleChance = 0.7f; // Chance to spawn obstacles at each opportunity
    public float enemyChance = 0.3f; // Chance to spawn enemy instead of obstacle
    public float difficultyIncreaseRate = 0.01f; // How fast difficulty increases over time
    
    [Header("Safety Rules")]
    public bool alwaysKeepOneLaneSafe = true; // Ensures at least one lane is always passable
    public int maxSimultaneousObstacles = 2; // Max obstacles that can block lanes at once
    
    [Header("Obstacle Types")]
    public ObstacleType[] availableObstacles;
    
    // Private variables
    private float lastSpawnX = 0f;
    private float gameStartTime;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private int[] laneObstacleCounts; // Track obstacles per lane for safety
    
    [System.Serializable]
    public class ObstacleType
    {
        public string name;
        public GameObject prefab;
        public float height = 2f;
        public bool blocksMovement = true;
        public bool damagesPlayer = true;
        public int damage = 10;
        public bool canBeDestroyedByFireball = true;
        [Range(0f, 1f)]
        public float spawnWeight = 1f; // Higher = more likely to spawn
    }
    
    void Start()
    {
        gameStartTime = Time.time;
        laneObstacleCounts = new int[laneYPositions.Length];
        
        // Create default obstacles if none assigned
        CreateDefaultObstacles();
        
        Debug.Log("ThreeLaneObstacleSpawner initialized with " + laneYPositions.Length + " lanes");
    }
    
    void Update()
    {
        if (InfiniteScrollManager.Instance != null && InfiniteScrollManager.Instance.isScrolling)
        {
            SpawnObstaclesIfNeeded();
        }
        
        CleanupOldObstacles();
    }
    
    void CreateDefaultObstacles()
    {
        if (availableObstacles == null || availableObstacles.Length == 0)
        {
            // Create basic obstacle types
            availableObstacles = new ObstacleType[3];
            
            // Wall obstacle
            availableObstacles[0] = new ObstacleType
            {
                name = "Wall",
                prefab = CreateDefaultObstacle("Wall", Color.gray, new Vector3(1f, 2f, 1f)),
                height = 2f,
                blocksMovement = true,
                damagesPlayer = false,
                canBeDestroyedByFireball = true,
                spawnWeight = 0.5f
            };
            
            // Spike obstacle
            availableObstacles[1] = new ObstacleType
            {
                name = "Spikes",
                prefab = CreateDefaultObstacle("Spikes", Color.red, new Vector3(2f, 1f, 1f)),
                height = 1f,
                blocksMovement = false,
                damagesPlayer = true,
                damage = 15,
                canBeDestroyedByFireball = true,
                spawnWeight = 0.3f
            };
            
            // Barrier obstacle
            availableObstacles[2] = new ObstacleType
            {
                name = "Barrier",
                prefab = CreateDefaultObstacle("Barrier", Color.yellow, new Vector3(0.5f, 3f, 1f)),
                height = 3f,
                blocksMovement = true,
                damagesPlayer = true,
                damage = 20,
                canBeDestroyedByFireball = false,
                spawnWeight = 0.2f
            };
        }
    }
    
    GameObject CreateDefaultObstacle(string obstacleName, Color color, Vector3 scale)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = "Default" + obstacleName;
        obstacle.transform.localScale = scale;
        
        Renderer renderer = obstacle.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = color;
        }
        
        obstacle.SetActive(false);
        return obstacle;
    }
    
    void SpawnObstaclesIfNeeded()
    {
        // In infinite scroller, use virtual camera position based on world scroll distance
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
        
        if (lastSpawnX < virtualCameraX + spawnDistance)
        {
            if (Random.value < GetCurrentObstacleChance())
            {
                SpawnObstacleGroup(lastSpawnX);
                //Debug.Log($"Spawned obstacle group at X={lastSpawnX}");
            }
            lastSpawnX += obstacleSpacing + Random.Range(-1f, 2f); // Add some variation
        }
    }
    
    float GetCurrentObstacleChance()
    {
        // Increase difficulty over time
        float timePlayed = Time.time - gameStartTime;
        float currentChance = obstacleChance + (timePlayed * difficultyIncreaseRate);
        return Mathf.Clamp01(currentChance);
    }
    
    void SpawnObstacleGroup(float x)
    {
        // Decide which lanes to block
        List<int> availableLanes = new List<int>();
        for (int i = 0; i < laneYPositions.Length; i++)
        {
            availableLanes.Add(i);
        }
        
        // Determine how many lanes to block
        int lanesToBlock = Random.Range(1, Mathf.Min(maxSimultaneousObstacles + 1, laneYPositions.Length));
        
        // If safety rule is enabled, never block all lanes
        if (alwaysKeepOneLaneSafe && lanesToBlock >= laneYPositions.Length)
        {
            lanesToBlock = laneYPositions.Length - 1;
        }
        
        // Select lanes to block
        List<int> lanesToSpawnIn = new List<int>();
        for (int i = 0; i < lanesToBlock; i++)
        {
            int randomIndex = Random.Range(0, availableLanes.Count);
            lanesToSpawnIn.Add(availableLanes[randomIndex]);
            availableLanes.RemoveAt(randomIndex);
        }
        
        // Spawn obstacles in selected lanes
        foreach (int laneIndex in lanesToSpawnIn)
        {
            SpawnObstacleInLane(x, laneIndex);
        }
    }
    
    void SpawnObstacleInLane(float x, int laneIndex)
    {
        // Decide whether to spawn enemy or obstacle
        bool spawnEnemy = (enemyPrefab != null) && (Random.value < enemyChance);
        
        if (spawnEnemy)
        {
            SpawnEnemyInLane(x, laneIndex);
        }
        else
        {
            SpawnRandomObstacleInLane(x, laneIndex);
        }
    }
    
    void SpawnEnemyInLane(float x, int laneIndex)
    {
        Vector3 position = new Vector3(x, laneYPositions[laneIndex] + 1f, 0f);
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        
        // Add WorldMover if needed
        WorldMover mover = enemy.GetComponent<WorldMover>();
        if (mover == null)
        {
            mover = enemy.AddComponent<WorldMover>();
        }
        mover.speedMultiplier = 1f;
        mover.destroyWhenOffScreen = true;
        
        activeObstacles.Add(enemy);
        laneObstacleCounts[laneIndex]++;
        
        //Debug.Log("Spawned enemy in lane " + laneIndex + " at x=" + x);
    }
    
    void SpawnRandomObstacleInLane(float x, int laneIndex)
    {
        if (availableObstacles == null || availableObstacles.Length == 0) return;
        
        // Select obstacle type based on weights
        ObstacleType selectedType = SelectWeightedObstacle();
        if (selectedType == null || selectedType.prefab == null) return;
        
        Vector3 position = new Vector3(x, laneYPositions[laneIndex] + selectedType.height * 0.5f, 0f);
        GameObject obstacle = Instantiate(selectedType.prefab, position, Quaternion.identity);
        obstacle.SetActive(true);
        obstacle.name = selectedType.name + "_Lane" + laneIndex;
        
        // Add obstacle component
        LaneObstacle obstacleComponent = obstacle.GetComponent<LaneObstacle>();
        if (obstacleComponent == null)
        {
            obstacleComponent = obstacle.AddComponent<LaneObstacle>();
        }
        
        // Configure obstacle
        obstacleComponent.Initialize(selectedType, laneIndex);
        
        // Add WorldMover
        WorldMover mover = obstacle.GetComponent<WorldMover>();
        if (mover == null)
        {
            mover = obstacle.AddComponent<WorldMover>();
        }
        mover.speedMultiplier = 1f;
        mover.destroyWhenOffScreen = true;
        
        activeObstacles.Add(obstacle);
        laneObstacleCounts[laneIndex]++;
        
        // Debug.Log("Spawned " + selectedType.name + " in lane " + laneIndex + " at x=" + x);
    }
    
    ObstacleType SelectWeightedObstacle()
    {
        float totalWeight = 0f;
        foreach (var obstacle in availableObstacles)
        {
            if (obstacle.prefab != null)
                totalWeight += obstacle.spawnWeight;
        }
        
        if (totalWeight <= 0f) return null;
        
        float randomValue = Random.value * totalWeight;
        float currentWeight = 0f;
        
        foreach (var obstacle in availableObstacles)
        {
            if (obstacle.prefab != null)
            {
                currentWeight += obstacle.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return obstacle;
                }
            }
        }
        
        return availableObstacles[0]; // Fallback
    }
    
    void CleanupOldObstacles()
    {
        // Clean up destroyed obstacles and update lane counts
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
                // Note: We can't easily determine which lane this was in,
                // so we'll reset counts periodically or when they get too high
            }
        }
        
        // Reset lane counts if they seem too high (safety measure)
        int totalTracked = 0;
        for (int i = 0; i < laneObstacleCounts.Length; i++)
        {
            totalTracked += laneObstacleCounts[i];
        }
        
        if (totalTracked > activeObstacles.Count * 2)
        {
            // Reset counts
            for (int i = 0; i < laneObstacleCounts.Length; i++)
            {
                laneObstacleCounts[i] = 0;
            }
        }
    }
    
    /// <summary>
    /// Manually spawn an obstacle in a specific lane (for testing)
    /// </summary>
    public void ForceSpawnObstacle(int laneIndex, float x)
    {
        if (laneIndex >= 0 && laneIndex < laneYPositions.Length)
        {
            SpawnObstacleInLane(x, laneIndex);
        }
    }
    
    /// <summary>
    /// Get current difficulty level
    /// </summary>
    public float GetCurrentDifficulty()
    {
        return GetCurrentObstacleChance();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn distance
        Gizmos.color = Color.red;
        float minY = laneYPositions.Length > 0 ? laneYPositions[0] - 1f : -1f;
        float maxY = laneYPositions.Length > 0 ? laneYPositions[laneYPositions.Length - 1] + 1f : 7f;
        Gizmos.DrawLine(new Vector3(spawnDistance, minY, 0f), new Vector3(spawnDistance, maxY, 0f));
        
        // Draw obstacle spacing
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f); // Orange color
        for (float x = 0f; x < spawnDistance; x += obstacleSpacing)
        {
            for (int i = 0; i < laneYPositions.Length; i++)
            {
                Vector3 pos = new Vector3(x, laneYPositions[i], 0f);
                Gizmos.DrawWireCube(pos, new Vector3(1f, 1f, 1f));
            }
        }
    }
}

// Obstacle component to handle behavior
public class LaneObstacle : MonoBehaviour
{
    private ThreeLaneObstacleSpawner.ObstacleType obstacleType;
    private int laneIndex;
    private bool hasBeenDestroyed = false;
    
    public void Initialize(ThreeLaneObstacleSpawner.ObstacleType type, int lane)
    {
        obstacleType = type;
        laneIndex = lane;
        
        // Set up tags
        gameObject.tag = "Obstacle";
        
        // Add appropriate collider if missing
        if (GetComponent<Collider>() == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            if (!obstacleType.blocksMovement)
            {
                collider.isTrigger = true; // Spikes should be triggers
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.collider);
    }
    
    void HandleCollision(Collider other)
    {
        if (hasBeenDestroyed) return;
        
        // Handle player collision
        if (other.CompareTag("Player"))
        {
            if (obstacleType.damagesPlayer)
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(obstacleType.damage);
                    Debug.Log(obstacleType.name + " damaged player for " + obstacleType.damage);
                }
            }
        }
        
        // Handle fireball collision (placeholder for future implementation)
        if (other.CompareTag("Fireball") && obstacleType.canBeDestroyedByFireball)
        {
            DestroyObstacle();
        }
    }
    
    public void DestroyObstacle()
    {
        if (hasBeenDestroyed) return;
        
        hasBeenDestroyed = true;
        
        // Add destruction effects here (particles, sound, etc.)
        Debug.Log(obstacleType.name + " destroyed!");
        
        Destroy(gameObject);
    }
    
    public ThreeLaneObstacleSpawner.ObstacleType GetObstacleType()
    {
        return obstacleType;
    }
    
    public int GetLaneIndex()
    {
        return laneIndex;
    }
}