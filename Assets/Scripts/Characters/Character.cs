using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class Character : MonoBehaviour, IHittable
{
    [SerializeField]
    protected BaseCharacterData _CharacterData;
    public BaseCharacterData characterData => _CharacterData;

    // References to other core components and scripts.
    [SerializeField]
    protected CharacterActionManager _ActionManager;
    public CharacterActionManager actionManager => _ActionManager;
    [SerializeField]
    protected ICharacterMovementController _MovementController;
    public ICharacterMovementController movementController => _MovementController;
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

    private bool isDead = false;

    // Should be called when a hit should result in something different depending on what you hit.
    // For example, the Player hitting an Enemy results in gaining pH, while hitting something else might not.
    public abstract void OnCharacterAttackHit(IHittable hit, AttackState attack, Vector3 hitPosition);

    // Should be called whenever this Character is hit by an attack.
    public virtual void Hit(AttackState attack, Vector3 hitPoint)
    {
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
                _VFXManager.TookDamageVFX(hitPoint, attack.character.transform.position);
            }
        }

        if (!isKnockbackImmune)
        {
            Vector3 knockbackDir = -(attack.character.transform.position - transform.position);
            movementController.ApplyImpulseForce(knockbackDir, attack.attackData.knockback);
        }
        
        actionManager.Hitstun();
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
        Destroy(this.gameObject);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _MovementController);
        gameObject.GetComponentInParentOrChildren(ref _Stats);
        gameObject.GetComponentInParentOrChildren(ref _VFXManager);
    }
#endif
}
