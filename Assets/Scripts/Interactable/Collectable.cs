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
                Player.instance.stats.SetHealth(Player.instance.characterData.maxHealth);
            }
            Destroy(gameObject);
        } 
    }
}
