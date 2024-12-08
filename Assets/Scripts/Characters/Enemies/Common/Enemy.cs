using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public override void OnCharacterAttackHit(IHittable hit, AttackState attack, Vector3 hitPosition)
    {
        if(hit is Player)
        {
            Debug.Log("Enemy hit player!");
        }
    }

    public override void OnCharacterAttackHit(IHittable hit, MyProjectile attack, Vector3 hitPosition)
    {
        return;
    }
}
