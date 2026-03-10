using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour, IHittable
{
    [SerializeField] GameObject wallObject;
    [SerializeField] ParticleSystem breakEffect;
    [SerializeField] Collider coll;
    [SerializeField] int health = 3;

    bool isBroken = false;

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (isBroken) return;
        
        breakEffect.Play();

        health--;
        if (health > 0) return;

        wallObject.SetActive(false);
        coll.enabled = false;
        isBroken = true;
    }

    public void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        return;
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        return;
    }
}
