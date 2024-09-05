using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class PlayerActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    NamedAnimancerComponent playerAnim;
    [SerializeField]
    MovementController moveController;
    [SerializeField]
    RotationController rotController;
    [SerializeField]
    AudioSource soundEffects;
    
    // There is a seperate PlayerActionState for each type of input that can be received from the player.
    // This mostly matches up with the player's Animation Controller states.
    public enum PlayerActionState { Idle, Moving, Attacking, Dashing, Interacting, Bubble, ChargeAttacking, SpellAttacking }
    
    [Header("State")]
    // The current action state of the player.
    PlayerActionState playerActionState;
    
    // If the current action can be interrupted by a different action.
    // This is enabled for the Idle or Moving states' full duration, but other actions have periods of non-interruptability.
    [SerializeField]
    bool actionIsInterruptible;

    // Use this flag to disable the ability for the player to perform all actions (for cutscenes, etc.)
    [SerializeField]
    bool playerCanAct = true;

    void Start()
    {
        playerActionState = PlayerActionState.Idle;
    }

    void Update()
    {
        // The Idle and Moving states are always interruptable.
        if(playerActionState == PlayerActionState.Idle || playerActionState == PlayerActionState.Moving)
        {
            actionIsInterruptible = true;
        }

        switch (playerActionState)
        {
            case PlayerActionState.Idle:

                break;

            case PlayerActionState.Moving:

                break;
            case PlayerActionState.Attacking:
                break;
            case PlayerActionState.Dashing:
                break;
            case PlayerActionState.Interacting:
                break;
            case PlayerActionState.Bubble:
                break;
            case PlayerActionState.ChargeAttacking:
                break;
            case PlayerActionState.SpellAttacking:
                break;
            default:
                break;
        }
    }

    public void Play()
    {

    }

    public PlayerActionState GetActionState()
    {
        return playerActionState;
    }

    #region Animation Event Functions
    // This function should be called by the first frame of every Animation that is the first one to be played of that state.
    public void UpdatePlayerState(PlayerActionState state)
    {
        playerActionState = state;
    }

    // This function should be called by an Animation to set if the action is interruptible for the following frames.
    public void SetActionInterruptible(bool isInterruptible)
    {
        actionIsInterruptible = isInterruptible;
    }
    #endregion
}
