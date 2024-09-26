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

    // Should handle VFX or in case of the player, gaining pH.
    // Should maybe have some way to get the point of contact for hit VFX (this may be complicated to do with OnTriggerEnter though).
    public abstract void OnCharacterAttackHit(IHittable hit, AttackState attack);

    public virtual void Hit(AttackData attackData)
    {
        _Stats.TakeDamage(attackData.damage);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _MovementController);
        gameObject.GetComponentInParentOrChildren(ref _Stats);
    }
#endif
}
