using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;

using Patterns;
using UnityEngine.AI;
using System.Linq.Expressions;
using UnityEngine.UIElements;

public class EnemyAI : MonoBehaviour
{
    #region Base Enemy Data

    public FSM fsm;
    public RichAI ai;
    public Animator anim;
    public string currentState;

    protected GameObject player;

    [Header("Detection")]
    public bool playerDetected = false;
    public float sightDistance = 20f;

    public float defaultRotationSpeed = 270;

    [Header("Follow State")]
    public float followDistance = 10f;
    public float followMoveSpeed = 6;
    public float followAcceleration = 15;

    protected GameObject target;
    private LayerMask mask;

    [Header("Circling (Follow State Variant)")]
    public bool isCircling = false;
    public bool isCirclingRight = true;
    public float circleDistance = 1.25f;
    public float circlingMoveSpeed = 10;
    public float circlingAcceleration = 20;
    [SerializeField] private GameObject rotationAnchor;
    [SerializeField] private Rigidbody enemyRigidbody;

    [Header("Enemy Base Data")]
    // This is more hp related stuff to handle the global enemy armor system.
    public TypesPH naturalPH;
    //public float maxArmorRatio = 0.5f; // The maximum percent of the health bar with armor can be fine tuned between enemies.
    // Ex, striders could have 20% armor, making them quite weak but always regen to the same point until almost defeated.
    public float maxHealth = 100f;
    public float maxArmor = 50f;
    public float health;
    public float armor;
    public float debuffTimer = 0;
    public float debuffTimerMax = 10f;
    public float armorResistMultiplier = 0.33f;
    public bool armorBroken;
    public bool isHitstunned;
    public bool wasHitstunned = false;
    public bool canBeHitstunned = true;
    public bool inInterruptFrames;
    public bool inPuddle;
    public float puddleTickInterval = 0.5f;
    private bool isDead = false;
    private float hitStopTimer = 0;

    //private float armorRegenTimer = 0.0f;
    //[SerializeField] private float regenArmorStartSpeed = 3.0f; // Time in seconds until armor regens. Lower = faster.
    [SerializeField] public AudioSource audioSource;
    [SerializeField] private AudioClip enemyArmoredImpactSound;
    [SerializeField] private AudioClip enemyImpactSound;
    //[SerializeField] private AudioClip enemyPotHitSound;

    private float slowdownRate = 0.14f;
    private float slowdownLength = 0.02f;

    public enum DamageSource {Sword, Rock, Pot, Puddle};

    #endregion

    // Start is called before the first frame update
    public void Start()
    {
        fsm = new FSM();
        player = GameObject.FindWithTag("Player");

        fsm.Add("Idle", new EnemyState(fsm, "Idle", this));
        fsm.Add("Follow", new EnemyState(fsm, "Follow", this));
        fsm.Add("Hitstun", new EnemyState(fsm, "Hitstun", this));
        fsm.Add("Die", new EnemyState(fsm, "Die", this));
        Init_Idle();
        Init_Follow();
        Init_Hitstun();
        Init_Die();

        fsm.SetCurrentState("Idle");

        mask = LayerMask.GetMask("Player", "Obstacles");

        target = new GameObject(this.gameObject.name + " AI Target");
        //var iconContent = EditorGUIUtility.IconContent("sv_label_1");
        //EditorGUIUtility.SetIconForObject(target, (Texture2D)iconContent.image);

        health = maxHealth;
        armor =  maxArmor;
    }

    // Update is called once per frame
    public void Update()
    {
        currentState = fsm.GetCurrentState();
        checkHealth();

        setDestination();

        fsm.Update();

        //HitstunCheck();
    }

    public void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    #region Helper Functions

    private void setDestination()
    {
        // Make sure the AI is always trying to move towards our target object
        ai.destination = target.transform.position;
    }

