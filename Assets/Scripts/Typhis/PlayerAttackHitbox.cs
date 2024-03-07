using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    PlayerCombatController controllerScript;
    PlayerStats stats;

    // Start is called before the first frame update
    void Start()
    {
        controllerScript = GetComponentInParent<PlayerCombatController>();
        stats = GetComponentInParent<PlayerStats>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Enemy"))
        {
            //Debug.Log("Hit " + other.gameObject.name);
            //Debug.Log("Dealt " + controllerScript.equippedWeapon.damage + " damage");
            if (!controllerScript.inThrust) {
                other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.swordStats.damage,
                controllerScript.swordStats.phDamage, controllerScript.swordStats.knockback,
                controllerScript.gameObject.transform.position);
            } else {
                other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
              other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.swordStats.damage * 2f,
                controllerScript.swordStats.phDamage * 2f, controllerScript.swordStats.knockback * 10,
                controllerScript.gameObject.transform.position);
            }

            if (other.gameObject.GetComponent<OnHitVFX>() != null)
            {
                OnHitVFX vfx = other.gameObject.GetComponent<OnHitVFX>();
                vfx.HitVFX();
            }


        }
        else if (other.gameObject.CompareTag("Switch"))
        {
            if (other.gameObject.GetComponent<Switch>() != null) {
              other.gameObject.GetComponent<Switch>().Toggle();
            }
            //Debug.Log(controllerScript.weaponSwingCombo);
        }
        else if (other.gameObject.CompareTag("BreakablePrefabContainer"))
        {
            other.gameObject.GetComponent<BreakablePrefabContainer>().Break();
        }
    }
}
