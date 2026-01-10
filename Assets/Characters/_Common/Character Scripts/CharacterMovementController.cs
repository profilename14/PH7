using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterMovementController : MonoBehaviour
{
    public abstract Vector3 GetVelocity();

    public abstract void AddVelocity(Vector3 velocity);

    public abstract void SetVelocity(Vector3 velocity);

    public abstract void LockVelocity(Vector3 velocity);

    public abstract void UnlockVelocity();

    public abstract void AddSpeedModifier(float modifier);

    public abstract void RemoveSpeedModifier(float modifier);

    public abstract void ApplyImpulseForce(Vector3 direction, float power);

    public abstract bool IsGrounded();

    public abstract void SetAllowMovement(bool isAllowed);

    public abstract void SetAllowRotation(bool isAllowed);

    public abstract void SetGroundDrag(float drag);
}
