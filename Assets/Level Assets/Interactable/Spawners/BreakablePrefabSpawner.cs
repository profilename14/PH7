using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakablePrefabSpawner : MonoBehaviour, IHittable
{
    [SerializeField]
    bool useHealth;

    [SerializeField]
    bool destroyOnTrigger;

    [SerializeField]
    float health;

    [SerializeField]
    GameObject prefabToSpawn;

    [SerializeField]
    UnityEvent eventToTrigger;
    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (useHealth)
        {
            health -= attack.attackData.damage;
            if (health <= 0)
            {
                Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
                eventToTrigger.Invoke();

                if(destroyOnTrigger)
                {
                    Destroy(this.gameObject);
                }
            }
        }
        else
        {
            Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
            eventToTrigger.Invoke();
            if (destroyOnTrigger)
            {
                Destroy(this.gameObject);
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
}
