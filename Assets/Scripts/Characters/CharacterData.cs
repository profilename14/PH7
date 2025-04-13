using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class CharacterData : ScriptableObject
{
    // In the case of the player, this should be their starting max health and not their current max health.
    public float maxHealth;
    // How fast is this character's max movement speed without any modifiers, in units/sec?
    public float maxBaseMoveSpeed;
    // How fast can this character accelerate their movement, without any modifiers?
    public float baseMoveAcceleration;
    // What multiplier does this character divide all knockback it receives by?
    public float knockbackResistance;
    // How fast is this character's max rotation speed without any modifiers, in degrees/sec?
    public float rotationSpeed;
    // The natural Chemical type of this character. Will be used for Chemical reactions, etc.
    public Chemical naturalType;
}
