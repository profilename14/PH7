using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterStats : MonoBehaviour
{
    [SerializeField]
    private float _Health;
    public float health => _Health;

    //If I want any methods without a default implementation, then declare an abstract method

    // These methods will be overwritten
    protected virtual void TakeDamage(float damage)
    {
        _Health -= damage;

        if (_Health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(this.gameObject);
    }
}
