using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    // This script is temporary and should be integrated into the enemy behavior script and player combat script at some point
    public float damage;
    public float knockback = 2.5f;
    public float phDamage = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerStats>().playerDamage(damage, phDamage, gameObject.transform.position, knockback);
        }
    }
}