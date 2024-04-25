using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    PlayerCombatController controllerScript;
    PlayerStats stats;

    private float slowdownRate = 0.14f;
    private float slowdownLength = 0.02f;


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
            if (other.gameObject.GetComponent<EnemyAI>() != null) {
                if (!controllerScript.inThrust) {
                    //GameManager.slowdownTime(slowdownRate, slowdownLength);
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swordStats.damage,
                    controllerScript.swordStats.phDamage, controllerScript.swordStats.knockback + 20,
                    controllerScript.gameObject.transform.position);
                } else {
                    //GameManager.slowdownTime(slowdownRate / 2f, slowdownLength);
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.swordStats.damage * 1.5f,
                    controllerScript.swordStats.phDamage * 2f, controllerScript.swordStats.knockback * 5f,
                    controllerScript.gameObject.transform.position);
                }
            } else {
                if (!controllerScript.inThrust) {
                    GameManager.slowdownTime(slowdownRate, slowdownLength);
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.swordStats.damage,
                    controllerScript.swordStats.phDamage, controllerScript.swordStats.knockback,
                    controllerScript.gameObject.transform.position);
                } else {
                    GameManager.slowdownTime(slowdownRate / 2f, slowdownLength);
                    other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(controllerScript.swordStats.damage * 1.5f,
                    controllerScript.swordStats.phDamage * 2f, controllerScript.swordStats.knockback * 5f,
                    controllerScript.gameObject.transform.position);
                }
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
            if (other.gameObject.GetComponent<BreakablePrefabContainer>() != null) {
              other.gameObject.GetComponent<BreakablePrefabContainer>().Break();
            }

        }
    }
}
