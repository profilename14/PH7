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
        activateableUI = transform.GetChild(0).gameObject.GetComponent<ActivateableUI>();

    }

    private void Update()
    {
        if(nearPlayer && (Input.GetKeyDown(KeyCode.E) || Input.GetButton("Jump")))
        {
            dialogueSystemTrigger.OnUse();
            activateableUI.hideUI();
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            nearPlayer = true;
            activateableUI.showUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            nearPlayer = false;
            activateableUI.hideUI();
        }
    }
}
