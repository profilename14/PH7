using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    PlayerCombatController controllerScript;

    // Start is called before the first frame update
    void Start()
    {
        controllerScript = GetComponentInParent<PlayerCombatController>();
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
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.equippedWeapon.damage, controllerScript.equippedWeapon.phDamage, controllerScript.equippedWeapon.knockback, controllerScript.gameObject.transform.position);
        }
        else if (other.gameObject.CompareTag("Switch"))
        {
            other.gameObject.GetComponent<Switch>().Toggle();
        }
        else if (other.gameObject.CompareTag("BreakablePrefabContainer"))
        {
            other.gameObject.GetComponent<BreakablePrefabContainer>().Break();
        }
    }
}
