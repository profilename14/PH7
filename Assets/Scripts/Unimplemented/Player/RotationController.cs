using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;

// Made by Jasper Fadden
// REsponsible for hving Typhis look at the mouse.


public class RotationController : MonoBehaviour
{
  public Vector3 directionVec;

  public bool isFacingMouse = false;
  public bool controllerBufferLock = false;

  private PlayerMovementController movementController;

  void Awake() {
    
    movementController = transform.parent.gameObject.GetComponent<PlayerMovementController>();
  }

  // Rotation controller handles game manager stuff
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.C))
    {
      if (!GameManagerOLD.isControllerUsed)
      {
        GameManagerOLD.isControllerUsed = true;
      }
      else
      {
        GameManagerOLD.isControllerUsed = false;
      }
    }

    /*if (Input.GetButton("Jump") && !Input.GetKeyDown(KeyCode.Space)) {
      GameManager.isControllerUsed = true;
    }*/

        if (Input.GetKeyDown(KeyCode.K))
    {
      if (!GameManagerOLD.isScreenshakeEnabled)
      {
        GameManagerOLD.isScreenshakeEnabled = true;
      }
      else
      {
        GameManagerOLD.isScreenshakeEnabled = false;
      }
    }

    if (GameManagerOLD.slowTimer > 0)
    {
      GameManagerOLD.slowTimer -= Time.deltaTime;

    }
    if (GameManagerOLD.slowTimer <= 0 && Time.timeScale < 1f)
    {
      Time.timeScale = 1f;
    }
  }

  public Vector3 GetRotationDirection() {
        return Vector3.zero;//movementController.GetMouseDirection();
  }

  public void snapToCurrentMouseAngle() {
    // THIS IS TEMPORARY!
    // Remove this and the rest of the script when all occurnces of snapToCurrentMouseAngle are replaced with rotateToMouse
    //movementController.RotateToDir();
    movementController.SetAllowRotation(false);
  }


}
