using System.Collections;
using System.Collections.Generic;
using Animancer;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CollectableUpgrade : MonoBehaviour
{
    [SerializeField] bool healsPlayer = false;
    //[SerializeField] int collectableID = 0; // must be unique and manually set, collectables with duplicate ids wont respawn
    private int sceneID = 0;
    public bool unlockDash;
    public bool unlockBubble;


    void Start()
    {
        sceneID = SceneManager.GetActiveScene().buildIndex;

        if (GameManager.instance.dashUnlocked && unlockDash)
        {
            Destroy(gameObject);
        }
        if (GameManager.instance.bubbleUnlocked && unlockBubble)
        {
            Destroy(gameObject);
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (healsPlayer)
            {
                Player.instance.stats.SetHealth(Player.instance.characterData.maxHealth);
            }

            if (unlockDash)
            {
                GameManager.instance.dashUnlocked = true;

                Player.instance.playerActionManager.UnlockDash();
            }
            if (unlockBubble)
            {
                GameManager.instance.bubbleUnlocked = true;
                Player.instance.playerActionManager.UnlockBubble();
            }
        

            Destroy(gameObject);
        } 
    }
}
