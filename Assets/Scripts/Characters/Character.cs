using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;
using Unity.VisualScripting;

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
    [SerializeField]
    protected bool isReactionImmune = false;
    [SerializeField]
    public int reactionResistance = 1; // 1-3, the num reactions you have to trigger to affect it

    [SerializeField]
    private ColliderEffectField currentPuddle = null; // Can read currentPuddle.effectType

    protected bool isDead = false;

    [SerializeField]
    private UnityEvent OnHitByAttack;

    public Chemical currentDebuff = Chemical.None;
    public bool isFrozen = false;
    public int freezeReactionsTriggered = 0;
    public float freezeSubReactionsTriggered = 0; // 0 to 1

    [SerializeField]
    private UnityEvent onexitPuddle;

    GameObject reactionUI;

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
        //print("IsInvincible:" + isInvincible);
        //Debug.Log("Hitbox collided");

        if (movementController == null) gameObject.GetComponentInParentOrChildren(ref movementController);

        

        if (!isInvincible)
        {
            if (isFrozen)
            {
                _Stats.TakeDamage(attack.attackData.damage * 1.5f);
            }
            else
            {
                _Stats.TakeDamage(attack.attackData.damage);
            }
            


            OnHitByAttack.Invoke();

            if (isDead)
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

        if (!isReactionImmune && characterData.naturalType == projectile.attackData.type)
        {
            // Ex Acid Applied to dry or acidfied vitriclaw = acified and no damage
            if (projectile.triggerDebuff == true && (currentDebuff == characterData.naturalType || currentDebuff == Chemical.None) )
            {
                if (!isReactionImmune)
                {
                    currentDebuff = projectile.attackData.type;
                }
                return;
            }
            else
            {
                // Ex Acid applied to vitriclaw that has a different debuff. Do nothing if this hit doesn't deal reactions
                if (!projectile.triggerReactions || isReactionImmune)
                {
                    return;
                }
            }
        }

        if (!isInvincible)
        {
            _Stats.TakeDamage(projectile.attackData.damage);

            OnHitByAttack.Invoke();

            if (isDead)
            {
                _VFXManager.DeathVFX();
                return;
            }
            else
            {
                _VFXManager.TookDamageVFX(hitPoint, projectile.transform.position);
            }

            if (!isReactionImmune) ChemicalReaction(projectile.attackData.type, projectile.triggerReactions, projectile.triggerDebuff, 1);
            
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
        //Debug.Log("Hit by effect field");
        if (movementController == null) gameObject.GetComponentInParentOrChildren(ref movementController);

        if (characterData.naturalType == effectField.effectType)
        {
            // Ex Acid Applied to dry or acidfied vitriclaw = acified and no damage
            if (effectField.triggerDebuff == true && (currentDebuff == characterData.naturalType || currentDebuff == Chemical.None))
            {
                currentDebuff = effectField.effectType;
                return;
            }
            else
            {
                // Ex Acid applied to vitriclaw that has a different debuff. Do nothing if this hit doesn't deal reactions
                // If it DOES, then continue and deal damage anyway even though its the same element
                if (!effectField.triggerReactions)
                {
                    return;
                }
            }
        }

        if (!isInvincible && damage != 0)
        {
            _Stats.TakeDamage(damage);

            OnHitByAttack.Invoke();

            if (isDead)
            {
                _VFXManager.DeathVFX();
                return;
            }

            if (effectField.causeHit)
            {
                _VFXManager.TookDamageVFX(this.transform.position, effectField.transform.position);
                if (!isHitstunImmune) actionManager.Hitstun();
            }



        }

        ChemicalReaction(effectField.effectType, effectField.triggerReactions, effectField.triggerDebuff, effectField.reactionPower);

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

    public virtual void SetIsHitstunImmune(bool isHitstunImmune)
    {
        this.isHitstunImmune = isHitstunImmune;
    }

    public virtual void SetCurrentPuddle(ColliderEffectField newPuddle)
    {
        if (newPuddle == null) onexitPuddle.Invoke();

        this.currentPuddle = newPuddle; // set to null on leaving a puddle
        if (currentPuddle != null)
        {
            //Debug.Log(currentPuddle.effectType);
        }
    }

    public void RemoveCurrentPuddle(ColliderEffectField puddle)
    {
        if(puddle == this.currentPuddle)
        {
            currentPuddle = null;
        }
    }

    public ColliderEffectField getCurrentPuddle()
    {
        return currentPuddle;
    }

    public virtual void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
    }

    protected void ChemicalReaction(Chemical attackingChemical, bool triggerReactions, bool triggerDebuff, float reactionPower)
    {
        if (triggerReactions && !isReactionImmune)
        {
            if (currentDebuff == Chemical.None || currentDebuff == attackingChemical)
            {
                // Ex Acid against a dry or acidified strider = freeze
                if (characterData.naturalType != attackingChemical)
                {
                    Debug.Log("Triggered Reaction: " + characterData.naturalType + " " + attackingChemical);
                    StartCoroutine(ChemicalReactionFreeze(reactionPower));
                }
            }
            else
            {
                // Ex Alkaline against an acidified strider = freeze
                if (currentDebuff != attackingChemical)
                {
                    Debug.Log("Triggered Reaction: " + currentDebuff + " " + attackingChemical);
                    StartCoroutine(ChemicalReactionFreeze(reactionPower));
                }
            }
        }

        if (triggerDebuff)
        {
            if (currentDebuff == Chemical.None)
            {
                currentDebuff = attackingChemical;
            }
            else
            {
                // Leave target dry if a reaction occured, if this cant do reactions replace debuff
                if (currentDebuff != attackingChemical)
                {
                    currentDebuff = Chemical.None;
                    
                    if (!triggerReactions)
                    {
                        currentDebuff = attackingChemical;
                    }
                }
            }
            
        }
    }

    public IEnumerator ChemicalReactionFreeze(float reactionPower)
    {
        if (isFrozen) {
            yield break;
        }


        if (reactionPower < 1)
        {
            freezeSubReactionsTriggered += reactionPower;
            if (freezeSubReactionsTriggered >= 1)
            {
                freezeSubReactionsTriggered--;
                freezeReactionsTriggered++;
            }
        }
        else
        {
            freezeReactionsTriggered += (int)reactionPower;
        }

        if (!reactionUI)
        {
            reactionUI = Instantiate(Resources.Load<GameObject>("CharacterReactionUI"), gameObject.transform);
        }

        if (freezeReactionsTriggered < reactionResistance)
        {
            yield break;
        }
        else
        {
            freezeReactionsTriggered = 0;
            freezeSubReactionsTriggered = 0;
            // Continue...
        }
        
        
        
        ChemicalReactionFreezeStart();

        yield return new WaitForSeconds(4 + (reactionResistance-1)); // 4-6 seconds

        ChemicalReactionFreezeEnd();
    }

    protected virtual void ChemicalReactionFreezeStart()
    {
        if (isFrozen) return;
        isFrozen = true;
        _ActionManager.Hitstun();
        _ActionManager.anim.Graph.PauseGraph();

        movementController.SetVelocity(Vector3.zero);
        movementController.SetAllowRotation(false);
        isKnockbackImmune = true;
    }
    
    protected virtual void ChemicalReactionFreezeEnd()
    {
        if (!isFrozen) return;
        isFrozen = false;
        _ActionManager.anim.Graph.UnpauseGraph();
        _ActionManager.EndHitStun();
        isKnockbackImmune = false;
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
