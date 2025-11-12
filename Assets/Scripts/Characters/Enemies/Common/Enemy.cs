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

    EnemyMovementController enemyMovementController;

    private float originalMaxSpeed = 0;

    private RigidbodyConstraints originalRestraints;
    protected GameObject SaltCrystalPrefab;
    [SerializeField]
    protected GameObject curSaltCrystal = null;
    protected SaltCrystal curSaltCrystalScript = null;


    private void Awake()
    {
        enemyData = (EnemyData)characterData;
        enemyMovementController = GetComponent<EnemyMovementController>();
        SaltCrystalPrefab = Resources.Load<GameObject>("Enemy Reaction Salt Crystal");
    }

    public override void Hit(AttackState attack, Vector3 hitPoint)
    {
        //Debug.Log(this.gameObject.name + " took " + attack.attackData.damage + " damage from " + attack.name + "! Has " + _Stats.health + " health left!");
        base.Hit(attack, hitPoint);

        if(curSaltCrystal)
        {
            // the reaction salt crystal lacks collision, and instead has the enemy collider redirect damage to it
            curSaltCrystalScript.Hit(attack, hitPoint); 

            if(_Stats.health <= 0)
            {
                while (curSaltCrystalScript.GetHealth() > 0)
                {
                    curSaltCrystalScript.Hit(attack, hitPoint);
                }
            }

            if (curSaltCrystalScript.GetHealth() <= 0)
            {
                ChemicalReactionFreezeEnd();
            }
        }

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
        if (curSaltCrystal) Destroy(curSaltCrystal);
        EnemyActionManager am = (EnemyActionManager)actionManager;
        am.OnDeath();
        gameObject.SetActive(false);
    }

    public float GetBounciness()
    {
        return enemyData.bouncinessMod;
    }

    protected override void ChemicalReactionFreezeStart()
    {
        if (curSaltCrystal == null)
        {
            curSaltCrystal = Instantiate(SaltCrystalPrefab, transform.position, Quaternion.identity);
            curSaltCrystalScript = curSaltCrystal.GetComponent<SaltCrystal>();
            curSaltCrystalScript.SetSender(this);
            ChemicalReactionManager.instance.ClearNearbyChemicals(transform.position);
            movementController.SetAllowRotation(false);
            if(movementController is EnemyMovementController e)
            {
                e.SetForceLookAtPlayer(false);
                e.SetForceManualRotation(false);
            }
        }
        else
        {
            return;
        }

        base.ChemicalReactionFreezeStart();

        originalMaxSpeed = enemyMovementController.pathfinding.maxSpeed;
        enemyMovementController.pathfinding.maxSpeed = 0;
        originalRestraints = enemyMovementController.rb.constraints;
        enemyMovementController.rb.constraints = RigidbodyConstraints.FreezePositionX |
                                                 RigidbodyConstraints.FreezePositionZ |
                                                 RigidbodyConstraints.FreezeRotation;

    }
    protected override void ChemicalReactionFreezeEnd()
    {
        if (curSaltCrystal == null)
        {
            return;
        }
        else
        {
            Destroy(curSaltCrystal);
        }

        base.ChemicalReactionFreezeEnd();
        
        enemyMovementController.pathfinding.maxSpeed = originalMaxSpeed;
        enemyMovementController.rb.constraints = originalRestraints;
    }
}
