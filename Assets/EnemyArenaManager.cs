using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArenaManager : MonoBehaviour
{
    [System.Serializable]
    public struct EnemyWave
    {
        public GameObject[] enemies;
    }

    [System.Serializable]
    public struct SpawnPoint
    {
        public Transform[] spawnPoints;
    }

    public List<GameObject> aliveEnemies = new List<GameObject>();

    public SpawnPoint[] spawnPoints;

    public EnemyWave[] enemiesToSpawn;

    public bool spawnOnStart;

    public bool spawnOnPlayerEnter;

    public int waves;

    public int currentWave = 0;

    public float waveDelay;

    private bool spawningEnemies;

    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        if(spawnOnStart)
        {
            spawningEnemies = true;
            StartCoroutine(SpawnEnemyWave(0, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < aliveEnemies.Count; i++)
        {
            if(aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }

        if(aliveEnemies.Count == 0 && !spawningEnemies && currentWave < waves)
        {
            spawningEnemies = true;
            StartCoroutine(SpawnEnemyWave(currentWave, waveDelay));
        }
    }

    IEnumerator SpawnEnemyWave(int waveNumber, float delay)
    {
        yield return new WaitForSeconds(delay);
        for(int i = 0; i < spawnPoints[waveNumber].spawnPoints.Length; i++)
        {
            if (spawnPoints[waveNumber].spawnPoints[i] == null || enemiesToSpawn[waveNumber].enemies[i] == null) continue;

            GameObject enemy = Instantiate(enemiesToSpawn[waveNumber].enemies[i], spawnPoints[waveNumber].spawnPoints[i]);
            enemy.GetComponent<EnemyBehavior>().AlertEnemy();
            enemy.GetComponent<EnemyBehavior>().target = player.transform;
            aliveEnemies.Add(enemy);
        }
        spawningEnemies = false;
        currentWave++;
    }
}
