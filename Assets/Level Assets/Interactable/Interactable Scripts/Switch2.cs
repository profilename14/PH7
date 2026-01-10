using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class Switch2 : MonoBehaviour, IHittable
{
    public bool isToggled = false;
    [SerializeField] ActivatedObject objectToActivate;

    [SerializeField] UnityEvent onToggleOn;

    [SerializeField] UnityEvent onToggleOff;

    [SerializeField]
    Animator anim;

    [SerializeField]
    bool allowToggleBack;

    public void Toggle()
    {
        if (isToggled == false)
        {
            isToggled = true;
            anim.Play("Lever Pull");

            onToggleOn.Invoke();

            if (objectToActivate)
            {
                objectToActivate.Activate();
            }
        }
        else if(allowToggleBack)
        {
            isToggled = false;
            anim.Play("Lever Unpull");

            onToggleOff.Invoke();
        }



        //Debug.Log("Activated object!");
    }

    public void Hit(AttackState attack, Vector3 hitPoint)
    {
        Toggle();
    }
    
    public void Hit(MyProjectile projectile, Vector3 hitPoint)
    {
        Toggle();
    }

    public void Hit(ColliderEffectField colliderEffectField, float damage)
    {
        return;
    }
}