    private void checkHealth()
    {
        if (hitStopTimer > 0) {
            hitStopTimer -= Time.deltaTime;
            if (hitStopTimer <= 0) {
                anim.speed = 1f;
            }
        }
        if (debuffTimer > 0) {
            debuffTimer -= Time.deltaTime;
            if (debuffTimer < 0) {
                debuffTimer = 0;
            }
        }
        else if (debuffTimer < 0) {
            debuffTimer += Time.deltaTime;
            if (debuffTimer > 0) {
                debuffTimer = 0;
            }
        }
        /*if (armor < maxArmorRatio * health)
        {
            armorRegenTimer += Time.deltaTime;

        }
        else if (armor >= maxArmorRatio * health)
        {
            armor = maxArmorRatio * health;
            armorRegenTimer = 0.0f;
        }*/

        /*if (armorRegenTimer > regenArmorStartSpeed)
        {
            armor += Time.deltaTime * (maxArmorRatio * health / 5.0f); // It takes 5 seconds of regen to fully recover armor.
            armorBroken = false;
        }*/

        /*if (health < 0)
        {
            Destroy(gameObject);
        }*/
    }

    public virtual void TakeDamage(float damage, float changeInPh, float knockback, Vector3 knockbackDir, DamageSource source)
    {
        

        if(health <= 0)
        {
            if (isDead) return;
            else fsm.SetCurrentState("Die");
        }

        if (source == DamageSource.Puddle) {
            if ((changeInPh < 0 && naturalPH == TypesPH.Alkaline) || (changeInPh > 0 && naturalPH == TypesPH.Acidic)) {
                debuffTimer = Mathf.Clamp(debuffTimer + Mathf.Abs(changeInPh), -1, debuffTimerMax);
            }
            
            if ((changeInPh > 0 && naturalPH == TypesPH.Alkaline) || (changeInPh < 0 && naturalPH == TypesPH.Acidic)) {
                debuffTimer = Mathf.Clamp(debuffTimer - Mathf.Abs(changeInPh), -1, debuffTimerMax);
            }
            
            return; // quick fix to the sound bug
        }

        float displayedDamage = 0;

        if (!armorBroken)
        {
            if (debuffTimer > 0) {
                armor -= damage;
                displayedDamage += damage;
            } else {
                armor -= damage * armorResistMultiplier;
                displayedDamage += damage * armorResistMultiplier;
            }
            

            if((changeInPh < 0 && naturalPH == TypesPH.Alkaline) || (changeInPh > 0 && naturalPH == TypesPH.Acidic))
            {
                debuffTimer = Mathf.Clamp(debuffTimer + Mathf.Abs(changeInPh), -1, debuffTimerMax);
                //Debug.Log("It's super effective!");
                // Opposite pH should deal damage directly to armor.
                //armor -= Mathf.Abs(changeInPh);

                displayedDamage += Mathf.Abs(changeInPh);
            }
            else if ((changeInPh > 0 && naturalPH == TypesPH.Alkaline) || (changeInPh < 0 && naturalPH == TypesPH.Acidic))
            {
                debuffTimer = Mathf.Clamp(debuffTimer - Mathf.Abs(changeInPh) / 2, -1, debuffTimerMax);
                //Debug.Log("Healing armor");
                // Same pH heals armor.
                //armor = Mathf.Clamp(armor + (armor * 1/Mathf.Abs(changeInPh)),0,maxArmor);
                //armorBroken = false;
                displayedDamage -= Mathf.Abs(changeInPh);
            }

            if(source == DamageSource.Pot)
            {
                //audioSource.PlayOneShot(enemyPotHitSound, 0.45F);
            }
            else if(source == DamageSource.Sword)
            {
                audioSource.PlayOneShot(enemyArmoredImpactSound, 0.25F);
            }
            else if (source == DamageSource.Puddle && (changeInPh < 0 && naturalPH == TypesPH.Alkaline) || (changeInPh > 0 && naturalPH == TypesPH.Acidic))
            {
                //audioSource.PlayOneShot(enemyImpactSound, 0.45F);
            }

            //Debug.Log("Damage: " + displayedDamage + " Against Armor");

            if (armor <= 0)
            {
                armor = 0;
                armorBroken = true;
            }
        }
        else
        {
            
            if (debuffTimer > 0) {
                if (armorResistMultiplier != 0) {
                    Debug.Log("Something has 0 armor resist multiplier!");
                    health -= damage / armorResistMultiplier;
                } else {
                    health -= damage * 2;
                }
                
            } else {
                health -= damage;
            }
            displayedDamage += damage;

            if ((changeInPh < 0 && naturalPH == TypesPH.Alkaline) || (changeInPh > 0 && naturalPH == TypesPH.Acidic))
            {
                debuffTimer = Mathf.Clamp(debuffTimer + Mathf.Abs(changeInPh), -1, debuffTimerMax);
                // Opposite pH should deal reduced damage to health.
                //health -= Mathf.Abs(changeInPh) * armorResistMultiplier;

                displayedDamage += Mathf.Abs(changeInPh);
            }
            else if ((changeInPh > 0 && naturalPH == TypesPH.Alkaline) || (changeInPh < 0 && naturalPH == TypesPH.Acidic))
            {
                if (debuffTimer > 0) {
                    debuffTimer = Mathf.Clamp(debuffTimer - Mathf.Abs(changeInPh) / 2, -1, debuffTimerMax);
                }
                // Same pH heals armor.
                //armor = 0;
                //armor = Mathf.Clamp(armor + (armor * 1 / Mathf.Abs(changeInPh)), 0, maxArmor);
                //armorBroken = false;
                displayedDamage -= Mathf.Abs(changeInPh);
            }

            //Debug.Log("Damage: " + displayedDamage + " Against Health");

            if (health <= 0)
            {
                if(source == DamageSource.Sword) audioSource.PlayOneShot(enemyImpactSound, 0.375F);
                //else if(source == DamageSource.Pot) audioSource.PlayOneShot(enemyPotHitSound, 0.375F);

                fsm.SetCurrentState("Die");
                isDead = true;
            }
            else
            {
                audioSource.PlayOneShot(enemyImpactSound, 0.3F);
                if (!isHitstunned && inInterruptFrames && source != DamageSource.Puddle)
                {
                    //Debug.Log("Hitstun!");
                    if (canBeHitstunned) {
                        fsm.SetCurrentState("Hitstun");
                    } else {
                        Debug.Log("Immune to Hitstun!");
                    }
                    
                }
            }
        }

        /*if (damage > 9.9) {
          Transform PopupTransform = Instantiate(PopupPrefab, transform.position, Quaternion.identity);
          DamagePopup popup = PopupTransform.GetComponent<DamagePopup>();
          popup.Setup(displayedDamage);
        }*/

        
        StartCoroutine(applyDelayedKnockback(0.00f, knockbackDir, knockback));
        
    }

    

