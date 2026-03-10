using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableLapis : MonoBehaviour
{
    [SerializeField] bool healsPlayer = false;
    [SerializeField] int collectableID = 0; // must be unique and manually set, collectables with duplicate ids wont respawn
    private int sceneID = 0;

    [SerializeField] float fallSpeed;

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
        if(fallSpeed > 0) transform.position -= new Vector3(0, fallSpeed, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.instance.lapis += 1;
            Player.instance.uiManager.UpdateLapis(GameManager.instance.lapis);
            print("Lapis collected: " + GameManager.instance.lapis);
            Destroy(gameObject);
        }
    }
}
