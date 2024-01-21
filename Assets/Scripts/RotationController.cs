using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Made by Jasper Fadden
// REsponsible for hving Typhis look at the mouse.

// Credits to Josh McCoy for much of the code implementing Attack-Decay-Sustain-Release movement.

public class RotationController : MonoBehaviour
{
  // Used to store what angle clockwise the player's mouse is relative to the player.
  public float angle;
  public Vector3 directionVec;
  Vector3 mousePosition;

  private float distanceMouseIs = 0.0f;
  private Vector3 Direction3D;

  bool isControllerUsed = false;

  void Update() {
  if (Input.GetKeyDown(KeyCode.C))
    {
      if (!GameManager.isControllerUsed)
      {
          GameManager.isControllerUsed = true;
      }
      else {
          GameManager.isControllerUsed = false;
      }
    }
  }

  public Vector3 GetRotationDirection() {
    Vector3 dir;
    if (GameManager.isControllerUsed) {
      dir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
      dir.Normalize();
    } else {
      float h = Input.mousePosition.x - Screen.width / 2;
      float v = Input.mousePosition.y - Screen.height / 2;
      dir = new Vector3(h, 0, v);
      dir.Normalize();
    }
    return dir;
  }


  void FixedUpdate() {
    /*mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    Vector2 direction = mousePosition - transform.position;
    // This is used to detect where the player should be facing.
    angle = Vector2.SignedAngle(Vector2.down, direction) + 270;

    transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle);*/
    if (GameManager.isControllerUsed)
    {
      Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
      angle = Mathf.Atan2(input.x,input.y) * Mathf.Rad2Deg;
      directionVec = new Vector3(input.x, 0, input.y);
      directionVec.Normalize();

      if ( Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2 || Mathf.Abs(Input.GetAxis("Vertical")) > 0.2) {
        transform.rotation = Quaternion.Euler (0, angle, 0);
      }

    }
    else {
      float h = Input.mousePosition.x - Screen.width / 2;
      float v = Input.mousePosition.y - Screen.height / 2;
      directionVec = new Vector3(h, 0, v);
      directionVec.Normalize();

      float angle = -Mathf.Atan2(v,h) * Mathf.Rad2Deg;

      transform.rotation = Quaternion.Euler (0, angle, 0);
    }


  }


}
