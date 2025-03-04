using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public enum Chemical {None, Alkaline, Acidic, Salt, Ammonia, Oil };

public abstract class CharacterStats : MonoBehaviour
{
    [SerializeField]
    Character _Character;

    [SerializeField]
    CharacterActionManager _ActionManager;

    [SerializeField]
    private float _Health;
    public float health => _Health;
    
    [SerializeField]
    private float _Armor;
    public float armor => _Armor;

    private Chemical _NaturalType;
    public Chemical naturalType => _NaturalType;

    protected virtual void Awake()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _Character);
        _Health = _Character.characterData.maxHealth;
        _NaturalType = _Character.characterData.naturalType;
    }

    // We should update this to account for special armor types and damage types some time in the future.
    public virtual void TakeDamage(float damage) 
    {
        

        if (_Armor > 0)
        {
            _Armor -= damage;
            if (_Armor < 0)
            {
                _Health += _Armor; // Roll over damage (negative armor) to health
                _Armor = 0;
            }
        }
        else
        {
            _Health -= damage;
        }

        
        //Debug.Log("Damage event: new health: " + health + " and new armor: " + armor); // Delete on making healthbars
        
        if (_Health <= 0 && _Armor <= 0) // Enemies can survive if they have armor and no health.
        {
            _Character.Die();
        }

        
    }

    public virtual void SetHealth(float newHealth)
    {
        _Health = newHealth;

        if (_Health <= 0)
        {
            _Character.Die();
        }
    }

    public virtual void SetArmor(float newArmor)
    {
        _Armor = newArmor;

        if (_Armor <= 0)
        {
            _Armor = 0;
        }
    }

/*#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _Character);
    }
#endif*/
}
