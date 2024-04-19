using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using UnityEditor;

using Patterns;

public class EnemyAI : MonoBehaviour
{
    #region Base Enemy Data

    public FSM fsm;
    public RichAI ai;
    public Animator anim;

    protected GameObject player;

    [Header("Detection")]
    public bool playerDetected = false;
    public float sightDistance = 20f;

    [Header("Follow State")]
    public float followDistance = 1f;
    public float followMoveSpeed = 5;
    public float followAcceleration = 5;

    protected GameObject target;
    private LayerMask mask;

    public string currentState;
    // This is stored in the enemyAI class for now. We can move it in the future.
    protected bool isCircling = true;
    private float circleDistance = 1.25f;
    [SerializeField] private GameObject rotationAnchor;
    [SerializeField] private Rigidbody enemyRigidbody;



    // This is more hp related stuff to handle the global enemy armor system.
    [SerializeField] public TypesPH naturalPH;
    [SerializeField] protected float maxArmorRatio = 0.5f; // The maximum percent of the health bar with armor can be fine tuned between enemies.
    // Ex, striders could have 20% armor, making them quite weak but always regen to the same point until almost defeated.
    [SerializeField] protected float maxHealth = 100f;
    [HideInInspector] public float health;
    [HideInInspector] public float armor;
    [HideInInspector] public bool armorBroken;

    
    private float armorRegenTimer = 0.0f;
    [SerializeField] private float regenArmorStartSpeed = 3.0f; // Time in seconds until armor regens. Lower = faster.
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip enemyArmoredImpactSound;
    [SerializeField] private AudioClip enemyImpactSound;
    [SerializeField] private AudioClip enemyPotHitSound;




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

        mask = LayerMask.GetMask("Exclude from A*", "BlocksVision");

        target = new GameObject(this.gameObject.name + " AI Target");
        var iconContent = EditorGUIUtility.IconContent("sv_label_1");
        EditorGUIUtility.SetIconForObject(target, (Texture2D)iconContent.image);

