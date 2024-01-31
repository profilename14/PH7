using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseableWallController : MonoBehaviour
{
    // just exists to store this collider so that dashing disables the main collision
    [SerializeField] public Collider linkedCollider;
}
