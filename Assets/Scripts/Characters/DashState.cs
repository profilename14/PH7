using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DashState : CharacterState
{

    [SerializeField]
    protected float distance;
    
    [SerializeField]
    protected float duration;

    public abstract void endDash();

}