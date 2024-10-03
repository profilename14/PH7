using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackState : CharacterState
{
    [SerializeField]
    private AttackData _AttackData;
    public AttackData attackData => _AttackData;

    public virtual void OnAttackHit(Vector3 position)
    {
        return;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If this script is disabled, then the player is not in this attack state and nothing should happen.
        if (this.enabled == false) return;

        // Check if we have collided with a hittable object.
        IHittable hittableScript = other.gameObject.GetComponent<IHittable>();
        if (hittableScript == null) return;

        // In the case of the player, you are hitting your own hitbox.
        // In the case of an Enemy, they are either hitting their own hitbox, or a hitbox of an ally Enemy.
        if (_Character.GetType() == hittableScript.GetType()) return;

        Vector3 attackHitPosition = other.ClosestPointOnBounds(_Character.transform.position);

        hittableScript.Hit(this, attackHitPosition);
        OnAttackHit(attackHitPosition);
        _Character.OnCharacterAttackHit(hittableScript, this, attackHitPosition);
    }

    // A ranged attack should pass _AttackData to the projectile, which will handle OnTriggerEnter when it hits something.
}