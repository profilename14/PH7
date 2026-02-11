using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
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

    public Character sender = null;

    bool senderIsPlayer;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        currentHealth -= attack.attackData.damage;
        hitEffect.Play();

        if (sender == null)
        {
            sender = attack.character;
        }

        if (currentHealth <= 0)
        {
            breakEffect.Play();
            if (attack is PlayerSwordAttack)
            {
                PlayerSwordAttack swordScript = (PlayerSwordAttack)attack;

                senderIsPlayer = true;

                Shatter(swordScript.GetSaltShatterDirection(), sender);
            }
            else if (attack is PlayerChargeAttack)
            {
                PlayerChargeAttack swordScript = (PlayerChargeAttack)attack;

                Shatter(swordScript.GetAttackingDirection(), sender);
            }
            else if(attack.character is Enemy)
            {
                Vector3 dir = (Player.instance.transform.position - this.transform.position);
                dir.y = 0;
                Shatter(dir.normalized, sender);
            }
            else
            {
                Shatter(transform.position - hitPoint, sender);
            }
        }
    }

    public void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        currentHealth -= projectile.attackData.damage;

        if (currentHealth <= 0)
        {
            breakEffect.Play();

            Vector3 dir = transform.position - hitPoint;
            dir.y = 0;

            Shatter(dir.normalized, sender);
        }
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        return;
    }

    public void Shatter(Vector3 targetDir, Character sender)
    {
        crystalObject.SetActive(false);
        hitTrigger.enabled = false;

        for (int i = 0; i < shardCount; i++)
        {
            float angleOffset = 0;
            if (i > 0)
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

    public void SetSender(Character newSender)
    {
        sender = newSender;
    }
    
    public float GetHealth()
    {
        return currentHealth;
    }
    public void SetHealth(float amount)
    {
        currentHealth = amount;
    }
}
