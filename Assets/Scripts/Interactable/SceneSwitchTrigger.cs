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
    private ActivateableUI activateableUI;
    public bool locked = false;

    void Awake() {
        if (isDoor) {
            activateableUI = transform.GetChild(0).gameObject.GetComponent<ActivateableUI>();
        }
    }

    private void Update()
    {
        if(touchingPlayer && isDoor && (Input.GetKeyDown(KeyCode.E) || Input.GetButton("Jump")))
        {
            if (locked == false) {
                player.transform.position = spawnPosition;
                player.GetComponent<PlayerStatsOLD>().spawnpoint = spawnPosition;
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            player = other.gameObject;
            touchingPlayer = true;
            if (locked == false && isDoor) {
                activateableUI.showUI();
            }
            
            if(!isDoor)
            {
                player.GetComponent<PlayerStatsOLD>().spawnpoint = spawnPosition;
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            touchingPlayer = false;
            if (locked == false && isDoor) {
                activateableUI.hideUI();
            }
        }
    }

    public void unlock() {
        if (locked) {
            locked = false;
        }
    }
}
