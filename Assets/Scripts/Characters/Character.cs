using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;
using Animancer.FSM;

public abstract class Character : MonoBehaviour
{
    // References to other core components and scripts.
    [SerializeField]
    private CharacterActionManager _ActionManager;
    public CharacterActionManager actionManager => _ActionManager;
    [SerializeField]
    private ICharacterMovementController _MovementController;
    public ICharacterMovementController movementController => _MovementController;
    [SerializeField]
    private CharacterStats _Stats;
    public CharacterStats stats => _Stats;

#if UNITY_EDITOR
    void OnValidate()
    {
        gameObject.GetComponentInParentOrChildren(ref _ActionManager);
        gameObject.GetComponentInParentOrChildren(ref _MovementController);
        gameObject.GetComponentInParentOrChildren(ref _Stats);
    }
#endif
}
