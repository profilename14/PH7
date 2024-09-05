using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackState : CharacterState
{
    // This state allows for easy reuse of attacks.
    // If a Character is enters the trigger of an attack hitbox (must be tagged with "EnemyAttack" or "PlayerAttack")

    [SerializeField]
    private float _Damage;
    public float damage => _Damage;

    // Animation events for the attack
}