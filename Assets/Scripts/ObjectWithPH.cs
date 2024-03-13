using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
//using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static UnityEngine.GraphicsBuffer;

// Thomas Watson
// Enemy Behavior code


public class ObjectWithPH : MonoBehaviour
{
    public float StartPH;
    [HideInInspector] public float CurrentPH;
    public float RegenPH = 0.33f; // This much H regened toward default per second.
    protected float RegenPHTimer = 0.0f;
    protected float RegenPHCooldown = 2.0f; // How long after a pH attack regen is disabled
    public bool destroyedAtPH = false;
    public float deathPH;
    public bool acidic = false;

    private void Awake()
    {
        CurrentPH = StartPH;
    }


    void Update()
    {

        if (RegenPHTimer > 0) {
          RegenPHTimer -= Time.deltaTime;
        } else {
          if (CurrentPH < StartPH) {
            CurrentPH += RegenPH * Time.deltaTime;
            if (CurrentPH > StartPH) {
              CurrentPH = StartPH;
            }
          } else if (CurrentPH > StartPH) {
            CurrentPH -= RegenPH * Time.deltaTime;
            if (CurrentPH < StartPH) {
              CurrentPH = StartPH;
            }
          }
        }

    }


    public void ChangePH(float ph)
    {

        CurrentPH += ph;

        if (CurrentPH > 14) {
          CurrentPH = 14;
        } else if (CurrentPH < 0) {
          CurrentPH = 0;
        }

        if (ph != 0) {
          RegenPHTimer = RegenPHCooldown;
        }

        if (destroyedAtPH) {
          if (CurrentPH >= deathPH && acidic) {
            Destroy(gameObject);
          } else if (CurrentPH <= deathPH && !acidic) {
            Destroy(gameObject);
          }
        }

    }

    public void NeutralizePH(float target) {
      if (CurrentPH > target) {
        float dif = CurrentPH - target;

        CurrentPH = CurrentPH - 0.4f * dif;
      } else {
        float dif = target - CurrentPH;

        CurrentPH = CurrentPH + 0.4f * dif;
      }
    }

}
