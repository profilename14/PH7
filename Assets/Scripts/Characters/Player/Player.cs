using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class Player : Character
{
    // Player singleton
    public static Player instance;

    public PlayerStats playerStats;

    public PlayerActionManager playerActionManager;

    private void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(this);
        }

        playerStats = (PlayerStats)stats;
        playerActionManager = (PlayerActionManager)actionManager;
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

    public override void Die()
    {
        GameManager.instance.PlayerRespawn();
        _Stats.SetHealth(characterData.maxHealth);
    }

    public void WarpPlayer(Vector3 position)
    {
        PlayerMovementController mc = (PlayerMovementController)movementController;
        mc.TeleportTo(position);
    }
}
