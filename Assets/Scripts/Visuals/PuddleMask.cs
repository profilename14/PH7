using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddleMask : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Puddle"))
        {
            Debug.Log("Puddle is masked");
            other.gameObject.GetComponentInChildren<MeshRenderer>().material.renderQueue = 3002;
        }
    }
}