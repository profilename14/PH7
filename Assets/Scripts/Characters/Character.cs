using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class Character : MonoBehaviour, IHittable
{
    [SerializeField]
    protected CharacterData _CharacterData;
    public CharacterData characterData => _CharacterData;

    // References to other core components and scripts.
    [SerializeField]
    protected CharacterActionManager _ActionManager;
    public CharacterActionManager actionManager => _ActionManager;

    public CharacterMovementController movementController;
    [SerializeField]
    protected CharacterStats _Stats;
    public CharacterStats stats => _Stats;
    [SerializeField]
    protected CharacterVFXManager _VFXManager;
    public CharacterVFXManager VFXManager => _VFXManager;

    [SerializeField]
    protected bool isInvincible = false;

    [SerializeField]
    protected bool isKnockbackImmune = false;

    [SerializeField]
    protected bool isHitstunImmune = false;

    protected bool isDead = false;

    protected void Awake()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref movementController);
        gameObject.GetComponentInParentOrChildren(ref _Stats);
        gameObject.GetComponentInParentOrChildren(ref _VFXManager);
    }

    // Should be called when a hit should result in something different depending on what you hit.
    // For example, the Player hitting an Enemy results in gaining pH, while hitting something else might not.
    public abstract void OnCharacterAttackHit(IHittable hit, AttackState attack, Vector3 hitPosition);

    public abstract void OnCharacterAttackHit(IHittable hit, MyProjectile attack, Vector3 hitPosition);

    // Should be called whenever this Character is hit by an attack.
    //Hit(AttackData, Character)
    //Hit(AttackData, Vector3)

    
    public virtual void Hit(AttackState attack, Vector3 hitPoint)
    {
        //Debug.Log("Hitbox collided");

        if (movementController == null) gameObject.GetComponentInParentOrChildren(ref movementController);

        if (!isInvincible)
        {
            _Stats.TakeDamage(attack.attackData.damage);

            if(isDead)
            {
                _VFXManager.DeathVFX();
                return;
            }
            else
            {
                //Debug.Log(attack.character);
                //Debug.Log(attack);
                _VFXManager.TookDamageVFX(hitPoint, attack.character.transform.position);
            }
        }

        if (!isKnockbackImmune && characterData.knockbackResistance != 0)
        {
            Vector3 knockbackDir = new Vector3(-(hitPoint.x - transform.position.x), 0, -(hitPoint.z - transform.position.z));
            movementController.ApplyImpulseForce(knockbackDir, attack.attackData.knockback / characterData.knockbackResistance);
        }
        
        if(!isHitstunImmune) actionManager.Hitstun();
    }

    public virtual void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        if (movementController == null) gameObject.GetComponentInParentOrChildren(ref movementController);

        if (!isInvincible)
        {
            _Stats.TakeDamage(projectile.attackData.damage);

            if (isDead)
            {
                _VFXManager.DeathVFX();
                return;
            }
            else
            {
                _VFXManager.TookDamageVFX(hitPoint, projectile.transform.position);
            }
            if(!isHitstunImmune) actionManager.Hitstun();
        }

        if (!isKnockbackImmune && characterData.knockbackResistance != 0)
        {
            Vector3 knockbackDir = -(projectile.transform.position - transform.position);
            if (movementController.IsGrounded())
            {
                movementController.ApplyImpulseForce(knockbackDir, projectile.attackData.knockback / characterData.knockbackResistance);
            }
            else 
            {
                // Lessen frictionless knockback (we may have to find a better fraction later).
                movementController.ApplyImpulseForce(knockbackDir, projectile.attackData.knockback / (2.5f * characterData.knockbackResistance));
            }
            
        }

    }

    public virtual void Hit(ColliderEffectField effectField, float damage)
    {
        Debug.Log("Hit by effect field");
        if (movementController == null) gameObject.GetComponentInParentOrChildren(ref movementController);

        if (!isInvincible)
        {
            _Stats.TakeDamage(damage);
            
            if (isDead)
            {
                _VFXManager.DeathVFX();
                return;
            }

            if(effectField.causeHit)
            {
                _VFXManager.TookDamageVFX(this.transform.position, effectField.transform.position);
                if (!isHitstunImmune) actionManager.Hitstun();
            }
        }

        if (!isKnockbackImmune && characterData.knockbackResistance != 0)
        {
            Vector3 knockbackDir = -(effectField.transform.position - transform.position);
            if (movementController.IsGrounded())
            {
                movementController.ApplyImpulseForce(knockbackDir, effectField.dynamicKnockback / characterData.knockbackResistance);
                movementController.ApplyImpulseForce(effectField.staticKnockback, effectField.staticKnockback.magnitude / characterData.knockbackResistance);
            }
            else
            {
                // Lessen frictionless knockback (we may have to find a better fraction later).
                movementController.ApplyImpulseForce(knockbackDir, effectField.dynamicKnockback / (2.5f * characterData.knockbackResistance));
                movementController.ApplyImpulseForce(effectField.staticKnockback, effectField.staticKnockback.magnitude / (2.5f * characterData.knockbackResistance));
            }

        }

    }

    public virtual void SetIsInvincible(bool isInvincible)
    {
        this.isInvincible = isInvincible;
    }

    public virtual void SetIsKnockbackImmune(bool isKnockbackImmune)
    {
        this.isKnockbackImmune = isKnockbackImmune;
    }

    public virtual void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
    }

/*#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _MovementController);
        gameObject.GetComponentInParentOrChildren(ref _Stats);
        gameObject.GetComponentInParentOrChildren(ref _VFXManager);
    }
#endif*/
}
