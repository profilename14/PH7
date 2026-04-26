using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject objectToMake;
    public bool objectIsAlive = false;
    public bool spawnRepeating = true;
    public float spawnInterval = 6f;
    float makeTimer = 0.0f;
    private GameObject curObject = null;
    [SerializeField] bool spawnOnAwake = false;
    [SerializeField] float spawnOnAwakeDelay;
    [SerializeField] Vector3 offset;

    [SerializeField]
    bool useTransformPoints;

    [SerializeField]
    int numberToSpawn = 1;

    [SerializeField]
    List<Transform> transformPoints;

    void Start() {
      if (spawnOnAwake) {
            if(spawnOnAwakeDelay == 0)
            {
                objectIsAlive = true;
                makeTimer = 0;
                SpawnPrefab();
            }
            else
            {
                Invoke("SpawnPrefab", spawnOnAwakeDelay);
            }
      }
    }

    public void SpawnPrefab()
    {
        if (!useTransformPoints)
        {
            curObject = Instantiate(objectToMake, transform.position + offset, Quaternion.identity);
        }
        else
        {
            IListExtensions.Shuffle(transformPoints);

            for (int i = 0; i < numberToSpawn; i++)
            {
                Instantiate(objectToMake, transformPoints[i % transformPoints.Count].position + offset, Quaternion.identity);
            }
        }
    }

    void Update() {
        if (spawnRepeating)
        {
            if (objectIsAlive == false)
            {
                makeTimer += Time.deltaTime;
                if (makeTimer > spawnInterval)
                {
                    makeTimer = 0;
                    objectIsAlive = true;
                    //Vector3 spawnPoint = new Vector3 (transform.position.x, transform.position.y + 1, transform.position.z);
                    curObject = Instantiate(objectToMake, transform.position, Quaternion.identity);

                    Debug.Log("Remade object!");
                }
            }
            else
            {
                if (curObject == null)
                {
                    objectIsAlive = false;
                }
            }
        }
    }


}
