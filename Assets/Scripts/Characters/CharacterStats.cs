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

    private Chemical _NaturalType;
    public Chemical naturalType => _NaturalType;

    bool isInvincible;

    protected virtual void Awake()
    {
        _Health = _Character.characterData.maxHealth;
        _NaturalType = _Character.characterData.naturalType;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isInvincible) return;

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

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _Character);
    }
#endif
}
