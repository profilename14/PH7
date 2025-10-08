using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class Enemy : Character
{
    EnemyData enemyData;

    private bool isLockedOn;

    public delegate void OnDeathDelegate();

    public OnDeathDelegate onDeath;

    private void Awake()
    {
        enemyData = (EnemyData) characterData;
    }

    public override void Hit(AttackState attack, Vector3 hitPoint)
    {
        //Debug.Log(this.gameObject.name + " took " + attack.attackData.damage + " damage from " + attack.name + "! Has " + _Stats.health + " health left!");
        base.Hit(attack, hitPoint);

        if(_Stats.health <= 0) { return; }

        RoamingEnemyActionManager r = gameObject.GetComponentInParentOrChildren<RoamingEnemyActionManager>();
        if (r != null && !r.isAggro) r.SpottedPlayer();
    }

    public override void Hit(ColliderEffectField effectField, float damage)
    {
        base.Hit(effectField, damage);
       // Debug.Log(this.gameObject.name + " took " + effectField.damageOnEnter + " damage from " + effectField.name + "! Has " + _Stats.health + " health left!");
    }

    public override void OnCharacterAttackHit(IHittable hit, AttackState attack, Vector3 hitPosition)
    {
        if(hit is Player)
        {
            //Debug.Log("Enemy hit player!");
        }
    }

    public override void OnCharacterAttackHit(IHittable hit, MyProjectile attack, Vector3 hitPosition)
    {
        return;
    }

    public double GetAlkalineOnHit()
    {
        return enemyData.alkalineOnHit;
    }

    public double GetAcidOnHit()
    {
        return enemyData.acidOnHit;
    }

    public void LockOn(OnDeathDelegate disableLockOn)
    {
        isLockedOn = true;
        onDeath += disableLockOn;
    }

    public void DisableLockOn()
    {
        isLockedOn = false;
        onDeath = null;
    }

    public override void Die()
    {
        isDead = true;
        if (isLockedOn) onDeath.Invoke();
        EnemyActionManager am = (EnemyActionManager)actionManager;
        am.OnDeath();
        gameObject.SetActive(false);
    }

    public float GetBounciness()
    {
        return enemyData.bouncinessMod;
    }
}
