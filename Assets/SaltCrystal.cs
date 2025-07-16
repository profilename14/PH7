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
    private int shardCount;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        currentHealth -= attack.attackData.damage;
        hitEffect.Play();

        if(currentHealth <= 0)
        {
            breakEffect.Play();
            Shatter(transform.position - hitPoint);
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

    public void Shatter(Vector3 targetDir)
    {
        Instantiate(crystalShardProjectile, transform.position, Quaternion.Euler(targetDir));
        gameObject.SetActive(false);
    }
}
