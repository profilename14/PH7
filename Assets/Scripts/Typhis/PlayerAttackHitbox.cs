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

                switch (controllerScript.GetActionState())
                {
                    case PlayerCombatController.PlayerState.Idle:

                        Debug.Log("Something is wrong, player sword hitbox hit enemy during idle.");
                        break;

                    case PlayerCombatController.PlayerState.Swing1:
                        Debug.Log("Hit swing 1!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing1Damage,
                        0, controllerScript.swing1Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        break;

                    case PlayerCombatController.PlayerState.Swing2:
                        Debug.Log("Hit swing 2!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing2Damage,
                        0, controllerScript.swing2Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        break;

                    case PlayerCombatController.PlayerState.Swing3:
                        Debug.Log("Hit swing 3!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing3Damage,
                        0, controllerScript.swing3Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        break;
                    case PlayerCombatController.PlayerState.Dash:

                        Debug.Log("Something is wrong, player sword hitbox hit enemy during dashing.");
                        break;
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
