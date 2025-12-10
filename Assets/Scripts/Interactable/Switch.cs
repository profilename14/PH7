using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;



public class Switch : MonoBehaviour
{
    bool isToggled = false;
    [SerializeField] ActivatedObject objectToActivate;

    public void Toggle()
    {
        if (isToggled == false) {
            isToggled = true;
        }
        objectToActivate.Activate();

        Debug.Log("Activated object!");
    }
}
