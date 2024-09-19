using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackState : CharacterState
{
    // This state might be unnecessary, but later it may be useful for combat.

    [SerializeField]
    private float _Damage;
    public float damage => _Damage;

    // Animation events for the attack?
}