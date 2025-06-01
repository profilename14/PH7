using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.GraphicsBuffer;



public class Switch2 : MonoBehaviour, IHittable
{
    public bool isToggled = false;
    [SerializeField] ActivatedObject objectToActivate;

    public void Toggle()
    {
        if (isToggled == false)
        {
            isToggled = true;
        }
        if (objectToActivate)
        {
            objectToActivate.Activate();
        }


        Debug.Log("Activated object!");
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
