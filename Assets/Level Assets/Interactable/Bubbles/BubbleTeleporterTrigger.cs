using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class BubbleTeleporterTrigger : MonoBehaviour
{
    [SerializeField]
    Transform bubblewarpPosition;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Bubble"))
        {
            other.gameObject.transform.position = bubblewarpPosition.position;
            other.gameObject.GetComponentInParentOrChildren<Rigidbody>().velocity = Vector3.zero;
        }
    }
}
