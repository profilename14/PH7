using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

[System.Serializable]
public enum CharacterActionPriority { Move, Low, Medium, High, Hitstun};

public abstract class CharacterActionManager : MonoBehaviour
{
    [Header("Base Action Manager Data")]
    [SerializeField]
    public AnimancerComponent anim;

    [SerializeField]
    public Character character;

    [SerializeField]
    protected CharacterState defaultState;

    public readonly StateMachine<CharacterState>.WithDefault StateMachine = new();

    // A set of different priorities that generic actions can fall under.
    public Dictionary<CharacterActionPriority, bool> allowedActionPriorities = new();

    // A set of different states that specific actions can fall under.
    public Dictionary<CharacterState, bool> allowedStates = new();

    protected virtual void Awake()
    {
        StateMachine.DefaultState = defaultState;
        allowedActionPriorities.Add(CharacterActionPriority.Hitstun, true);
        allowedActionPriorities.Add(CharacterActionPriority.High, true);
        allowedActionPriorities.Add(CharacterActionPriority.Medium, true);
        allowedActionPriorities.Add(CharacterActionPriority.Low, true);
        allowedActionPriorities.Add(CharacterActionPriority.Move, true);
    }

    public virtual void SetActionPriorityAllowed(CharacterActionPriority priority, bool isAllowed)
    {
        allowedActionPriorities[priority] = isAllowed;
    }

    public virtual void SetActionPriorityAllowed(CharacterActionPriority priority, float delay)
    {
        StartCoroutine(SetPriorityDelayed(priority, delay));
    }

    IEnumerator SetPriorityDelayed(CharacterActionPriority priority, float delay)
    {
        yield return new WaitForSeconds(delay);
        allowedActionPriorities[priority] = true;
    }

    public virtual void SetStateAllowed(CharacterState state, bool isAllowed)
    {
        allowedStates[state] = isAllowed;
    }

    public virtual void SetAllActionPriorityAllowed(bool b)
    {
        foreach (var key in new List<CharacterActionPriority>(allowedActionPriorities.Keys))
        {
            allowedActionPriorities[key] = b;
        }
    }

    public virtual void SetAllActionPriorityAllowed(bool b, float delay)
    {
        StartCoroutine(SetAllPriorityDelayed(b, delay));
    }

    IEnumerator SetAllPriorityDelayed(bool b, float delay)
    {
        yield return new WaitForSeconds(delay);
        SetAllActionPriorityAllowed(b);
    }

    public virtual void SetAllStatesAllowed(bool b)
    {
        foreach (var key in new List<CharacterState>(allowedStates.Keys))
        {
            allowedStates[key] = b;
        }
    }

    public virtual void SetAllActionPriorityAllowedExceptHitstun(bool b)
    {
        foreach (var key in new List<CharacterActionPriority>(allowedActionPriorities.Keys))
        {
            if (key != CharacterActionPriority.Hitstun) allowedActionPriorities[key] = b;
        }
    }

    public abstract void Hitstun();

    public abstract void EndHitStun();

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref anim);
        gameObject.GetComponentInParentOrChildren(ref character);
    }
#endif
}
