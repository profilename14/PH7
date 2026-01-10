using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestKillPlane : MonoBehaviour
{
    [SerializeField]
    public Transform playerSpawnPoint;

    [SerializeField]
    public ColliderEffectField effectField;

    private float respawnTimer = 0.25f;
    private bool respawnActive = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (respawnActive)
        {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0)
            {
                respawnActive = false;
                respawnTimer = 0.25f;
                Respawn();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            //Player.instance.Hit(effectField, effectField.damageOnEnter);
            if (!respawnActive)
            {
                respawnActive = true;
                respawnTimer = 0.25f;
                Player.instance.playerActionManager.UIManager.loadingScreen.fadeToBlackFall();
            }
        }
    }

    private void Respawn()
    {
        PlayerMovementController mc = (PlayerMovementController)Player.instance.movementController;
        mc.TeleportTo(playerSpawnPoint.position);
    }
}
