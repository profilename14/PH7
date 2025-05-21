using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class MyProjectile : MonoBehaviour
{
    protected Character _Sender;
    public Character sender => _Sender;

    protected AttackData _AttackData;
    public AttackData attackData => _AttackData;

    public bool projectileIsActive = false;

    [SerializeField] private float lifespan = 4.5f;
    private float lifespanTimer = 0f;
    [SerializeField] private float speed = 50f;

    void Update()
    {
        lifespanTimer += Time.deltaTime;
        if (lifespanTimer > lifespan) {
            gameObject.SetActive(false);
        }
        transform.position += this.transform.forward * Time.deltaTime * speed;
    }

    public void InitProjectile(Vector3 position, Vector3 rotation, Character sender, AttackData data)
    {
        transform.position = position;
        transform.eulerAngles = rotation;
        this._Sender = sender;
        this._AttackData = data;
        projectileIsActive = true;
        OnProjectileActivate();
    }

    public void InitProjectile(Vector3 position, Quaternion rotation, Character sender, AttackData data)
    {
        transform.position = position;
        transform.rotation = rotation;
        this._Sender = sender;
        this._AttackData = data;
        projectileIsActive = true;
        OnProjectileActivate();
    }

    public virtual void OnProjectileActivate()
    {
        return;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!projectileIsActive) return;

        // If this script is disabled, then the player is not in this attack state and nothing should happen.
        if (this.enabled == false) return;

        if (other.CompareTag("Trigger")) return;

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
        }
        
        gameObject.SetActive(false);
    }

    protected virtual void OnAttackHit(Vector3 position, Collider other)
    {
        return;
    }
}
