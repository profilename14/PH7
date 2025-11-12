using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyProjectile : MonoBehaviour
{
    protected Character _Sender;
    public Character sender => _Sender;

    protected AttackData _AttackData;
    public AttackData attackData => _AttackData;

    public bool projectileIsActive = false;

    [SerializeField] private float lifespan = 4.5f;
    private float lifespanTimer = 0f;
    [SerializeField] private float forwardSpeed = 50f;

    [SerializeField] private float fallSpeed;

    [SerializeField] private float initialForce;

    [SerializeField] private UnityEvent projectileDestroyEvent;

    [SerializeField] private float registerCollisionsDelay;

    [SerializeField] private Rigidbody rb;

    [SerializeField] private bool ignoreOtherProjectiles;

    [SerializeField] public bool triggerDebuff = true;
    
    [SerializeField] public bool triggerReactions = true;

    [SerializeField] LayerMask projectileCollMask;

    void Update()
    {
        if (projectileIsActive)
        {
            lifespanTimer += Time.deltaTime;
            if (lifespanTimer > lifespan)
            {
                gameObject.SetActive(false);
            }
            transform.position += this.transform.forward * Time.deltaTime * forwardSpeed;
        }
    }

    private void FixedUpdate()
    {
        if(fallSpeed > 0) rb.velocity -= new Vector3(0, fallSpeed, 0);
    }

    public void InitProjectile(Vector3 position, Vector3 rotation, Character sender, AttackData data)
    {
        transform.position = position;
        transform.eulerAngles = rotation;
        this._Sender = sender;
        this._AttackData = data;
        Invoke("SetProjectileActive", registerCollisionsDelay);
        OnProjectileActivate();
    }

    public void InitProjectile(Vector3 position, Quaternion rotation, Character sender, AttackData data)
    {
        transform.position = position;
        transform.rotation = rotation;
        this._Sender = sender;
        this._AttackData = data;
        Invoke("SetProjectileActive", registerCollisionsDelay);
        OnProjectileActivate();
    }

    public virtual void OnProjectileActivate()
    {
        //Debug.Log("Projectile activate");
        rb.AddForce(transform.forward * initialForce, ForceMode.Impulse);
        return;
    }

    public void SetProjectileActive()
    {
        projectileIsActive = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!projectileIsActive) return;

        // If this script is disabled, then the player is not in this attack state and nothing should happen.
        if (this.enabled == false) return;

        if (other.CompareTag("Trigger")) return;

        if (ignoreOtherProjectiles && other.CompareTag("Projectile")) return;

        if ((projectileCollMask & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }
        else
        {
            projectileDestroyEvent?.Invoke();
            gameObject.SetActive(false);
        }

        Debug.Log("Projectile hit layer " + other.gameObject.layer);

        // Check if we have collided with a hittable object.
        IHittable hittableScript = other.gameObject.GetComponentInParentOrChildren<IHittable>();
        if (hittableScript != null)
        {
            // In the case of the player, you are hitting your own hitbox.
            // In the case of an Enemy, they are either hitting their own hitbox, or a hitbox of an ally Enemy.
            if (_Sender.GetType() == hittableScript.GetType()) return;

            Vector3 attackHitPosition = other.ClosestPointOnBounds(transform.position);

            hittableScript.Hit(this, attackHitPosition);
            OnAttackHit(attackHitPosition, other);
            sender.OnCharacterAttackHit(hittableScript, this, attackHitPosition);

            projectileDestroyEvent?.Invoke();
            gameObject.SetActive(false);
        }

        //Debug.Log(other);

        
    }

    protected virtual void OnAttackHit(Vector3 position, Collider other)
    {
        return;
    }
}
