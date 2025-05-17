using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public class Pogoable : MonoBehaviour, IHittable
{
    public bool isDestroyable = true;
    private int health = 1;
    public float bouncinessMod = 1;

    
    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (isDestroyable)
        {
            health -= 1;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        return; // Should a bubble pop if hit by projectile?
    }

    public virtual void Hit(ColliderEffectField effectField, float damage)
    {
        return; // Should a bubble pop if hit by effect field?
    }

    public virtual void Die()
    {
        Destroy(this.gameObject);
    }


}
