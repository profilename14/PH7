using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TyphisSpawnOverride : MonoBehaviour
{
    public static TyphisSpawnOverride instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
    }

    private void Start()
    {
        GameManager.instance.respawnPosition = this.transform.position;
        PlayerMovementController p = (PlayerMovementController) Player.instance.movementController;
        p.TeleportTo(this.transform.position);
    }
}
