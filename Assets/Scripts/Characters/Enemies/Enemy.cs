using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public override void OnCharacterAttackHit(IHittable hit)
    {
        if(hit is Player)
        {
            Debug.Log("Enemy hit player!");
        }
    }
}
