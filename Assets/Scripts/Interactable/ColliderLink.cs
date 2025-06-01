using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderLink : MonoBehaviour
{
    // Mainly exists to store this collider so that dashing disables the main collision
    // Used by Gates and PH Doors so that the physical collision can be disabled without
    // disabling the trigger.
    public bool isDashable = false;
    public bool usesSoapstones = false;
    public float minPH = 0.0f;
    public float maxPH = 14.0f;
    public float soapstonesRequired = 0;
    [SerializeField] public Collider linkedCollider;
}
