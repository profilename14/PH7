using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActivateableUI : MonoBehaviour
{
    private Vector3 originalScale;
    private bool isInvisible;
    [SerializeField] private bool startActive = false;

    
    Transform camTransform;

    void Start() {
        camTransform = GameObject.FindWithTag("MainCamera").transform;
        if (GameObject.Find("Main Camera"))
        {
            gameObject.GetComponent<Canvas>().worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }



        originalScale = transform.localScale;  
        if (!startActive) {
            transform.localScale = new Vector3(0, 0, 0); // Make the UI invisible until activated
            isInvisible = true;
        }
        else {
            isInvisible = false;
        }
        
    }

    void LateUpdate()
    {
        if (!isInvisible) {
            transform.LookAt(transform.position + camTransform.forward);
        }
        
    }

    public void showUI() {
        if (isInvisible) {
            transform.LookAt(transform.position + camTransform.forward);
            isInvisible = false;
            transform.localScale = originalScale;
        }
    }

    public void hideUI() {
        if (!isInvisible) {
            isInvisible = true;
            transform.localScale = new Vector3(0, 0, 0);
        }
    }




}