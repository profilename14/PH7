using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    // This script is temporary and should be integrated into the enemy behavior script and player combat script at some point
    public float damage;
    public float knockback = 2.5f;
    public float phChange = 0f;
    public float attackPH;
    private PlayerStats playerStatsScript;
    private EnemyBehavior enemyScript;

    private void Awake()
    {
        enemyScript = GetComponentInParent<EnemyBehavior>();
        attackPH = enemyScript.StartPH;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !playerStatsScript.isInvincible)
        {
            playerStatsScript.playerDamage(damage * enemyScript.neutralizationFactor, attackPH, phChange, gameObject.transform.position, knockback);
        }
    }

    public void SetPlayerStatsRef(PlayerStats script)
    {
        playerStatsScript = script;
    }
}
