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

                float damageMult = getDamageMult(other.gameObject.GetComponent<EnemyAI>());
                switch (controllerScript.GetActionState())
                {
                    case PlayerCombatController.PlayerState.Idle:

                        Debug.Log("Something is wrong, player sword hitbox hit enemy during idle.");
                        break;

                    case PlayerCombatController.PlayerState.Swing1:
                        Debug.Log("Hit swing 1!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing1Damage * damageMult,
                        0, controllerScript.swing1Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        changePlayerPH(other.gameObject.GetComponent<EnemyAI>());
                        break;

                    case PlayerCombatController.PlayerState.Swing2:
                        Debug.Log("Hit swing 2!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing2Damage * damageMult,
                        0, controllerScript.swing2Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        changePlayerPH(other.gameObject.GetComponent<EnemyAI>());
                        break;

                    case PlayerCombatController.PlayerState.Swing3:
                        Debug.Log("Hit swing 3!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing3Damage * damageMult,
                        0, controllerScript.swing3Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        changePlayerPH(other.gameObject.GetComponent<EnemyAI>());
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

    private void changePlayerPH(EnemyAI opponent) {
        if (opponent.naturalPH == TypesPH.Alkaline) {
            if (stats.inAcid) {
                stats.changeAcidity(1.0f);
                stats.changePH(1.0f);
                //stats.changePH(-1);
            } else {
                stats.changePH(1.5f);
                //stats.changeAcidity(-1);
            }
            
        } else if (opponent.naturalPH == TypesPH.Acidic) {
            if (stats.inAlkaline) {
                stats.changePH(1.0f);
                stats.changeAcidity(1.0f);
                //stats.changeAcidity(-0.3f);
            }  else {
                stats.changeAcidity(1.5f);
                //stats.changePH(-0.3f);
            }

        }

        
    }

    private float getDamageMult(EnemyAI opponent) {
        if (opponent.naturalPH == TypesPH.Alkaline) {
            return (1.5f * (stats.acid / 14f) + 0.5f);
        } else if (opponent.naturalPH == TypesPH.Acidic) {
            return (1.5f * (stats.ph / 14f) + 0.5f);
        } else {
            return 1;
        }
        
    }
}
