using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using UnityEngine.SceneManagement;

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

    [System.Serializable]
    public struct PatrolPoint
    {
        public GameObject[] patrolPoints;
    }

    private List<GameObject> aliveEnemies = new List<GameObject>();

    public SpawnPoint[] spawnPoints;

    public PatrolPoint[] patrolPoints;

    public EnemyWave[] enemiesToSpawn;
    
    public EnemyWave[] objectsToEnable; // Non enemy waves but it works considering they're gameobjects

    public bool spawnOnStart;

    public bool triggerOnPlayerEnter;

    // enemiesToSpawn uses enemies already in the room and does not spawn them, only using them to check if they are alive or not
    public bool usePreExistingEnemies;

    public int waves;

    public int currentWave = 0;

    public float waveDelay;
    public float waveSpawnDelay = 0;

    private bool spawningEnemies;

    public GameObject player;

    public GameObject[] wallsToEnable;

    private bool activated = false;

    [SerializeField] private bool lockedDoor = false;
    [SerializeField] SceneSwitchTrigger doorPrefab;

    [SerializeField] private int combatRoomID = 0; // must be unique
    private int sceneID;

    // Start is called before the first frame update
    void Start()
    {

        sceneID = SceneManager.GetActiveScene().buildIndex;
        if (GameManager.instance.clearedCombatRooms[sceneID].TryGetValue(combatRoomID, out bool isCleared))
        {
            Debug.Log("Checking: " + isCleared);
            if (isCleared == true)
            {
                triggerOnPlayerEnter = false;
                spawnOnStart = false;
            }
        }
        else // first load of this scene
        {
            GameManager.instance.clearedCombatRooms[sceneID][combatRoomID] = false;
        }


        if (spawnOnStart)
        {
            activated = true;
            spawningEnemies = true;
            StartCoroutine(EnableObjectSet(0, 0));
            if(!usePreExistingEnemies) StartCoroutine(SpawnEnemyWave(0, 0));
            else
            {
                aliveEnemies.AddRange(enemiesToSpawn[0].enemies);
                spawningEnemies = false;
                currentWave = 1;
            }
            foreach (GameObject g in wallsToEnable)
            {
                g.SetActive(true);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        aliveEnemies.RemoveAll((GameObject g) => !g.activeInHierarchy);
        /*if (aliveEnemies.Count > 0)
        {
            while (aliveEnemies.Count > 0 && aliveEnemies[0] == null)
            {
                aliveEnemies.RemoveAt(0);
            }
        }
        */

        /*for(int i = 0; i < aliveEnemies.Count; i++)
        {
            if(aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }*/

        /*if (Input.GetKey(KeyCode.K) && true) {
            for(int i = 0; i < aliveEnemies.Count; i++)
            {
                //aliveEnemies.RemoveAt(i);
                foreach(GameObject g in aliveEnemies)
                {
                    Destroy(g);
                }
            }
        }*/

        if(aliveEnemies.Count == 0 && !spawningEnemies && activated)
        {
            if (currentWave < waves)
            {
                //Debug.Log("Spawned from next wave");
                spawningEnemies = true;
                StartCoroutine(EnableObjectSet(currentWave, 0));
                StartCoroutine(SpawnEnemyWave(currentWave, waveDelay));
            }
            else
            {
                activated = false;
                foreach (GameObject g in wallsToEnable)
                {
                    g.SetActive(false);

                }
                if (lockedDoor == true)
                {
                    doorPrefab.unlock();
                }

                GameManager.instance.clearedCombatRooms[sceneID][combatRoomID] = true;
            }
        }
    }

    IEnumerator SpawnEnemyWave(int waveNumber, float delay)
    {
        yield return new WaitForSeconds(delay);
        for(int i = 0; i < spawnPoints[waveNumber].spawnPoints.Length; i++)
        {
            if (spawnPoints[waveNumber].spawnPoints[i] == null || enemiesToSpawn[waveNumber].enemies[i] == null) continue;
            yield return new WaitForSeconds(waveSpawnDelay);

            GameObject enemy = Instantiate(enemiesToSpawn[waveNumber].enemies[i], spawnPoints[waveNumber].spawnPoints[i]);
            EnemyPatrol enemyPatrol = enemy.GetComponentInParentOrChildren<EnemyPatrol>();
            if (enemyPatrol != null)
            {
                enemyPatrol.InitPatrolPoints(patrolPoints[waveNumber].patrolPoints.Length, patrolPoints[waveNumber].patrolPoints);
            }
            Vector3 location = new Vector3 (enemy.gameObject.transform.position.x, 0, enemy.gameObject.transform.position.z);
            enemy.gameObject.transform.position = spawnPoints[waveNumber].spawnPoints[i].transform.position;
            aliveEnemies.Add(enemy);
            if(enemy.gameObject.GetComponentInParentOrChildren<RoamingEnemyActionManager>() != null)
            {
                //Debug.Log("Spotted Player!");
                enemy.gameObject.GetComponentInParentOrChildren<RoamingEnemyActionManager>().SpottedPlayer();
            }

        }
        spawningEnemies = false;
        currentWave++;
    }

    IEnumerator EnableObjectSet(int waveNumber, float delay)
    {
        if (objectsToEnable.Length <= waveNumber) {
            yield break;
        }
        for(int i = 0; i < objectsToEnable[waveNumber].enemies.Length; i++)
        {
            yield return new WaitForSeconds(delay);
            if (objectsToEnable[waveNumber].enemies[i] == null) continue;

            objectsToEnable[waveNumber].enemies[i].SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && triggerOnPlayerEnter)
        {
            activated = true;
            GetComponent<BoxCollider>().enabled = false;
            spawningEnemies = true;
            if (!usePreExistingEnemies) StartCoroutine(SpawnEnemyWave(0, 0));
            else
            {
                aliveEnemies.AddRange(enemiesToSpawn[0].enemies);
                spawningEnemies = false;
                currentWave = 1;
            }
            foreach (GameObject g in wallsToEnable)
            {
                g.SetActive(true);
            }
        }
    }
}
