using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    PlayerAttackController controllerScript;

    // Start is called before the first frame update
    void Start()
    {
        controllerScript = GetComponentInParent<PlayerAttackController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit " + other.gameObject.name);
            Debug.Log("Dealt " + controllerScript.equippedWeapon.damage + " damage");
            other.gameObject.GetComponent<EnemyCombatController>().TakeDamage(controllerScript.equippedWeapon.damage, controllerScript.equippedWeapon.phDamage, controllerScript.equippedWeapon.knockback, controllerScript.gameObject.transform.position);
        }
    }
}