        health = maxHealth;
        armor = maxArmorRatio * health;
    }

    // Update is called once per frame
    public void Update()
    {
        currentState = fsm.GetCurrentState();
        checkHealth();

        setDestination();

        fsm.Update();
    }

    private void setDestination()
    {
        // Make sure the AI is always trying to move towards our target object
        ai.destination = target.transform.position;

        fsm.Update();
        // Code for visually rotating the enemy towards the player when moving.
        rotationAnchor.transform.rotation = Quaternion.LookRotation(
            Vector3.RotateTowards(rotationAnchor.transform.forward, ai.destination - transform.position, 0.025f, 0) );

        if (isCircling) { // Credits to arongranberg.com for the circling tutorial.
            var normal = (ai.position - ai.destination).normalized;
            var tangent = Vector3.Cross(normal, target.transform.up);

            // We can accomplish circling by getting the tangent of the vector to the player and offsetting it (for speed).
            ai.destination = ai.destination + normal * circleDistance + tangent * 4.5f;
        }
    }

    private void checkHealth() {
        if (armor < maxArmorRatio * health) {
            armorRegenTimer += Time.deltaTime;

        } else if (armor >= maxArmorRatio * health) {
            armor = maxArmorRatio * health;
            armorRegenTimer = 0.0f;
        }

        if (armorRegenTimer > regenArmorStartSpeed) {
            armor += Time.deltaTime * (maxArmorRatio * health / 5.0f); // It takes 5 seconds of regen to fully recover armor.
            armorBroken = false;
        }

        if (health < 0) {
            Destroy(gameObject);
        }
    }

    public void FixedUpdate()
    {
        fsm.FixedUpdate();
    }

    public virtual void TakeDamage(float damage, float ph, float knockback, Vector3 sourcePos)
    {
        float displayedDamage = 0;
        if (!armorBroken) {
            if (ph != 0) {

                if ( (ph < 0 && naturalPH == TypesPH.Alkaline) || (ph > 0 && naturalPH == TypesPH.Acidic) ) {
                    armor -= Mathf.Abs(ph);
                    if (armor <= 0) {
                        armor = 0;
                        armorBroken = true;
                    }
                    displayedDamage = Mathf.Abs(ph);
                } else if (naturalPH == TypesPH.Neutral) { // What to do is tbd when the enemy's neutral

                } else { // The wrong element must have been used, so heal armor.
                    armor += Mathf.Abs(ph);
                    armorBroken = false;
                    displayedDamage = -Mathf.Abs(ph);
                }
                
            audioSource.PlayOneShot(enemyPotHitSound, 0.45F);
                
            } else if (damage > 0) {
                armor -= (damage / 3.0f);
                if (armor <= 0) {
                        armor = 0;
                        armorBroken = true;
                }
                displayedDamage = damage / 3.0f;
                audioSource.PlayOneShot(enemyArmoredImpactSound, 0.2F);
            }

            if (displayedDamage != 0) {
                Debug.Log("Damage: " + displayedDamage + " Against Armor");
            }

        } else {
            if (damage > 0) {
                health -= damage;
                displayedDamage = damage;
                
                audioSource.PlayOneShot(enemyImpactSound, 0.25F);
            } else {
                if ( (ph < 0 && naturalPH == TypesPH.Alkaline) || (ph > 0 && naturalPH == TypesPH.Acidic) ) {
                    health -= Mathf.Abs(ph / 3.0f);
                    displayedDamage = Mathf.Abs(ph / 3.0f);
                } else if (naturalPH == TypesPH.Neutral) { // What to do is tbd when the enemy's neutral

                } else { // The wrong element must have been used, so heal armor.
                    armor += Mathf.Abs(ph);
                    armorBroken = false;
                    displayedDamage = -Mathf.Abs(ph);
                }
                audioSource.PlayOneShot(enemyPotHitSound, 0.3F);
            }

            if (displayedDamage != 0) {
                Debug.Log("Damage: " + displayedDamage + " Against Health");
            }
        }

        if (damage > 0 || ph != 0) {
            armorRegenTimer = 0;
        }


        /*if (damage > 9.9) {
          Transform PopupTransform = Instantiate(PopupPrefab, transform.position, Quaternion.identity);
          DamagePopup popup = PopupTransform.GetComponent<DamagePopup>();
          popup.Setup(displayedDamage);
        }*/

        Vector3 dir = -((sourcePos - transform.position).normalized);
        Vector3 velocity = dir * knockback;
        enemyRigidbody.AddForce(velocity, ForceMode.Impulse);

        if (health <= 0) Destroy(this.gameObject);

    }

    #region Initializing states

    void Init_Idle()
    {
        EnemyState state = (EnemyState)fsm.GetState("Idle");

        state.OnEnterDelegate += delegate ()
        {
            //Debug.Log("OnEnter - Idle");
            ai.isStopped = true;
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Idle");
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
            ai.maxSpeed = followMoveSpeed;
            ai.acceleration = followAcceleration;

            anim.SetBool("Walking", true);
            ai.endReachedDistance = followDistance;
            //Debug.Log("OnEnter - Follow");
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Follow");
            ai.endReachedDistance = 0.01f;
        };

        state.OnUpdateDelegate += delegate ()
        {
            //Debug.Log("OnUpdate - Follow");
            target.transform.position = player.transform.position;

            if(ai.reachedEndOfPath)
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
        };
    }

    void Init_Hitstun()
    {
        EnemyState state = (EnemyState)fsm.GetState("Hitstun");

        state.OnEnterDelegate += delegate ()
        {
            // Make sure that the AI is stopped
            ai.isStopped = true;

            //Debug.Log("OnEnter - Follow");
        };

        state.OnExitDelegate += delegate ()
        {
            //Debug.Log("OnExit - Follow");
        };

        state.OnUpdateDelegate += delegate ()
        {
            //Debug.Log("OnUpdate - Follow");

            target.transform.position = player.transform.position;
        };
    }

    void Init_Die()
    {

    }
    #endregion

    
}

public enum TypesPH { Alkaline, Neutral, Acidic }