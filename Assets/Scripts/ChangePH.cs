using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePH : MonoBehaviour
{
    [SerializeField] private float changeInPH;
    [SerializeField] private float maxLifespan = 5;
    private float curLifespan;
    [SerializeField] private bool permanent;

    void Start() {
      curLifespan = maxLifespan;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<PlayerStats>().ph += changeInPH * Time.deltaTime;
            if (!permanent) {
              curLifespan -= Time.deltaTime;
              if (curLifespan < 0) {
                Destroy(gameObject);
              }
            }


        } // else if its an enemy todo
    }

}
