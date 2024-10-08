using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SaltShield : MonoBehaviour
{
    [SerializeField] private float maxLifespan = 0.10f;
    private float curLifespan;
    private float deltaPhysics = 0.02f; // on trigger stay is always called 50 times a second
    public PlayerStats playerStats;
    private bool alreadyHealed = false;


    void Start() {
        curLifespan = maxLifespan;

    }

    void Update()
    {

        
        curLifespan -= Time.deltaTime;

        if (curLifespan < 0) {
            Destroy(gameObject);
        }


    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Projectile")) {

            Destroy(other);
            if (alreadyHealed || playerStats == null) {
                return;
            }
            alreadyHealed = true;

            
            if (playerStats.health >= playerStats.healthMax) {
                Debug.Log("Cannot heal past maximum HP");
                return;
            }
                
            playerStats.SetHealth(playerStats.health + 1); // Will ask about how to formally set health during the meeting, playerstats just changed recently.
            Debug.Log("Player health is now: " + playerStats.health);

            

        }
    }

}