    public void EnteredPuddle(float damage, float pHChange)
    {
        if (inPuddle == false)
        {
            inPuddle = true;
            StartCoroutine(PuddleDamageTicks(damage, pHChange));
        }
    }

    public IEnumerator applyDelayedKnockback(float time, Vector3 knockbackDir, float knockback) {
        if (time > 0) {
            circlingMoveSpeed /= 50;
            followMoveSpeed /= 50;
            yield return new WaitForSeconds(time);
            
            circlingMoveSpeed *= 50;
            followMoveSpeed *= 50;
        }
        

        Vector3 dir = knockbackDir.normalized;
        Vector3 velocity = dir * knockback;
        enemyRigidbody.AddForce(velocity, ForceMode.Impulse);

        Debug.Log("AAAAAAAAAAAAAAAAAAAAA");
    }

    public IEnumerator PuddleDamageTicks(float damage, float pHChange)
    {
        while(inPuddle)
        {
            TakeDamage(damage, pHChange, 0, Vector3.zero, DamageSource.Puddle);
            yield return new WaitForSeconds(puddleTickInterval);
        }
    }

    public void StopHitstun()
    {
        fsm.SetCurrentState("Follow");
    }

    public void StartupFinished()
    {
        inInterruptFrames = false;
    }

    public void PauseStartupForSeconds(float seconds)
    {
        StartCoroutine(PauseStartup(seconds));
    }

    public IEnumerator PauseStartup(float seconds)
    {
        anim.speed = 0;

        yield return new WaitForSeconds(seconds);

        anim.speed = 1;
    }

    public void hitPause() {
        hitStopTimer = 0.1f;
        anim.speed = 0.0f;
    }

    public void Die()
    {

        Destroy(this.gameObject);
    }
    public void AlertEnemy()
    {
        //playerDetected = true;
        sightDistance = 200f;
        
        //anim.SetTrigger("Notice Player");
        //fsm.SetCurrentState("Follow");
    }
    #endregion

