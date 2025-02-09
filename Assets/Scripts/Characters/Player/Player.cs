using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class Player : Character
{
    // Player singleton
    public static Player instance;

    private PlayerStats playerStats;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);

        playerStats = (PlayerStats)stats;
    }

    public override void OnCharacterAttackHit(IHittable hit, AttackState attack, Vector3 hitPosition)
    {
        if(hit is Enemy)
        {
            Enemy enemy = (Enemy)hit;

            playerStats.ModifyAcid(enemy.GetAcidOnHit());
            playerStats.ModifyAlkaline(enemy.GetAlkalineOnHit());
        }
    }

    public override void OnCharacterAttackHit(IHittable hit, MyProjectile attack, Vector3 hitPosition)
    {
        return;
    }
}
