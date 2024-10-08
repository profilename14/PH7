using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterSpell : CharacterState
{

    [SerializeField]
    private float _AcidCost;
    public float acidCost => _AcidCost;

    [SerializeField]
    private float _AlkalineCost;
    public float alkalineCost => _AlkalineCost;


}