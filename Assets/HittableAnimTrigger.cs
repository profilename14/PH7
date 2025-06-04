using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Animancer;

public class HittableAnimTrigger : MonoBehaviour, IHittable
{
    [SerializeField]
    AnimancerComponent anim;

    [SerializeField]
    ClipTransition animation;

    [SerializeField]
    bool twoWayAnimation;

    bool hitFlag;

    [SerializeField]
    ClipTransition animationReverse;

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        if (!twoWayAnimation && hitFlag) return;

        if (twoWayAnimation && hitFlag)
        {
            anim.Play(animationReverse);
        }
        else anim.Play(animation);

        hitFlag = !hitFlag;
    }

    public virtual void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        return; // Should a bubble pop if hit by projectile?
    }

    public virtual void Hit(ColliderEffectField effectField, float damage)
    {
        return; // Should a bubble pop if hit by effect field?
    }
}
