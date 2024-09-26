using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterMovementController
{
    public void AddVelocity(Vector3 velocity);

    public void SetVelocity(Vector3 velocity);

    public void AddSpeedModifier(float modifier);

    public void RemoveSpeedModifier(float modifier);

    public void ApplyForce(Vector3 source, float power);

    public bool IsGrounded();
}
