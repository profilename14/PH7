using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterMovementController : MonoBehaviour
{
    [SerializeField]
    private float baseMoveSpeed;

    [SerializeField]
    private List<float> speedModifiers = new List<float>();

    [SerializeField]
    private bool _CanMove;
    public bool canMove => _CanMove;

    public abstract void AddVelocity(float velocity);

    public abstract void SetVelocity(float velocity);

    protected virtual void SetPosition(Vector3 position)
    {
        this.gameObject.transform.position = position;
    }

    public abstract void AddSpeedModifier(float modifier);

    public abstract void RemoveSpeedModifier(float modifier);
}
