using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Destroy(this.gameObject);
    }
}
