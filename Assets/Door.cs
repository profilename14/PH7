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

    bool canSwitchScene = true;

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

        GameManager.instance.LoadNewScene(sceneToLoad, destinationDoorId);

        canSwitchScene = false;
    }
}
