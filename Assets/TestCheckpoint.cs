using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCheckpoint : MonoBehaviour
{
    [SerializeField]
    TestKillPlane killPlane;

    [SerializeField]
    Transform spawnPoint;

    private void Awake()
    {
        if (spawnPoint == null) spawnPoint = this.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            killPlane.playerSpawnPoint = spawnPoint;
        }
    }
}
