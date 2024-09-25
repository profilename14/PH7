using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float lifespan = 4.5f;
    [SerializeField] private float lifespanTimer = 0f;
    [SerializeField] private float speed = 10f;


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
