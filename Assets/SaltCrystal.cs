using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaltCrystal : MonoBehaviour, IHittable
{
    [SerializeField]
    private float maxHealth;

    [SerializeField]
    private float currentHealth;

    [SerializeField]
    private ParticleSystem hitEffect;

    [SerializeField]
    private ParticleSystem breakEffect;

    [SerializeField]
    private GameObject crystalShardProjectile;

    [SerializeField]
    private float shardAngleOffset;

    [SerializeField]
    private Vector3 shardPositionOffset;

    [SerializeField]
    private int shardCount;

    [SerializeField]
    private GameObject crystalObject;

    [SerializeField]
    private Collider hitTrigger;

    [SerializeField]
    private AttackData shardAttackData;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        currentHealth -= attack.attackData.damage;
        hitEffect.Play();

        if (currentHealth <= 0)
        {
            breakEffect.Play();
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
        throw new System.NotImplementedException();
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        throw new System.NotImplementedException();
    }

    public void Shatter(Vector3 targetDir, Character sender)
    {
        crystalObject.SetActive(false);
        hitTrigger.enabled = false;

        for (int i = 0; i < shardCount; i++)
        {
            float angleOffset = 0;
            if(i > 0)
            {
                if (i % 2 == 0) angleOffset = i / 2 * shardAngleOffset;
                else if (i == 1) angleOffset = -shardAngleOffset;
                else angleOffset = -(i - 2) * shardAngleOffset;
            }

            Vector3 offsetDir = new Vector3(targetDir.x, targetDir.y, targetDir.z);
            GameObject shard = Instantiate(crystalShardProjectile, transform.position + shardPositionOffset, Quaternion.Euler(offsetDir));
            shard.GetComponent<MyProjectile>().InitProjectile(transform.position + shardPositionOffset, Quaternion.LookRotation(offsetDir, Vector3.up) * Quaternion.Euler(0, angleOffset, 0), sender, shardAttackData);
        }
    }
}
