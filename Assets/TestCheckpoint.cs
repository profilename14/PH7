using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCheckpoint : MonoBehaviour
{
    [SerializeField]
    TestKillPlane killPlane;

    [SerializeField]
    Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            killPlane.playerSpawnPoint = spawnPoint;
        }
    }
}
