using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class Player : Character
{
    // Player singleton
    public static Player instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    public override void OnCharacterAttackHit(IHittable hit, AttackState attack)
    {
        attack.OnAttackHit();

        if(hit is Enemy)
        {
            Debug.Log("Player hit enemy!");
            // _Stats.GainPH()
        }
    }
}
