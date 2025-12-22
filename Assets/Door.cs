using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string sceneToLoad;

    public string thisDoorId;

    public string destinationDoorId;

    public Transform typhisEntranceTransform;

    public GameObject tooltip;

    bool canSwitchScene = false;

    private float startTimer = 1f;

    void Start()
    {
        
    }

    void Update()
    {
        if (startTimer > 0)
        {
            startTimer -= Time.deltaTime;
            if (startTimer <= 0)
            {
                canSwitchScene = true;
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.instance.playerActionManager.interactCallback += SwitchScene;
            tooltip.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.instance.playerActionManager.interactCallback -= SwitchScene;
            tooltip.SetActive(false);
        }
    }

    public void SwitchScene()
    {
        if (!canSwitchScene) return;

        Player.instance.playerActionManager.UIManager.loadingScreen.fadeToBlackDoor();

        GameManager.instance.LoadNewScene(sceneToLoad, destinationDoorId);

        canSwitchScene = false;
    }
}
