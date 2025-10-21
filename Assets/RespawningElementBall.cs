using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class RespawningElementBall : MonoBehaviour, IHittable
{
    [SerializeField]
    private float maxHealth;

    [SerializeField]
    private float currentHealth;

    [SerializeField]
    private GameObject elementProjectile;

    [SerializeField]
    private Collider hitTrigger;

    [SerializeField]
    private AttackData projectileAttackData;

    [SerializeField]
    private Vector3 projectilePositionOffset;
    

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        currentHealth -= attack.attackData.damage;


        if (currentHealth <= 0)
        {
            if (attack is PlayerSwordAttack)
            {
                PlayerSwordAttack swordScript = (PlayerSwordAttack)attack;
                
                Shatter(swordScript.GetAttackingDirection(), attack.character);
            }
            else if (attack is PlayerChargeAttack)
            {
                PlayerChargeAttack swordScript = (PlayerChargeAttack)attack;

                Shatter(swordScript.GetAttackingDirection(), attack.character);
            }
            else
            {
                Shatter(transform.position - hitPoint, attack.character);
            }
        }
    }

    public void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        return;
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        return;
    }

    public void Shatter(Vector3 targetDir, Character sender)
    {
        hitTrigger.enabled = false;



        Vector3 offsetDir = new Vector3(targetDir.x, targetDir.y, targetDir.z);
        GameObject shard = Instantiate(elementProjectile, transform.position + projectilePositionOffset, Quaternion.Euler(offsetDir));
        shard.GetComponent<MyProjectile>().InitProjectile(transform.position + projectilePositionOffset, Quaternion.LookRotation(offsetDir, Vector3.up), sender, projectileAttackData);

        Destroy(gameObject);
    }

}
