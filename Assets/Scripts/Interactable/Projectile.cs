using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifespan = 4.5f;
    [SerializeField] private float lifespanTimer = 0f;
    [SerializeField] private float speed = 50f;

    [SerializeField]
    private AttackState _AttackState;


    public Character sender; // Set by a seperate script to let the projectile know who fired it.


    // Start is called before the first frame update
    void Start()
    {
      lifespanTimer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifespanTimer += Time.deltaTime;
        if (lifespanTimer > lifespan) {
          Destroy(gameObject);
        }
        transform.position += this.transform.forward * Time.deltaTime * speed;
    }

    void OnTriggerEnter(Collider other)
    {
        if (this.enabled == false) return;

         // Check if we have collided with a hittable object.
        IHittable hittableScript = other.gameObject.GetComponent<IHittable>();
        if (hittableScript == null) return;

        // In the case of the player, you are hitting your own hitbox.
        // In the case of an Enemy, they are either hitting their own hitbox, or a hitbox of an ally Enemy.
        if (sender.GetType() == hittableScript.GetType()) return;

        hittableScript.Hit(_AttackState, transform.position);
        Debug.Log("Hit with projectile!");
        //sender.OnCharacterAttackHit(hittableScript, this); // Might be reserved for melee

        /*
        if(other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<PlayerStatsOLD>().playerDamage(1, 0, transform.position, 5f);
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("Switch")) {
            if (other.gameObject.GetComponent<Switch>() != null) {
              other.gameObject.GetComponent<Switch>().Toggle(); // Could be neat for puzzles if you have to get an enemy to shoot a switch
            }
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("AllowsBubble") || other.gameObject.CompareTag("Enemy")) {
            //
        } else {
          //Destroy(gameObject);
        }*/

    }
    void OnColliderEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerStatsOLD>().playerDamage(1, 0, transform.position, 5f);
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("Switch")) {
            if (other.gameObject.GetComponent<Switch>() != null) {
              other.gameObject.GetComponent<Switch>().Toggle(); // Could be neat for puzzles if you have to get an enemy to shoot a switch
            }
            Destroy(gameObject);
        } else if (other.gameObject.CompareTag("AllowsBubble") || other.gameObject.CompareTag("Enemy")) {
            //
        } else {
          //Destroy(gameObject);
        }

    }
}
