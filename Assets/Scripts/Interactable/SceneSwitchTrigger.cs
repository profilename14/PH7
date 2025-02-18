using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitchTrigger : MonoBehaviour
{
    public string sceneToLoad;

    public bool isDoor;

    private ActivateableUI activateableUI;
    public bool locked = false;

    void Awake() {
        if (isDoor) {
            //activateableUI = transform.GetChild(0).gameObject.GetComponent<ActivateableUI>();
        }
    }

    private void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Player.instance.playerActionManager.interactCallback += SwitchScene;
            
            /*if (locked == false && isDoor) {
                activateableUI.showUI();
            }*/
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.instance.playerActionManager.interactCallback -= SwitchScene;

            /*if (locked == false && isDoor) {
                activateableUI.hideUI();
            }*/
        }
    }

    public void unlock() {
        if (locked) {
            locked = false;
        }
    }

    public void SwitchScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