    #region Initializing states

    void Init_Idle()
    {
        EnemyState state = (EnemyState)fsm.GetState("Idle");

        state.OnEnterDelegate += delegate ()
        {
            ai.isStopped = true;
        };

        state.OnExitDelegate += delegate ()
        {

        };

        state.OnFixedUpdateDelegate += delegate ()
        {
            if(!playerDetected)
            {
                Physics.Raycast(transform.position + Vector3.up, (player.transform.position - transform.position).normalized, out RaycastHit hit, sightDistance, mask);

                Debug.DrawRay(transform.position + Vector3.up, (player.transform.position - transform.position).normalized * sightDistance);

                if (hit.collider != null && hit.collider.gameObject.CompareTag("Player"))
                {
                    playerDetected = true;
                    anim.SetTrigger("Notice Player");
                    fsm.SetCurrentState("Follow");
                }
            }
        };
    }

    void Init_Follow()
    {
        EnemyState state = (EnemyState)fsm.GetState("Follow");

        state.OnEnterDelegate += delegate ()
        {
            // Make sure that the AI is not stopped
            ai.isStopped = false;
            ai.enableRotation = true;
            inInterruptFrames = true;
        };

        state.OnExitDelegate += delegate ()
        {
            ai.endReachedDistance = 0.01f;
            ai.enableRotation = true;
        };

        state.OnUpdateDelegate += delegate ()
        {
            if(health <= 0)
            {
                fsm.SetCurrentState("Die");
            }

            target.transform.position = player.transform.position;

            if (!isCircling)
            {
                ai.maxSpeed = followMoveSpeed;
                ai.acceleration = followAcceleration;
                if (debuffTimer > 0) {
                    ai.maxSpeed = followMoveSpeed * 0.6f;
                    ai.acceleration = followAcceleration * 0.6f;
                }
                ai.enableRotation = true;
                ai.endReachedDistance = followDistance;

                if (ai.reachedEndOfPath)
                {
                    ai.isStopped = true;
                    var direction = (target.transform.position - transform.position).normalized;
                    Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
                }
                else
                {
                    ai.isStopped = false;
                }
            }
            else
            {
                ai.maxSpeed = circlingMoveSpeed;
                ai.acceleration = circlingAcceleration;
                
                if (debuffTimer > 0) {
                    ai.maxSpeed = circlingMoveSpeed * 0.6f;
                    ai.acceleration = circlingAcceleration * 0.6f;
                }
                ai.endReachedDistance = 0.01f;
                ai.enableRotation = false;

                var normal = (ai.position - ai.destination).normalized;
                var tangent = Vector3.Cross(normal, target.transform.up);

                // We can accomplish circling by getting the tangent of the vector to the player and offsetting it (for speed).
                if(isCirclingRight) ai.destination = ai.destination + normal * circleDistance + tangent * 4.5f;
                else ai.destination = ai.destination + normal * circleDistance + tangent * -4.5f;
                if(!ai.pathPending) ai.SearchPath();

                //Rotate to look at player
                var direction = (target.transform.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);

                if (ai.reachedEndOfPath)
                {
                    ai.isStopped = true;
                }
                else
                {
                    ai.isStopped = false;
                }
            }
        };
    }

    void Init_Hitstun()
    {
        EnemyState state = (EnemyState)fsm.GetState("Hitstun");

        state.OnEnterDelegate += delegate ()
        {
            enemyRigidbody.velocity = Vector3.zero;
            ai.isStopped = true;
            ai.enableRotation = false;
            isHitstunned = true;
            anim.SetTrigger("Hitstun");
        };

        state.OnExitDelegate += delegate ()
        {
            isHitstunned = false;
            ai.isStopped = false;
            ai.enableRotation = true;
            wasHitstunned = true;
        };
    }

    void Init_Die()
    {
        EnemyState state = (EnemyState)fsm.GetState("Die");

        state.OnEnterDelegate += delegate ()
        {
            enemyRigidbody.velocity = Vector3.zero;
            ai.isStopped = true;
            ai.enableRotation = false;
            anim.SetTrigger("Die");
        };
    }
    #endregion

    
}

public enum TypesPH { Alkaline, Neutral, Acidic }