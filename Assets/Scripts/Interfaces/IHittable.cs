using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittable
{
    public void Hit(AttackState attack, Vector3 hitPoint);
    public void Hit(MyProjectile projectile, Vector3 hitPoint);
    public void Hit(ColliderEffectField colliderEffectField, float damage);


    //public float GetBounciness();
}
