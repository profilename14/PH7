using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlatKit;

public class SeethroughController : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Seethrough"))
        {
            Color c = other.gameObject.GetComponent<Renderer>().material.color;
            c.a = 0.5f;
            other.gameObject.GetComponent<Renderer>().material.color = c;
            //other.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Seethrough"))
        {
            Color c = other.gameObject.GetComponent<Renderer>().material.color;
            c.a = 1;
            other.gameObject.GetComponent<Renderer>().material.color = c;
            //other.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }
}
