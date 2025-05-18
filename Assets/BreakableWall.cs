using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour, IHittable
{
    [SerializeField] GameObject wallObject;
    [SerializeField] ParticleSystem breakEffect;
    [SerializeField] Collider coll;

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        wallObject.SetActive(false);
        breakEffect.Play();
        coll.enabled = false;
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
