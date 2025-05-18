using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    [SerializeField] bool healsPlayer = false;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if (healsPlayer)
            {
                other.gameObject.GetComponentInParentOrChildren<PlayerStats>().SetHealth(10);
            }
            Destroy(gameObject);
        } 
    }
}
