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
    public float RegenPH = 0.33f; // This much pH regened toward default per second.
    protected float RegenPHTimer = 0.0f;
    protected float RegenPHCooldown = 2.0f; // How long after a pH attack regen is disabled
    public bool destroyedAtPH = false;
    public float deathPH;
    public bool acidic = false;
    public float changeRatePH = 1;
    public bool slowsOnChangePH = false;
    private Rigidbody rigid;
    public bool canBeAttacked = false;
    public float phOnHit = 0;
    public GameObject particles;
    [SerializeField] public AudioSource audioSource;
    [SerializeField] private AudioClip soundEffect;

    private void Awake()
    {
        CurrentPH = StartPH;
        if (slowsOnChangePH) {
          rigid = gameObject.GetComponent<Rigidbody>();
        }
        audioSource = GameObject.FindGameObjectWithTag("Sound").GetComponent<AudioSource>();
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

        if (slowsOnChangePH){
          rigid.velocity = rigid.velocity * 0.9f;
        }

        CurrentPH += ph * changeRatePH;

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

    public void instantiateParticles() {
      
      if (particles != null) {
        Instantiate(particles, transform.position, Quaternion.identity);
        if (soundEffect != null) {
          audioSource.PlayOneShot(soundEffect, 0.35F);
        }
        
      }
    }



}
