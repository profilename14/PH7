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

    [SerializeField]
    public CinemachineManager cinemachineManager;

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
        if (hit is Enemy)
        {
            Enemy enemy = (Enemy)hit;

            //playerStats.ModifyAcid(enemy.GetAcidOnHit());
            //playerStats.ModifyAlkaline(enemy.GetAlkalineOnHit());

            playerStats.ModifyAlkaline(0.6f);

            /*if (enemy.characterData.naturalType == Chemical.Alkaline)
            {
                playerStats.ModifyAlkaline(1.5f);
            }
            else if (enemy.currentDebuff == Chemical.Alkaline)
            {
                playerStats.ModifyAlkaline(1.25f);
            }
            else
            {
                playerStats.ModifyAlkaline(1f);
            }*/
        }
    }

    void FixedUpdate()
    {
        if (playerStats.alkaline <= 1.7f)
        {
            playerStats.ModifyAlkaline(0.33f * Time.deltaTime);
        }
    }

    public override void OnCharacterAttackHit(IHittable hit, MyProjectile attack, Vector3 hitPosition)
    {
        return;
    }

    public override void Die()
    {
        playerActionManager.UIManager.loadingScreen.fadeToBlackDoor();
        GameManager.instance.PlayerRespawn();
        _Stats.SetHealth(characterData.maxHealth);
    }

    public void WarpPlayer(Vector3 position)
    {
        PlayerMovementController mc = (PlayerMovementController)movementController;
        mc.TeleportTo(position);
    }
}
