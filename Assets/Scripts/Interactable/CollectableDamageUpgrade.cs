using System.Collections;
using System.Collections.Generic;
using Animancer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CollectableDamageUpgrade : MonoBehaviour
{
    [SerializeField] bool healsPlayer = false;
    [SerializeField] int collectableDamageID = 0; // must be unique and manually set, collectables with duplicate ids wont respawn
    private int sceneID = 0;

    void Start()
    {
        sceneID = SceneManager.GetActiveScene().buildIndex;

        if (GameManager.instance.collectablesDamageObtained[sceneID].TryGetValue(collectableDamageID, out bool isCollected))
        {
            if (isCollected == true)
            {
                Destroy(gameObject);
            }
        }
        else // first load of this scene
        {
            GameManager.instance.collectablesDamageObtained[sceneID][collectableDamageID] = false;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if (healsPlayer)
            {
                Player.instance.stats.SetHealth(Player.instance.characterData.maxHealth);
            }

            GameManager.instance.damageUpgrade += 1;
            
            GameManager.instance.collectablesDamageObtained[sceneID][collectableDamageID] = true;
            Destroy(gameObject);
        } 
    }
}
