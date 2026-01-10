using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Readable : MonoBehaviour
{
    private bool nearPlayer;

    //private GameObject player;
    private DialogueSystemTrigger dialogueSystemTrigger;
    private ActivateableUI activateableUI;

    private void Start()
    {
        dialogueSystemTrigger = gameObject.GetComponent<DialogueSystemTrigger>();
        activateableUI = gameObject.GetComponentInChildren<ActivateableUI>();

        activateableUI.hideUI();

    }

    private void Update()
    {
        /*if (nearPlayer)
        {
            dialogueSystemTrigger.OnUse();
            nearPlayer = false;
            activateableUI.hideUI();

        }*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearPlayer = true;
            activateableUI.showUI();
            //Player.instance.playerActionManager.interactCallback += Activate;
            //print("Player is NOW near readable");
            dialogueSystemTrigger.OnUse();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearPlayer = false;
            activateableUI.hideUI();
            //Player.instance.playerActionManager.interactCallback -= Activate;
            //print("Player is NO LONGER near readable");
        }
    }

    private void Activate()
    {
        dialogueSystemTrigger.OnUse();
        activateableUI.hideUI();
    }
}
