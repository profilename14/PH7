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
    protected AnimancerComponent _Anim;
    public AnimancerComponent anim => _Anim;

    [SerializeField]
    protected Character _Character;

    [SerializeField]
    protected CharacterState _Idle;

    public readonly StateMachine<CharacterState>.WithDefault StateMachine = new();

    // A set of different priorities that generic actions can fall under.
    protected Dictionary<CharacterActionPriority, bool> _AllowedActionPriorities = new();
    public Dictionary<CharacterActionPriority, bool> allowedActionPriorities => _AllowedActionPriorities;

    // A set of different states that specific actions can fall under.
    protected Dictionary<CharacterState, bool> _AllowedStates = new();
    public Dictionary<CharacterState, bool> allowedStates => _AllowedStates;

    protected virtual void Awake()
    {
        StateMachine.DefaultState = _Idle;
        _AllowedActionPriorities.Add(CharacterActionPriority.Hitstun, true);
        _AllowedActionPriorities.Add(CharacterActionPriority.High, true);
        _AllowedActionPriorities.Add(CharacterActionPriority.Medium, true);
        _AllowedActionPriorities.Add(CharacterActionPriority.Low, true);
        _AllowedActionPriorities.Add(CharacterActionPriority.Move, true);
    }

    public virtual void SetActionPriorityAllowed(CharacterActionPriority priority, bool isAllowed)
    {
        _AllowedActionPriorities[priority] = isAllowed;
    }

    public virtual void SetActionPriorityAllowed(CharacterActionPriority priority, float delay)
    {
        StartCoroutine(SetPriorityDelayed(priority, delay));
    }

    IEnumerator SetPriorityDelayed(CharacterActionPriority priority, float delay)
    {
        yield return new WaitForSeconds(delay);
        _AllowedActionPriorities[priority] = true;
    }

    public virtual void SetStateAllowed(CharacterState state, bool isAllowed)
    {
        _AllowedStates[state] = isAllowed;
    }

    public virtual void SetAllActionPriorityAllowed(bool b)
    {
        foreach (var key in new List<CharacterActionPriority>(_AllowedActionPriorities.Keys))
        {
            _AllowedActionPriorities[key] = b;
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
        foreach (var key in new List<CharacterState>(_AllowedStates.Keys))
        {
            _AllowedStates[key] = b;
        }
    }

    public virtual void SetAllActionPriorityAllowedExceptHitstun(bool b)
    {
        foreach (var key in new List<CharacterActionPriority>(_AllowedActionPriorities.Keys))
        {
            if (key != CharacterActionPriority.Hitstun) _AllowedActionPriorities[key] = b;
        }
    }

    public abstract void Hitstun();

    public abstract void EndHitStun();

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _Anim);
        gameObject.GetComponentInParentOrChildren(ref _Character);
    }
#endif
}
