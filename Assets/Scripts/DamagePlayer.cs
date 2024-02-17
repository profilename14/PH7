using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Serialization;

public class DamagePlayer : MonoBehaviour
{
    // This script is temporary and should be integrated into the enemy behavior script and player combat script at some point
    public float damage;
    public float knockback = 2.5f; 
    public float damagePh = 0f;
    public float damageVol = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStats>().playerDamage(damage, damagePh, damageVol, gameObject.transform.position, knockback);
        }
    }
}
