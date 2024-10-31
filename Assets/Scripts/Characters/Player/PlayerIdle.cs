using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer.FSM;
using Animancer;

public class PlayerIdle : CharacterState
{
    // Probably won't do anything significant except hold the CharacterState fields
    // and maybe do some VFX for the idle state.

    [SerializeField]
    TransitionAsset idleAnimation;

    protected override void OnEnable()
    {
        _ActionManager.anim.Play(idleAnimation);
    }
}
