using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public string sceneToLoad;

    public string thisDoorId;

    public string destinationDoorId;

    public Transform typhisEntranceTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.instance.playerActionManager.interactCallback += SwitchScene;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.instance.playerActionManager.interactCallback -= SwitchScene;
        }
    }

    public void SwitchScene()
    {
        GameManager.instance.LoadNewScene(sceneToLoad, destinationDoorId);
    }
}
