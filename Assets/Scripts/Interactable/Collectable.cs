using System.Collections;
using System.Collections.Generic;
using Animancer;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Collectable : MonoBehaviour
{
    [SerializeField] bool healsPlayer = false;
    [SerializeField] int collectableID = 0; // must be unique and manually set, collectables with duplicate ids wont respawn
    private int sceneID = 0;

    void Start()
    {
        sceneID = SceneManager.GetActiveScene().buildIndex;

        if (GameManager.instance.collectablesObtained[sceneID].TryGetValue(collectableID, out bool isCollected))
        {
            if (isCollected == true)
            {
                Destroy(gameObject);
            }
        }
        else // first load of this scene
        {
            GameManager.instance.collectablesObtained[sceneID][collectableID] = false;
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if (healsPlayer)
            {
                Player.instance.stats.SetHealth(Player.instance.characterData.maxHealth);
                GameManager.instance.soapstones += 1;
                print("Soapstones collected: " + GameManager.instance.soapstones);
            }
            GameManager.instance.collectablesObtained[sceneID][collectableID] = true;
            Destroy(gameObject);
        } 
    }
}
