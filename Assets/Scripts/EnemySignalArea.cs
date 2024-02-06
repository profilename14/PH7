using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySignalArea : MonoBehaviour
{

    public List<GameObject> enemiesToSignal; // not an enemy behavior list so we can tell their names in the inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            foreach(GameObject enemy in enemiesToSignal) {
              if (enemy != null) {
                enemy.GetComponent<EnemyBehavior>().AlertEnemy();
              }
            }
        }
    }
}
