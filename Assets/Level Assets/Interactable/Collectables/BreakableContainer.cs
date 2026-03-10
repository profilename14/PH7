using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakableContainer : MonoBehaviour, IHittable
{
    [SerializeField] GameObject containerObject;
    [SerializeField] ParticleSystem breakEffect;
    [SerializeField] Collider coll;
    [SerializeField] int health = 3;

    bool isBroken = false;

    [SerializeField]
    UnityEvent onHit;

    [SerializeField]
    UnityEvent onBreak;

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (isBroken) return;

        breakEffect.Play();

        health--;

        if (health > 0)
        {
            onHit.Invoke();
            return;
        }

        onBreak.Invoke();

        containerObject.SetActive(false);
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
