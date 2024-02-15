using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Used by the neutral rain Typhis can summon. Acts similar to hazards.
public class RainSpell : MonoBehaviour
{
    [SerializeField] private float changeInHP = -5;
    [SerializeField] private float balancedChangeInHP = -12.5f;
    [SerializeField] private float maxLifespan = 5;
    private float curLifespan;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second
    public PlayerStats playerStats;
    private float playerPH = 14;


    void Start() {
      curLifespan = maxLifespan;
    }

    void Update() {
      curLifespan -= Time.deltaTime;
      if (curLifespan < 0) {

        if (playerStats != null) {
          playerStats.rainPower = false;
        }
        Destroy(gameObject);
      }
      if (playerStats != null) {
        playerPH = playerStats.ph;
      }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Enemy") {
          // Ensure this doesn't cause I frames later
          if (5.5f < playerPH && playerPH < 9.5f) {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
              -balancedChangeInHP * deltaPhysics, 0f, 0f, new Vector3(0,0,0));
          } else {
            other.gameObject.GetComponent<EnemyBehavior>().TakeDamage(
              -changeInHP * deltaPhysics, 0f, 0f, new Vector3(0,0,0));
          }

        }
    }


    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<PlayerStats>().rainPower = true;
        }
    }
    void OnTriggerExit (Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.GetComponent<PlayerStats>().rainPower = false;
        }
    }
}
