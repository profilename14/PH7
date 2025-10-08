using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    [SerializeField] Vector3 offset;

    void Start() {
      if (spawnOnAwake) {
        objectIsAlive = true;
        makeTimer = 0;
        SpawnPrefab();
      }
    }

    public void SpawnPrefab()
    {
        curObject = Instantiate(objectToMake, transform.position + offset, Quaternion.identity);
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
