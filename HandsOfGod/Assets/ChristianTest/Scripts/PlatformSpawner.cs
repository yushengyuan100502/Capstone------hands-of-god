using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Platform Settings")]
    public GameObject platformPrefab; // Assign a cube or your custom platform prefab
    public float platformWidth = 5f;
    public float platformHeight = 1f;
    public float spawnDistance = 20f; // How far ahead to spawn
    
    [Header("Ground Level")]
    public float groundY = 0f;
    public float groundVariation = 2f; // How much the ground can vary up/down
    
    [Header("Spawning")]
    public float spawnInterval = 0.5f; // Time between spawns
    public int platformsPerSpawn = 3; // How many platforms to spawn in a row
    
    [Header("Gap Settings")]
    public float gapChance = 0.3f; // Chance of creating a gap (0-1)
    public float minGapSize = 2f;
    public float maxGapSize = 6f;
    
    private float nextSpawnTime;
    private float lastSpawnX;
    private float currentGroundY;
    
    void Start()
    {
        // If no prefab assigned, create a simple cube
        if (platformPrefab == null)
        {
            platformPrefab = CreateDefaultPlatform();
        }
        
        currentGroundY = groundY;
        lastSpawnX = spawnDistance;
        
        // Spawn initial platforms
        SpawnInitialPlatforms();
    }
    
    void Update()
    {
        if (InfiniteScrollManager.Instance != null && !InfiniteScrollManager.Instance.isScrolling)
            return;
            
        if (Time.time >= nextSpawnTime)
        {
            SpawnPlatformGroup();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnInitialPlatforms()
    {
        // Spawn platforms from current position to spawn distance
        for (float x = -10f; x < spawnDistance; x += platformWidth)
        {
            SpawnSinglePlatform(x, groundY);
        }
    }
    
    void SpawnPlatformGroup()
    {
        // Decide if we should create a gap
        bool createGap = Random.value < gapChance;
        
        if (createGap)
        {
            // Create a gap
            float gapSize = Random.Range(minGapSize, maxGapSize);
            lastSpawnX += gapSize;
            
            // Vary the ground height after gaps
            currentGroundY = groundY + Random.Range(-groundVariation, groundVariation);
        }
        
        // Spawn a group of platforms
        for (int i = 0; i < platformsPerSpawn; i++)
        {
            SpawnSinglePlatform(lastSpawnX, currentGroundY);
            lastSpawnX += platformWidth;
        }
    }
    
    void SpawnSinglePlatform(float x, float y)
    {
        Vector3 spawnPosition = new Vector3(x, y, 0f);
        GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
        
        // Add WorldMover component if it doesn't have one
        if (platform.GetComponent<WorldMover>() == null)
        {
            platform.AddComponent<WorldMover>();
        }
        
        // Set platform size
        platform.transform.localScale = new Vector3(platformWidth, platformHeight, 1f);
        
        // Make sure it has proper physics
        if (platform.GetComponent<Collider>() == null)
        {
            platform.AddComponent<BoxCollider>();
        }
    }
    
    GameObject CreateDefaultPlatform()
    {
        // Create a simple cube prefab
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "DefaultPlatform";
        
        // Add a material or color if you want
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
        }
        
        // Don't destroy the original, make it a prefab-like object
        cube.SetActive(false);
        
        return cube;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(spawnDistance, groundY - 2f, 0), new Vector3(spawnDistance, groundY + 2f, 0));
        
        // Draw ground level
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(-20f, groundY, 0), new Vector3(30f, groundY, 0));
        
        // Draw ground variation
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-20f, groundY + groundVariation, 0), new Vector3(30f, groundY + groundVariation, 0));
        Gizmos.DrawLine(new Vector3(-20f, groundY - groundVariation, 0), new Vector3(30f, groundY - groundVariation, 0));
    }
}