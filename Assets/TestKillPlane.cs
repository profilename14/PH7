using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestKillPlane : MonoBehaviour
{
    [SerializeField]
    public Transform playerSpawnPoint;

    [SerializeField]
    public ColliderEffectField effectField;

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
            Player.instance.Hit(effectField, effectField.damageOnEnter);
            PlayerMovementController mc = (PlayerMovementController)Player.instance.movementController;
            mc.TeleportTo(playerSpawnPoint.position);
        }
    }
}
