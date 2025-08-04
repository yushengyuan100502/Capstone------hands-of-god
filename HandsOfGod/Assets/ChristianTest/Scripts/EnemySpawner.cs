using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab; // Assign your SlimeEnemy prefab here
    public float spawnDistance = 20f; // How far ahead to spawn enemies
    
    [Header("Spawn Timing")]
    public float minSpawnInterval = 3f;
    public float maxSpawnInterval = 8f;
    public float spawnIntervalDecrease = 0.01f; // Spawns get more frequent over time
    
    [Header("Spawn Positions")]
    public float groundY = 1f; // Y position to spawn enemies
    public float spawnHeightVariation = 3f; // Can spawn enemies on platforms above ground
    
    [Header("Enemy Behavior")]
    public bool addWorldMoverToEnemies = true;
    
    private float nextSpawnTime;
    private float currentMinInterval;
    private float currentMaxInterval;
    
    void Start()
    {
        currentMinInterval = minSpawnInterval;
        currentMaxInterval = maxSpawnInterval;
        ScheduleNextSpawn();
    }
    
    void Update()
    {
        if (InfiniteScrollManager.Instance != null && !InfiniteScrollManager.Instance.isScrolling)
            return;
            
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            ScheduleNextSpawn();
            
            // Gradually make spawns more frequent
            currentMinInterval = Mathf.Max(1f, currentMinInterval - spawnIntervalDecrease);
            currentMaxInterval = Mathf.Max(2f, currentMaxInterval - spawnIntervalDecrease);
        }
    }
    
    void ScheduleNextSpawn()
    {
        float interval = Random.Range(currentMinInterval, currentMaxInterval);
        nextSpawnTime = Time.time + interval;
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: No enemy prefab assigned!");
            return;
        }
        
        // Choose spawn position
        Vector3 spawnPosition = GetSpawnPosition();
        
        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        
        // Add WorldMover component if requested and not already present
        if (addWorldMoverToEnemies && enemy.GetComponent<WorldMover>() == null)
        {
            WorldMover mover = enemy.AddComponent<WorldMover>();
            mover.speedMultiplier = 1f; // Enemies move at same speed as world
            mover.destroyWhenOffScreen = true;
        }
        
        Debug.Log("Enemy spawned at: " + spawnPosition);
    }
    
    Vector3 GetSpawnPosition()
    {
        // Spawn at the spawn distance
        float x = spawnDistance;
        
        // Choose Y position - either ground level or on a platform above
        float y = groundY;
        
        // Randomly spawn at different heights
        if (Random.value > 0.7f) // 30% chance to spawn higher
        {
            y = groundY + Random.Range(1f, spawnHeightVariation);
        }
        
        return new Vector3(x, y, 0f);
    }
    
    // Method to manually spawn an enemy (can be called by other scripts)
    public void ForceSpawnEnemy()
    {
        SpawnEnemy();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn distance line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(spawnDistance, groundY - 2f, 0), new Vector3(spawnDistance, groundY + spawnHeightVariation + 1f, 0));
        
        // Draw ground level
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(spawnDistance - 2f, groundY, 0), new Vector3(spawnDistance + 2f, groundY, 0));
        
        // Draw spawn height variation
        Gizmos.color = Color.yellow;
        for (float h = groundY; h <= groundY + spawnHeightVariation; h += 1f)
        {
            Gizmos.DrawWireCube(new Vector3(spawnDistance, h, 0), new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}