using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    public GameObject EnemyPrefab;
    public Transform[] SpawnPoints;
    public int enemyNumber;
    int levelIndex;

    private void Awake()
    {
        Entity.OnDeath += EnemyDied;
        levelIndex = 1;
        LoadWave();
    }

    private void OnDestroy()
    {
        Entity.OnDeath -= EnemyDied;
    }

    void EnemyDied(bool isPlayer)
    {
        if (!isPlayer)
        {
            enemyNumber -= 1;
        }

        if (enemyNumber <= 0)
        {
            LoadWave();
        }
    }
    
    void Spawn()
    {
        if (EnemyPrefab == null || SpawnPoints.Length == 0)
        {
            Debug.LogError("EnemyPrefab ou SpawnPoints non assign� !");
            return;
        }

        int randomIndex = Random.Range(0, SpawnPoints.Length);
        Transform spawnPoint = SpawnPoints[randomIndex];

        GameObject newEnemy = Instantiate(EnemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("EnemyPrefab Spawned" + newEnemy);
    }

    public void SetLevelIndex(int levelnumber)
    {
        levelIndex = levelnumber;
    }

    public void LoadWave()
    {
        enemyNumber = levelIndex * 2;
        for (int i = 0; i < enemyNumber; i++)
        {
            Spawn();
        }

        levelIndex += 1;
    }
}
