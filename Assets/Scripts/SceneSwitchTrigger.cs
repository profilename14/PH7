using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public Vector3 spawnPosition;

    public bool isDoor;
    private bool touchingPlayer;

    private GameObject player;

    private void Update()
    {
        if(touchingPlayer && isDoor && Input.GetKeyDown(KeyCode.E))
        {
            player.transform.position = spawnPosition;
            player.GetComponent<PlayerStats>().spawnpoint = spawnPosition;
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.gameObject;
            touchingPlayer = true;
            if(!isDoor)
            {
                player.GetComponent<PlayerStats>().spawnpoint = spawnPosition;
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = false;
        }
    }
}
