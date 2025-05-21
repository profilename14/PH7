using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RespawnFountain : MonoBehaviour
{
    public Transform typhisRespawnPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Respawn position set!");
            GameManager.instance.respawnPosition = typhisRespawnPosition.position;
            Player.instance.stats.SetHealth(Player.instance.characterData.maxHealth);
            GameManager.instance.sceneToLoadOnRespawn = SceneManager.GetActiveScene().name;
        }
    }
}
