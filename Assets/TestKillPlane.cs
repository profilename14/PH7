using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestKillPlane : MonoBehaviour
{
    [SerializeField]
    Vector3 playerSpawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            PlayerMovementController mc = (PlayerMovementController) other.gameObject.GetComponent<Player>().movementController;
            mc.TeleportTo(playerSpawnPoint);
        }
    }
}
