using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    PlayerCombatController controllerScript;
    PlayerStats stats;

    private float slowdownRate = 0.14f;
    private float slowdownLength = 0.02f;

    [SerializeField] Animator playerAnim;
    float hitStop = 0.1f;
    private float hitStopTimer = 0;



    // Start is called before the first frame update
    void Start()
    {
        controllerScript = GetComponentInParent<PlayerCombatController>();
        stats = GetComponentInParent<PlayerStats>();
        playerAnim = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hitStopTimer > 0) {
            hitStopTimer -= Time.deltaTime;
            if (hitStopTimer <= 0) {
                playerAnim.speed = 1f;
            }
        }
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
                        if (other.gameObject.GetComponent<EnemyAI>().health < -10) {
                            Destroy(other.gameObject);
                        }
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing1Damage * damageMult,
                        0, controllerScript.swing1Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        changePlayerPH(other.gameObject.GetComponent<EnemyAI>());
                        break;

                    case PlayerCombatController.PlayerState.Swing2:
                        Debug.Log("Hit swing 2!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        if (other.gameObject.GetComponent<EnemyAI>().health < -10) {
                            Destroy(other.gameObject);
                        }
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing2Damage * damageMult,
                        0, controllerScript.swing2Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        changePlayerPH(other.gameObject.GetComponent<EnemyAI>());
                        break;

                    case PlayerCombatController.PlayerState.Swing3:
                        Debug.Log("Hit swing 3!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        if (other.gameObject.GetComponent<EnemyAI>().health < -10) {
                            Destroy(other.gameObject);
                        }
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(controllerScript.swing3Damage * damageMult,
                        0, controllerScript.swing3Knockback,
                        controllerScript.gameObject.transform.parent.right, EnemyAI.DamageSource.Sword);
                        changePlayerPH(other.gameObject.GetComponent<EnemyAI>());
                        break;

                    case PlayerCombatController.PlayerState.Spinslash:
                        Debug.Log("Hit swing 3!");
                        other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        EnemyAI targetEnemy = other.gameObject.GetComponent<EnemyAI>();
                        if (controllerScript.alkalineSlash == true) {
                            if (targetEnemy.naturalPH == TypesPH.Acidic) {
                                targetEnemy.debuffTimer += 7f;
                                damageMult = 6;
                            }
                        } else if (controllerScript.acidSlash == true) {
                            if (targetEnemy.naturalPH == TypesPH.Alkaline) {
                                targetEnemy.debuffTimer += 7f;
                                damageMult = 6;
                            }
                        }
                        if (other.gameObject.GetComponent<EnemyAI>().health < -10) { // If you spam enemies in death animation they die faster
                            Destroy(other.gameObject);
                        }
                        other.gameObject.GetComponent<EnemyAI>().TakeDamage(30 * damageMult,
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
                hitStopTimer = hitStop;
                playerAnim.speed = 0.0f;
                other.GetComponent<EnemyAI>().hitPause();
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
        else if (other.gameObject.CompareTag("HasPH"))
        {
            ObjectWithPH phObject = other.gameObject.GetComponent<ObjectWithPH>();
            if (phObject != null && phObject.canBeAttacked) {
                if (phObject.phOnHit > 0) {
                    stats.changePH(phObject.phOnHit);
                } else {
                    stats.changeAcidity(-phObject.phOnHit);
                }
                phObject.ChangePH(-3);
                phObject.instantiateParticles();
                
            }

        }
    }

    private void changePlayerPH(EnemyAI opponent) {
        if (opponent.debuffTimer > 0) {
            //stats.makeScreenshake();
        }
        if (opponent.naturalPH == TypesPH.Alkaline) {
            if (stats.inAcid || opponent.debuffTimer > 0) {
                stats.changeAcidity(1.0f);
                stats.changePH(2f);
                //stats.changePH(-1);
            } else {
                stats.changePH(1.5f);
                //stats.changeAcidity(-1);
                if (stats.inAlkaline || opponent.debuffTimer < 0) {
                    stats.changePH(1.0f);
                }
            }
            
        } else if (opponent.naturalPH == TypesPH.Acidic) {
            if (stats.inAlkaline || opponent.debuffTimer > 0) {
                stats.changePH(1.0f);
                stats.changeAcidity(2f);
                //stats.changeAcidity(-0.3f);
            }  else {
                stats.changeAcidity(1.5f);
                //stats.changePH(-0.3f);
                if (stats.inAcid || opponent.debuffTimer < 0) {
                    stats.changeAcidity(1.0f);
                }
            }

        }

        
    }

    private float getDamageMult(EnemyAI opponent) {
        if (opponent.naturalPH == TypesPH.Alkaline) {
            return (0f * (stats.acid / 14f) + 1f);
        } else if (opponent.naturalPH == TypesPH.Acidic) {
            return (0f * (stats.ph / 14f) + 1f);
        } else {
            return 1;
        }
        
    }
}
