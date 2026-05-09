using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectableLapis : MonoBehaviour
{
    [SerializeField] bool healsPlayer = false;
    [SerializeField] int collectableID = 0; // must be unique and manually set, collectables with duplicate ids wont respawn
    private int sceneID = 0;

    [SerializeField] float fallSpeed;

    [SerializeField]
    UnityEvent onPickup;

    bool closeToGround;

    void Start()
    {
        /*sceneID = SceneManager.GetActiveScene().buildIndex;

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
        }*/

    }

    private void FixedUpdate()
    {
        if(fallSpeed > 0 && !closeToGround) transform.position -= new Vector3(0, fallSpeed, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            onPickup.Invoke();
            GameManager.instance.lapis += 1;
            Player.instance.uiManager.UpdateLapis(GameManager.instance.lapis);
            print("Lapis collected: " + GameManager.instance.lapis);
            Destroy(gameObject);
        }
        else if(other.gameObject.layer == 18 || other.gameObject.layer == 0)
        {
            closeToGround = true;
        }
    }
}
