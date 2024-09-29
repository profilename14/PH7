using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterMovementController
{
    public void AddVelocity(Vector3 velocity);

    public void SetVelocity(Vector3 velocity);

    public void LockVelocity(Vector3 velocity);

    public void UnlockVelocity();

    public void AddSpeedModifier(float modifier);

    public void RemoveSpeedModifier(float modifier);

    public void ApplyImpulseForce(Vector3 direction, float power);

    public bool IsGrounded();

    public void SetAllowMovement(bool isAllowed);

    public void SetAllowRotation(bool isAllowed);
}
