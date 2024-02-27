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

            int curAttackState = controllerScript.weaponSwingCombo;
            if (curAttackState == 0 || curAttackState == 1) {
              other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.equippedWeapon.damage,
                controllerScript.equippedWeapon.phDamage, controllerScript.equippedWeapon.knockback,
                controllerScript.gameObject.transform.position);
            } else if (curAttackState == 2) {
              other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.equippedWeapon.damage * 1.5f,
                controllerScript.equippedWeapon.phDamage * 1.5f, controllerScript.equippedWeapon.knockback * 1.75f,
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
