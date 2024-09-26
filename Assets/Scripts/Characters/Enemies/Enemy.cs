using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public override void OnCharacterAttackHit(IHittable hit, AttackState attack)
    {
        if(hit is Player)
        {
            Debug.Log("Enemy hit player!");
        }
    }
}
