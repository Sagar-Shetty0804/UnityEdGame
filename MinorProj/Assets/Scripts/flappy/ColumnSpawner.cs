// Enhanced ColumnSpawner.cs
using UnityEngine;

public class ColumnSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject columnPrefab;
    public float spawnInterval = 3f;
    public float spawnXPosition = 12f;
    
    [Header("Column Positioning")]
    public float[] tileYPositions = {3.3f, 1.1f, -1.1f, -3.3f}; // Y positions for the 4 tiles
    
    private float nextSpawnTime;
    private bool isSpawning = true;

    void Start()
    {
        nextSpawnTime = Time.time + spawnInterval;
        Debug.Log("Column Spawner Started!");
        SpawnColumn();
    }
    
    void Update()
    {
        if (isSpawning && Time.time >= nextSpawnTime)
        {
            SpawnColumn();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    void SpawnColumn()
    {
        if (columnPrefab != null)
        {
            Vector3 spawnPosition = new Vector3(spawnXPosition, 0, 0);
            GameObject newColumn = Instantiate(columnPrefab, spawnPosition, Quaternion.identity);
            
            Debug.Log("Column spawned at: " + spawnPosition);
        }
        else
        {
            Debug.LogError("Column prefab is not assigned!");
        }
    }
    
    public void StartSpawning()
    {
        isSpawning = true;
        nextSpawnTime = Time.time + spawnInterval;
        Debug.Log("Column spawning started");
    }
    
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("Column spawning stopped");
    }
}